// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open System.Threading.Tasks
open IntelliJUpdater
open IntelliJUpdater.Versioning
open TruePath

type TaskResult =
    | HasChanges of {|
            BranchName: string
            CommitMessage: string
            PrTitle: string
            PrBodyMarkdown: string
        |}
    | NoChanges

type IdeBuildSpec = {
    IdeVersions: Map<string, IdeVersion>
    KotlinVersion: Version
    UntilVersion: string
}

type StoredEntityVersion = {
    File: LocalPath
    Field: string
    Update: EntityVersion
}

let private ReadLatestSpec(update: Update): Task<EntityVersion> =
    match update.Kind with
    | Ide ide ->
        task {
            let! ideVersion = Ide.ReadLatestVersion ide update.VersionFlavor update.VersionConstraint
            return EntityVersion.Ide ideVersion
        }
    | Kotlin -> task {
        let! ideVersion = Ide.ReadLatestVersion IdeKind.IntelliJIdeaCommunity update.VersionFlavor update.VersionConstraint
        let kotlinVersion = Kotlin.ForIde ideVersion.Wave
        return EntityVersion.Kotlin kotlinVersion
    }

let ReadLatestSpecs(config: Configuration): Task<StoredEntityVersion[]> = task {
    let results = ResizeArray()
    for update in config.Updates do
        let! entityVersion = ReadLatestSpec update
        results.Add {
            File = update.File
            Field = update.Field
            Update = entityVersion |> Augmentations.Augment update.Augmentation
        }
    return Array.ofSeq results
}

let private ReadValue (filePath: LocalPath) (key: string) =
    match filePath.GetExtensionWithoutDot() with
    | "toml" -> TomlFile.ReadValue filePath key
    | "properties" -> PropertiesFile.ReadValue filePath key
    | other -> failwithf $"Unknown file extension: \"{other}\"."

let private ReadVersion(update: Update): Task<StoredEntityVersion> = task {
    let! version =
        match update.Kind, update.Augmentation with
        | Ide _, None ->
            task {
                let! text = ReadValue update.File update.Field
                let version = IdeVersion.Parse text
                return EntityVersion.Ide version
            }
        | Ide _, Some NextMajor ->
            task {
                let! text = ReadValue update.File update.Field
                let waveNumber = text.Replace(".*", "") |> int
                return EntityVersion.NextMajor waveNumber
            }
        | Kotlin, None ->
            task {
                let! text = ReadValue update.File update.Field
                let version = Version.Parse text
                return EntityVersion.Kotlin version
            }
        | x, y -> failwithf $"Unsupported update kind and augmentation: {x} with {y}."

    return {
        File = update.File
        Field = update.Field
        Update = version
    }
}

let ReadCurrentSpecs(config: Configuration) = task {
    return! config.Updates |> Array.map ReadVersion |> Task.WhenAll
}

let WriteUntilVersion (version: string) propertiesFilePath = task {
    let! properties = File.ReadAllTextAsync propertiesFilePath
    let re = Regex @"\r?\nuntilBuildVersion=(.*?)\r?\n"
    let newContent = re.Replace(properties, $"\nuntilBuildVersion={version}\n")
    do! File.WriteAllTextAsync(propertiesFilePath, newContent)
    return properties <> newContent
}

let private ApplyVersion (update: StoredEntityVersion): Task<bool> =
    let versionText =
        match update.Update with
        | EntityVersion.Ide version -> version.ToString()
        | EntityVersion.Kotlin version -> version.ToString()
        | EntityVersion.NextMajor waveNumber -> $"{waveNumber}.*"

    match update.File.GetExtensionWithoutDot() with
    | "toml" -> TomlFile.WriteValue update.File update.Field versionText
    | "properties" -> PropertiesFile.WriteValue update.File update.Field versionText
    | other -> failwithf $"Unknown file extension: \"{other}\"."

let ApplySpec (updates: StoredEntityVersion[]) = task {
    for version in updates do
        let! changed = ApplyVersion version
        if not changed then
            failwithf $"Cannot apply change to the configuration file: {version}."
}

