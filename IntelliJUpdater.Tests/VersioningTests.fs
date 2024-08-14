// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.VersioningTests

open IntelliJUpdater.Versioning
open Xunit

[<Theory>]
[<InlineData("14", 14, 0)>]
[<InlineData("14.0.1", 14, 0)>]
[<InlineData("15.0", 15, 0)>]
let ``Legacy wave parser``(version: string, major: int, minor: int): unit =
    let ideVersion = IdeVersion.Parse version
    let expected = Legacy(major, minor)
    Assert.Equal(expected, ideVersion.Wave)
    Assert.Equal(FullVersion.Parse version, ideVersion.FullVersion)

[<Theory>]
[<InlineData("139.1.20", 139, 1, 20)>]
[<InlineData("241.18034.55", 241, 18034, 55)>]
let ``Number-based wave parser``(version: string, major: int, minor: int, patch: int): unit =
    let ideVersion = IdeVersion.Parse version
    let expected = YearBasedVersion major
    Assert.Equal(expected, ideVersion.Wave)
    Assert.Equal(FullVersion.Parse version, ideVersion.FullVersion)

let ``Year-based wave parser``(): unit =
    let expected = {
        Wave = YearBased(2024, 2)
        FullVersion = FullVersion(Some 2024, Some 2, Some 0, Some 1)
        Flavor = Stable
        IsSnapshot = false
    }
    let actual = IdeVersion.Parse "2024.2.0.1"
    Assert.Equal(expected, actual)

[<Fact>]
let ``IntelliJ toString``(): unit =
    let latest = { Wave = Latest; FullVersion = FullVersion.None ; Flavor = RollingEAP; IsSnapshot = true }
    let rollingEap = { Wave = YearBasedVersion 231
                       FullVersion = FullVersion.Parse "231"
                       Flavor = RollingEAP
                       IsSnapshot = true }
    let eap = { Wave = YearBasedVersion 231
                FullVersion = FullVersion.Parse "231.9423"
                Flavor = RollingEAPCandidate
                IsSnapshot = true }

    Assert.Equal("LATEST-EAP-SNAPSHOT", latest.ToString())
    Assert.Equal("231-EAP-SNAPSHOT", rollingEap.ToString())
    Assert.Equal("231.9423-EAP-CANDIDATE-SNAPSHOT", eap.ToString())
