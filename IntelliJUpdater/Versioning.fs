// SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Versioning

open System

type FullVersion =
    | FullVersion of major: int option * minor: int option * build: int option * hasThousands: bool

    static member None = FullVersion(None, None, None, false)

    static member Parse(s: string) =
        if s = "LATEST" then FullVersion.None else
        let mutable components = s.Split '.' |> Array.map int
        let failed() = failwithf $"Cannot parse full version: {s}."
        if components.Length = 0 then failed()
        let hasThousands =
            if components[0] > 2000 && components.Length >= 2 then
                let yearCode = components[0] - 2000
                if yearCode < 0 || yearCode > 99 then failed()
                let numberCode = components[1]
                if numberCode < 0 || numberCode > 9 then failed()
                components <- [|
                    yearCode * 10 + numberCode
                    yield! Array.skip 2 components
                |]
                true
            else false

        match components with
        | [| major |] -> FullVersion(Some(int major), None, None, hasThousands)
        | [| major; minor |] -> FullVersion(Some(int major), Some(int minor), None, hasThousands)
        | [| major; minor; build |] -> FullVersion(Some(int major), Some(int minor), Some(int build), hasThousands)
        | _ -> failwithf $"Cannot parse full version: {s}."

    override this.ToString() =
        let (FullVersion(major, minor, build, hasThousands)) = this
        let numbers =
            if hasThousands
            then seq {
                major |> Option.map (fun x -> 2000 + x / 10)
                major |> Option.map(fun x -> x % 10)
                minor
                build
            }
            else seq { major; minor; build }
        numbers |> Seq.choose id |> Seq.map string |> String.concat "."

[<CustomEquality; CustomComparison>]
type IdeWave =
    | Legacy of major: int * minor: int // e.g. 14.0
    | YearBasedVersion of major: int // e.g. 231
    | YearBased of year: int * number: int // 2024.1
    | Latest // literally LATEST

    interface IComparable with
        member this.CompareTo(other: obj): int =
            let other = other :?> IdeWave
            match this, other with
            | _ ,_ when this = other -> 0
            | Latest, _ -> 1
            | _, Latest -> -1
            | Legacy(a, b), Legacy(c, d) -> compare (a, b) (c, d)
            | Legacy _, _ -> -1
            | _, Legacy _ -> 1
            | _, _ ->
                let normalizeYearBased = function
                    | YearBasedVersion major -> major
                    | YearBased(year, number) as x ->
                        let yearCode = year - 2000
                        if yearCode < 0 || yearCode > 99
                        then failwithf $"Invalid year in YearBased version: {x}."
                        if number < 0 || number > 9
                        then failwithf $"Invalid number in YearBased version: {x}."
                        yearCode * 10 + number
                    | x -> failwithf $"Impossible version pattern passed: {x}."

                let a = normalizeYearBased this
                let b = normalizeYearBased other
                compare a b

    override this.Equals(other: obj) =
        match other with
        | :? IdeWave as x ->
            match this, x with
            | Legacy(a, b), Legacy(c, d) -> a = c && b = d
            | YearBasedVersion a, YearBasedVersion b -> a = b
            | YearBased(a, b), YearBased(c, d) -> a = c && b = d
            | Latest, Latest -> true
            | _ -> false
        | _ -> false

    override this.GetHashCode() =
        match this with
        | Legacy(major, minor) -> hash(major, minor)
        | YearBasedVersion major -> hash major
        | YearBased(year, number) -> hash(year, number)
        | Latest -> 1

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

[<StructuralEquality; StructuralComparison>]
type IdeVersion =
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
