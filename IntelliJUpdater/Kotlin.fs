// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Kotlin

open System
open IntelliJUpdater.Versioning

// https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
let ForIde (wave: IdeWave): Version =
    match wave with
    | YearBased(2025, 1) -> Version.Parse "2.1.10"
    | YearBased(2024, 3) -> Version.Parse "2.0.21"
    | YearBased(2024, 2) -> Version.Parse "1.9.24"
    | YearBased(2024, 1) -> Version.Parse "1.9.22"
    | YearBased(2023, 3) -> Version.Parse "1.9.21"
    | YearBased(2023, 2) -> Version.Parse "1.8.20"
    | YearBased(2023, 1) -> Version.Parse "1.8.0"
    | _ -> failwithf $"Cannot determine Kotlin version for IDE wave {wave}."
