// SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
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

[<Fact>]
let ``Year-based wave parser works``(): unit =
    let expected = {
        Wave = YearBased(2024, 2)
        FullVersion = FullVersion(Some 242, Some 0, Some 1, true)
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
    Assert.Equal("2024.2.0.1", (IdeVersion.Parse "2024.2.0.1").ToString())

[<Fact>]
let ``Version comparison``(): unit =
    Assert.True(IdeVersion.Parse "2024.2" > IdeVersion.Parse "2024.2-RC1-SNAPSHOT")
    Assert.True(IdeVersion.Parse "2024.2-RC1-SNAPSHOT" > IdeVersion.Parse "2024.1")

[<Fact>]
let ``Sort order for year-based and number-based versions is correct``(): unit =
    Assert.True(IdeVersion.Parse "2024.2" > IdeVersion.Parse "241.1234")
    Assert.True(IdeVersion.Parse "243.1234" > IdeVersion.Parse "2024.2")
    Assert.True(IdeVersion.Parse "243.1234" > IdeVersion.Parse "2024.3")
