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

    static let mapUpdate(update: JsonUpdate): Update = {
        File = LocalPath update.File
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

    static member Read(input: Stream): Task<Configuration> = task {
        let! config = JsonSerializer.DeserializeAsync<JsonConfiguration>(input, jsonOptions)
        return {
            Updates = config.Updates |> Array.map mapUpdate
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
    | Ide of string
    | Kotlin

    static member Parse(x: string): UpdateKind =
        match x.ToLowerInvariant() with
        | "kotlin" -> Kotlin
        | other -> Ide other
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