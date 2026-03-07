// SPDX-FileCopyrightText: 2025-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.KotlinTests

open IntelliJUpdater
open IntelliJUpdater.Versioning
open Semver
open Xunit

let private parse (s: string) = SemVersion.Parse(s, SemVersionStyles.Any)

[<Fact>]
let ``Kotlin version determiner``(): unit =
    Assert.Equal(parse "2.1.10", Kotlin.ForIde <| YearBased(2025, 1))
    Assert.Equal(parse "2.1.10", Kotlin.ForIde <| YearBasedVersion 251)
    Assert.Equal(parse "2.3.20-RC2", Kotlin.ForIde <| YearBasedVersion 261)
