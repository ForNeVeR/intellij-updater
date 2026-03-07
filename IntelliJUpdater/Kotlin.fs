// SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Kotlin

open IntelliJUpdater.Versioning
open Semver

let private parse (s: string) = SemVersion.Parse(s, SemVersionStyles.Any)

// https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
let ForIde (wave: IdeWave): SemVersion =
    match wave.NormalizedMajorNumber with
    | 261 -> parse "2.3.20-RC2" // tentative, see https://github.com/JetBrains/intellij-community/blob/261/.idea/libraries/kotlin_stdlib.xml#L3
    | 253 -> parse "2.2.20"
    | 252 -> parse "2.1.20"
    | 251 -> parse "2.1.10"
    | 243 -> parse "2.0.21"
    | 242 -> parse "1.9.24"
    | 241 -> parse "1.9.22"
    | 233 -> parse "1.9.21"
    | 232 -> parse "1.8.20"
    | 231 -> parse "1.8.0"
    | 223 -> parse "1.7.22"
    | 222 -> parse "1.6.21"
    | 221 -> parse "1.6.10"
    | _ -> failwithf $"Cannot determine Kotlin version for IDE wave {wave}."
