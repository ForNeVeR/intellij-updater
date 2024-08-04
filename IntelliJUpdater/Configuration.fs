// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace IntelliJUpdater

open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open IntelliJUpdater.Versioning
open TruePath

[<CLIMutable>]
type JsonConfiguration = {
    Updates: JsonUpdate[]
    PrBodyPrefix: string option
}
and [<CLIMutable>] JsonUpdate = {
    File: string
    Field: string
    Kind: string
    VersionFlavor: string
    VersionConstraint: string option
    Augmentation: string option
}

type Configuration =
    {
        Updates: Update[]
        PrBodyPrefix: string option
    }

    static let mapUpdate (basePath: LocalPath) (update: JsonUpdate): Update = {
        File = basePath / update.File
        Field = update.Field
        Kind = UpdateKind.Parse update.Kind
        VersionFlavor = UpdateFlavor.Parse update.VersionFlavor
        VersionConstraint = update.VersionConstraint |> Option.map IdeVersionConstraint.Parse
        Augmentation = update.Augmentation |> Option.map Augmentation.Parse
    }

    static let jsonOptions = JsonSerializerOptions(
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    )

    static member Read(location: LocalPath) = task {
        use stream = new FileStream(location.Value, FileMode.Open, FileAccess.Read)
        return! Configuration.Read(location, stream)
    }

    static member Read(basePath: LocalPath, input: Stream): Task<Configuration> = task {
        let! config = JsonSerializer.DeserializeAsync<JsonConfiguration>(input, jsonOptions)
        return {
            Updates = config.Updates |> Array.map (mapUpdate basePath.Parent.Value)
            PrBodyPrefix = config.PrBodyPrefix
        }
    }
and Update = {
    File: LocalPath
    Field: string
    Kind: UpdateKind
    VersionFlavor: UpdateFlavor
    VersionConstraint: IdeVersionConstraint option
    Augmentation: Augmentation option
}
and UpdateKind =
    | Ide of IdeKind
    | Kotlin

    static member Parse(x: string): UpdateKind =
        match x.ToLowerInvariant() with
        | "kotlin" -> Kotlin
        | other -> Ide(IdeKind.Parse other)
and [<RequireQualifiedAccess>] IdeKind =
    | Rider
    | IntelliJIdeaCommunity

    static let mapping = Map.ofArray [|
        "rider", Rider
        "intellij-idea-community", IntelliJIdeaCommunity
    |]

    static member Parse(x: string) =
        match Map.tryFind (x.ToLowerInvariant()) mapping with
        | Some ide -> ide
        | None ->
            let keys = String.concat ", " mapping.Keys
            failwithf $"Cannot parse IDE kind {x}. Supported keys: {keys}."
and UpdateFlavor =
    | Release
    | EAP
    | Nightly

    static member Parse(x: string): UpdateFlavor =
        match x.ToLowerInvariant() with
        | "release" -> Release
        | "eap" -> EAP
        | "nightly" -> Nightly
        | o -> failwithf $"""Cannot parse versionFlavor value "{o}"."""
and IdeVersionConstraint =
    | LessOrEqualTo of IdeVersion

    static member Parse(x: string): IdeVersionConstraint =
        if x.StartsWith "<=" then
            LessOrEqualTo(IdeVersion.Parse (x.Substring 2))
        else
            failwithf $"""Cannot parse VersionConstraint: "{x}"."""
and Augmentation =
    | NextMajor

    static member Parse(x: string): Augmentation =
        match x with
        | "nextMajor" -> NextMajor
        | o -> failwithf $"""Cannot parse augmentation value "{o}"."""
