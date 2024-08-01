// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Text.RegularExpressions
open System.Threading.Tasks
open System.Xml.Linq
open System.Xml.XPath
open IntelliJUpdater
open IntelliJUpdater.Versioning
open TruePath

let snapshotMetadataUrl =
    Uri "https://www.jetbrains.com/intellij-repository/snapshots/com/jetbrains/intellij/rider/riderRD/maven-metadata.xml"
let releaseMetadataUrl =
    Uri "https://www.jetbrains.com/intellij-repository/releases/com/jetbrains/intellij/rider/riderRD/maven-metadata.xml"

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

// https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
let GetKotlinVersion wave =
    match wave with
    | YearBased(2024, 2) -> Version.Parse "1.9.24"
    | YearBased(2024, 1) -> Version.Parse "1.9.22"
    | YearBased(2023, 3) -> Version.Parse "1.9.21"
    | YearBased(2023, 2) -> Version.Parse "1.8.20"
    | YearBased(2023, 1) -> Version.Parse "1.8.0"
    | _ -> failwithf $"Cannot determine Kotlin version for IDE wave {wave}."

type StoredEntityVersion = {
    File: LocalPath
    Field: string
    Update: EntityVersion
}

let ReadLatestSpecs config: Task<StoredEntityVersion[]> = task {
    let specs = failwithf "TODO"
    let kotlinKey = failwithf "TODO"
    let untilKey = failwithf "TODO"
    use http = new HttpClient()
    let readVersions (url: Uri) filter = task {
        printfn $"Loading document \"{url}\"."
        let sw = Stopwatch.StartNew()

        let! response = http.GetAsync(url)
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync()
        let document = XDocument.Parse content
        printfn $"Loaded and processed the document in {sw.ElapsedMilliseconds} ms."

        let versions =
            document.XPathSelectElements "//metadata//versioning//versions//version"
            |> Seq.toArray
        if versions.Length = 0 then failwithf "No Rider SDK versions found."
        let maxVersion =
            versions
            |> Seq.map(fun version ->
                let version = version.Value
                printfn $"Version found: {version}"
                version
            )
            |> Seq.map IdeVersion.Parse
            |> Seq.filter filter
            |> Seq.max

        return maxVersion
    }

    let! pairs =
        specs
        |> Map.toSeq
        |> Seq.map(fun(id, (url, filter)) -> task {
            let! versions = readVersions url filter
            return id, versions
        })
        |> Task.WhenAll

    let ideVersions = Map.ofArray pairs
    let ideVersionForKotlin = ideVersions |> Map.find kotlinKey
    let ideVersionForUntilBuild = ideVersions |> Map.find untilKey

    return failwithf "TODO"
}

let private ReadValue (filePath: LocalPath) (key: string) =
    match filePath.GetExtensionWithoutDot() with
    | "toml" -> TomlFile.ReadValue filePath key
    | "properties" -> PropertiesFile.ReadValue filePath key
    | other -> failwithf $"Unknown file extension: \"{other}\"."

let private ReadVersion(update: Update): Task<StoredEntityVersion> = task {
    let! version =
        match update.Kind, update.Augmentation with
        | Ide key, None ->
            task {
                let! text = ReadValue update.File update.Field
                let version = IdeVersion.Parse text
                return EntityVersion.Ide version
            }
        | Ide key, Some NextMajor ->
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

    let fullVersion v =
        let (YearBased(year, number)) = v.Wave
        String.concat "" [|
            string year
            "."
            string number

            if v.Minor <> 0 then
                "."
                string v.Minor

            match v.Flavor with
            | Snapshot -> ()
            | EAP(n, dev) ->
                let d = if dev then "D" else ""
                $" EAP{string n}{d}"
            | RC n -> $" RC{string n}"
            | Stable -> ()
        |]

    let toString item =
        match item.Update with
        | EntityVersion.Ide version -> fullVersion version
        | EntityVersion.Kotlin version -> version.ToString()
        | EntityVersion.NextMajor waveNumber -> $"{waveNumber}.*"

    let message = String.concat "\n" [|
        for item in diff do
            let key = item.Key
            let old = localMap[key]
            let updated = remoteMap[key]

            let file, field = key
            $"- {file}:{field}: {toString old} -> {toString updated}"
    |]

    let preSection =
        config.PrBodyPrefix
        |> Option.map(fun x -> x + "\n\n")

    {|
        BranchName = "dependencies/rider"
        CommitMessage = "Dependencies: update Rider"
        PrTitle = "Rider Update"
        PrBodyMarkdown = $"""
{preSection}## Version Updates
{message}
"""
    |}

let isStable version =
    version.Flavor = Stable

let atLeastEap version =
    match version.Flavor with
    | Snapshot -> false
    | EAP _ -> true
    | RC _ -> true
    | Stable -> true

let ideVersionSpec = Map.ofArray [|
    "riderSdk", (releaseMetadataUrl, isStable)
    "riderSdkPreview", (snapshotMetadataUrl, atLeastEap)
|]

let readConfig path = task {
    use stream = File.OpenRead path
    return! Configuration.Read stream
}

let processData configPath = task {
    let! config = readConfig configPath
    let! latestSpec = ReadLatestSpecs config
    let! currentSpec = ReadCurrentSpecs config
    if latestSpec <> currentSpec then
        printfn "Changes detected."
        printfn $"Local spec: {currentSpec}."
        printfn $"Remote spec: {latestSpec}."
        do! ApplySpec latestSpec
        return HasChanges <| GenerateResult config currentSpec latestSpec
    else
        printfn $"No changes detected: both local and remote specs are {latestSpec}."
        return NoChanges
}

let writeOutput result out = task {
    match result with
    | NoChanges ->
        do! File.WriteAllTextAsync(out, "has-changes=false")
    | HasChanges changes ->
        let prBodyMarkdownPath = Path.GetTempFileName()
        do! File.WriteAllTextAsync(prBodyMarkdownPath, changes.PrBodyMarkdown)
        let text = $"""has-changes=true
branch-name={changes.BranchName}
commit-message={changes.CommitMessage}
pr-title={changes.PrTitle}
pr-body-path={prBodyMarkdownPath}
"""
        do! File.WriteAllTextAsync(out, text.ReplaceLineEndings "\n")

    printfn $"Result printed to \"{out}\"."
}

type Args = {
    ConfigPath: string
    OutputPath: string
}

let readArgs = function
| [| config; output |] -> { ConfigPath = config; OutputPath = output }
| _ -> failwith "Arguments expected: <config-file-path> <output-file-path>"

[<EntryPoint>]
let main (args: string[]): int =
    async {
        let args = readArgs args
        let! result = Async.AwaitTask <| processData args.ConfigPath
        do! Async.AwaitTask <| writeOutput result args.OutputPath
    } |> Async.RunSynchronously
    0