let GenerateResult (config: Configuration) (localSpec: StoredEntityVersion[]) (remoteSpec: StoredEntityVersion[]) =
    let toMap specs =
        specs
        |> Seq.map(fun x -> KeyValuePair((x.File, x.Field), x))
        |> Dictionary

    let localMap = toMap localSpec
    let remoteMap = toMap remoteSpec

    let diff = localMap |> Seq.filter(fun kvp -> kvp.Value.Update <> remoteMap[kvp.Key].Update)

    let toString item =
        match item.Update with
        | EntityVersion.Ide version -> version.ToString()
        | EntityVersion.Kotlin version -> version.ToString()
        | EntityVersion.NextMajor waveNumber -> $"{waveNumber}.*"

    let message = String.concat "\n" [|
        for item in diff do
            let key = item.Key
            let old = localMap[key]
            let updated = remoteMap[key]

            let file, field = key
            $"- `{file}:{field}`: {toString old} -> {toString updated}"
    |]

    let preSection =
        config.PrBodyPrefix
        |> Option.map(fun x -> x + "\n\n")
        |> Option.defaultValue ""

    {|
        BranchName = "dependencies/intellij"
        CommitMessage = "Dependencies: update IntelliJ-based IDE versions"
        PrTitle = "IntelliJ Update"
        PrBodyMarkdown = $"""{preSection}## Version Updates
{message}
"""
    |}

let private FindChangedItems (localSpec: StoredEntityVersion[]) (remoteSpec: StoredEntityVersion[]) =
    let toMap specs =
        specs
        |> Seq.map(fun x -> KeyValuePair((x.File, x.Field), x))
        |> Dictionary

    let localMap = toMap localSpec
    let remoteMap = toMap remoteSpec

    remoteMap
    |> Seq.filter(fun kvp -> kvp.Value.Update <> localMap[kvp.Key].Update)
    |> Seq.map _.Value
    |> Seq.toArray

let processData configPath = task {
    let! config = Configuration.Read configPath
    let! latestSpec = ReadLatestSpecs config
    let! currentSpec = ReadCurrentSpecs config
    let diff = FindChangedItems currentSpec latestSpec
    if not <| Array.isEmpty diff then
        printfn "Changes detected."
        printfn $"Local spec: %A{currentSpec}."
        printfn $"Remote spec: %A{latestSpec}."
        printfn $"Changes: %A{diff}."
        do! ApplySpec diff
        return HasChanges <| GenerateResult config currentSpec latestSpec
    else
        printfn $"No changes detected: both local and remote specs are %A{latestSpec}."
        return NoChanges
}

let writeOutput result (out: LocalPath) = task {
    let! text =
        match result with
        | NoChanges -> Task.FromResult "has-changes=false"
        | HasChanges changes -> task {
            let prBodyMarkdownPath = Path.GetTempFileName()
            do! File.WriteAllTextAsync(prBodyMarkdownPath, changes.PrBodyMarkdown)
            return $"""has-changes=true
branch-name={changes.BranchName}
commit-message={changes.CommitMessage}
pr-title={changes.PrTitle}
pr-body-path={prBodyMarkdownPath}
"""
        }

    do! File.WriteAllTextAsync(out.Value, text.ReplaceLineEndings "\n")
    printfn $"{text}"
    printfn $"Result printed to \"{out}\"."
}

type Args = {
    ConfigPath: LocalPath
    OutputPath: LocalPath
}

let readArgs: string[] -> Args = function
| [| config; output |] -> { ConfigPath = LocalPath config; OutputPath = LocalPath output }
| _ -> failwith "Arguments expected: <config-file-path> <output-file-path>"

[<EntryPoint>]
let main (args: string[]): int =
    async {
        let args = readArgs args
        let! result = Async.AwaitTask <| processData args.ConfigPath
        do! Async.AwaitTask <| writeOutput result args.OutputPath
    } |> Async.RunSynchronously
    0
