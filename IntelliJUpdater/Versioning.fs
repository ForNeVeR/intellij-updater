// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Versioning

open System

type FullVersion =
    | FullVersion of major: int option * minor: int option * patch: int option * build: int option

    static member None = FullVersion(None, None, None, None)

    static member Parse(s: string) =
        if s = "LATEST" then FullVersion.None else
        match s.Split '.' with
        | [| major |] -> FullVersion(Some(int major), None, None, None)
        | [| major; minor |] -> FullVersion(Some(int major), Some(int minor), None, None)
        | [| major; minor; patch |] -> FullVersion(Some(int major), Some(int minor), Some(int patch), None)
        | [| major; minor; patch; build |] ->
            FullVersion(Some(int major), Some(int minor), Some(int patch), Some(int build))
        | _ -> failwithf $"Cannot parse full version: {s}."

    override this.ToString() =
        let (FullVersion(major, minor, patch, build)) = this
        seq { major; minor; patch; build } |> Seq.choose id |> Seq.map string |> String.concat "."

type IdeWave =
    | Legacy of major: int * minor: int // e.g. 14.0
    | YearBasedVersion of major: int // e.g. 231
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
        FullVersion: FullVersion
        Flavor: IdeFlavor
        IsSnapshot: bool
    }

    static member Parse(description: string): IdeVersion =
        let components = description.Split '-'

        let parseComponents (waveString: string) flavor isSnapshot =
            let wave =
                let waveComponents = waveString.Split '.'
                match waveComponents with
                | [| "LATEST" |] -> Latest
                | _ ->
                    let major, minor =
                        match waveComponents |> Array.truncate 2 with
                        | [| major |] when int major < 1000 -> int major, 0 // allow versions like "14" or "231"
                        | [| major; minor |] -> int major, int minor
                        | _ -> failwithf $"Cannot parse year-based version \"{waveString}\"."
                    match major with
                    | _ when major < 100 -> Legacy(major, minor)
                    | _ when major < 1000 -> YearBasedVersion major
                    | _ -> YearBased(major, minor)

            let flavor =
                match IdeFlavor.Parse flavor, isSnapshot with
                | Stable, true -> Snapshot
                | flavor, _ -> flavor
            {
                Wave = wave
                FullVersion = FullVersion.Parse waveString
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
            match this.Wave with
            | Latest -> "LATEST"
            | _ -> this.FullVersion.ToString()

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
