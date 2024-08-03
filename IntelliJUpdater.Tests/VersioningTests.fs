// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.VersioningTests

open IntelliJUpdater.Versioning
open Xunit

[<Theory>]
[<InlineData("14", 14, 0, 0)>]
[<InlineData("14.0.1", 14, 0, 1)>]
[<InlineData("15.0", 15, 0, 0)>]
let ``Legacy wave parser``(version: string, major: int, minor: int, patch: int): unit =
    let version = IdeVersion.Parse version
    let expected = Legacy(major, minor)
    Assert.Equal(expected, version.Wave)
    Assert.Equal(patch, version.Patch)

[<Theory>]
[<InlineData("139.1.20", 139, 1, 20)>]
[<InlineData("241.18034.55", 241, 18034, 55)>]
let ``Number-based wave parser``(version: string, major: int, minor: int, patch: int): unit =
    let version = IdeVersion.Parse version
    let expected = YearBasedVersion(major, minor)
    Assert.Equal(expected, version.Wave)
    Assert.Equal(patch, version.Patch)
