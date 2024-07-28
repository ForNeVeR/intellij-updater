// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

﻿module IntelliJUpdater.Versioning

type IdeWave =
    | YearBased of year: int * number: int // 2024.1
type IdeFlavor =
    | Snapshot
    | EAP of int * dev: bool
    | RC of int
    | Stable

    static member Parse(x: string) =
        if x.StartsWith "EAP" && x.EndsWith "D" then EAP(int(x.Substring(3, x.Length - 4)), true)
        else if x.StartsWith "EAP" then EAP(int(x.Substring 3), false)
        else if x.StartsWith "RC" then RC(int(x.Substring 2))
        else if x = "" then Stable
        else failwithf $"Cannot parse IDE flavor: {x}."

type IdeVersion = // TODO[#358]: Verify ordering
    {
        Wave: IdeWave
        Minor: int
        Flavor: IdeFlavor
        IsSnapshot: bool
    }

    static member Parse(description: string) =
        let components = description.Split '-'

        let parseComponents (yearBased: string) flavor isSnapshot =
            let yearBasedComponents = yearBased.Split '.'
            let year, number, minor =
                match yearBasedComponents with
                | [| year; number |] -> int year, int number, 0
                | [| year; number; minor |] -> int year, int number, int minor
                | _ -> failwithf $"Cannot parse year-based version \"{yearBased}\"."
            let flavor =
                match IdeFlavor.Parse flavor, isSnapshot with
                | Stable, true -> Snapshot
                | flavor, _ -> flavor
            {
                Wave = YearBased(year, number)
                Minor = minor
                Flavor = flavor
                IsSnapshot = isSnapshot
            }

        match components with
        | [| yearBased |] -> parseComponents yearBased "" false
        | [| yearBased; "SNAPSHOT" |] -> parseComponents yearBased "" true
        | [| yearBased; flavor; "SNAPSHOT" |] -> parseComponents yearBased flavor true
        | _ -> failwithf $"Cannot parse IDE version \"{description}\"."

    override this.ToString() =
        String.concat "" [|
            let (YearBased(year, number)) = this.Wave
            string year
            "."
            string number
            if this.Minor <> 0 then
                "."
                string this.Minor

            match this.Flavor with
            | Snapshot -> ""
            | EAP(n, dev) ->
                let d = if dev then "D" else ""
                $"-EAP{string n}{d}"
            | RC n -> $"-RC{n}"
            | Stable -> ""

            if this.IsSnapshot then "-SNAPSHOT"
        |]
