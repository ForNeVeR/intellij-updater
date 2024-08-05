// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Versioning

open System

type IdeWave =
    | Legacy of major: int * minor: int // e.g. 14.0
    | YearBasedVersion of major: int * version: int // e.g. 231.8109
    | YearBased of year: int * number: int // 2024.1
    | Latest // literally LATEST
type IdeFlavor =
    | Snapshot
    | RollingEAP
    | RollingEAPCandidate
    | EAP of int * dev: bool
    | RC of int
    | Stable

    static member Parse(x: string) =
        if x = "EAP" then RollingEAP
        else if x = "EAP-CANDIDATE" then RollingEAPCandidate
        else if x.StartsWith "EAP" && x.EndsWith "D" then EAP(int(x.Substring(3, x.Length - 4)), true)
        else if x.StartsWith "EAP" then EAP(int(x.Substring 3), false)
        else if x.StartsWith "RC" then RC(int(x.Substring 2))
        else if x = "" then Stable
        else failwithf $"Cannot parse IDE flavor: {x}."

type IdeVersion = // TODO[#6]: Verify ordering
    {
        Wave: IdeWave
        Patch: int
        Flavor: IdeFlavor
        IsSnapshot: bool
    }

    static member Parse(description: string) =
        let components = description.Split '-'

        let parseComponents (wave: string) flavor isSnapshot =
            let wave, patch =
                let waveComponents = wave.Split '.'
                match waveComponents with
                | [| "LATEST" |] -> Latest, 0
                | _ ->
                    let major, minor, patch =
                        match waveComponents with
                        | [| major |] when int major < 1000 -> int major, 0, 0 // allow versions like "14" or "231"
                        | [| major; minor |] -> int major, int minor, 0
                        | [| major; minor; patch |] -> int major, int minor, int patch
                        | _ -> failwithf $"Cannot parse year-based version \"{wave}\"."
                    let wave =
                        match major with
                        | _ when major < 100 -> Legacy(major, minor)
                        | _ when major < 1000 -> YearBasedVersion(major, minor)
                        | _ -> YearBased(major, minor)
                    wave, patch

            let flavor =
                match IdeFlavor.Parse flavor, isSnapshot with
                | Stable, true -> Snapshot
                | flavor, _ -> flavor
            {
                Wave = wave
                Patch = patch
                Flavor = flavor
                IsSnapshot = isSnapshot
            }

        match components with
        | [| wave |] -> parseComponents wave "" false
        | [| wave; "SNAPSHOT" |] -> parseComponents wave "" true
        | [| wave; flavor; "CANDIDATE"; "SNAPSHOT" |] -> parseComponents wave (flavor + "-CANDIDATE") true
        | [| wave; flavor; "SNAPSHOT" |] -> parseComponents wave flavor true
        | _ -> failwithf $"Cannot parse IDE version \"{description}\"."

    override this.ToString() =
        String.concat "" [|
            let components =
                match this.Wave with
                // NOTE: Legacy waves might not round-trip, e.g. "14" will be converted into "14.0". We don't care.
                | Legacy(major, minor) -> [| string major; string minor |]
                | YearBasedVersion(major, version) when version = 0 -> [| string major|]
                | YearBasedVersion(major, version) -> [| string major; string version |]
                | YearBased(year, number) -> [| string year; string number |]
                | Latest -> [| "LATEST" |]
            components |> String.concat "."
            if this.Patch <> 0 then
                "."
                string this.Patch

            match this.Flavor with
            | Snapshot -> ""
            | RollingEAP -> "-EAP"
            | RollingEAPCandidate -> "-EAP-CANDIDATE"
            | EAP(n, dev) ->
                let d = if dev then "D" else ""
                $"-EAP{string n}{d}"
            | RC n -> $"-RC{n}"
            | Stable -> ""

            if this.IsSnapshot then "-SNAPSHOT"
        |]

[<RequireQualifiedAccess>]
type EntityVersion =
    | Ide of IdeVersion
    | Kotlin of Version
    | NextMajor of waveNumber: int
