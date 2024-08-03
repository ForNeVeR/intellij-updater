// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.IdeTests

open System.Threading.Tasks
open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Fact>]
let ``Rider version is read``(): Task = task {
    let! version = Ide.ReadLatestVersion IdeKind.Rider Release (Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4")))
    let expected = {
        Wave = YearBased(2024, 1)
        Patch = 4
        Flavor = Stable
        IsSnapshot = false
    }
    Assert.Equal(expected, version)
}

[<Fact>]
let ``IntelliJ IDEA Community version is read``(): Task = task {
    let! version = Ide.ReadLatestVersion IdeKind.IntelliJIdeaCommunity Release (Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4")))
    let expected = {
        Wave = YearBased(2024, 1)
        Patch = 4
        Flavor = Stable
        IsSnapshot = false
    }
    Assert.Equal(expected, version)
}
