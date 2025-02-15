// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Kotlin

open System
open IntelliJUpdater.Versioning

// https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
let ForIde (wave: IdeWave): Version =
    match wave.NormalizedYearMajorNumber with
    | 251 -> Version.Parse "2.1.10"
    | 243 -> Version.Parse "2.0.21"
    | 242 -> Version.Parse "1.9.24"
    | 241 -> Version.Parse "1.9.22"
    | 233 -> Version.Parse "1.9.21"
    | 232 -> Version.Parse "1.8.20"
    | 231 -> Version.Parse "1.8.0"
    | _ -> failwithf $"Cannot determine Kotlin version for IDE wave {wave}."
