// SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Kotlin

open System
open IntelliJUpdater.Versioning

// https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
let ForIde (wave: IdeWave): Version =
    match wave.NormalizedMajorNumber with
    | 261 -> Version.Parse "2.3.20-RC2" // tentative, see https://github.com/JetBrains/intellij-community/blob/261/.idea/libraries/kotlin_stdlib.xml#L3
    | 253 -> Version.Parse "2.2.20"
    | 252 -> Version.Parse "2.1.20"
    | 251 -> Version.Parse "2.1.10"
    | 243 -> Version.Parse "2.0.21"
    | 242 -> Version.Parse "1.9.24"
    | 241 -> Version.Parse "1.9.22"
    | 233 -> Version.Parse "1.9.21"
    | 232 -> Version.Parse "1.8.20"
    | 231 -> Version.Parse "1.8.0"
    | 223 -> Version.Parse "1.7.22"
    | 222 -> Version.Parse "1.6.21"
    | 221 -> Version.Parse "1.6.10"
    | _ -> failwithf $"Cannot determine Kotlin version for IDE wave {wave}."
