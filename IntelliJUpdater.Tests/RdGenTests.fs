// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.RdGenTests

open System
open System.Threading.Tasks
open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Fact>]
let ``RdGen version determiner``(): Task = task {
    let! v243 = RdGen.ForIde <| YearBased(2024, 3)
    let! v251 = RdGen.ForIde <| YearBasedVersion 251
    Assert.Equal(Version.Parse("2024.3.1"), v243)
    Assert.Equal(Version.Parse("2025.1.1"), v251)
}
