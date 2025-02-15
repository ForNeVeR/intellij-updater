// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.KotlinTests

open System
open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Fact>]
let ``Kotlin version determiner``(): unit =
    Assert.Equal(Version.Parse("2.1.10"), Kotlin.ForIde <| YearBased(2025, 1))
    Assert.Equal(Version.Parse("2.1.10"), Kotlin.ForIde <| YearBasedVersion 251)
