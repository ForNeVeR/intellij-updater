// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.IdeTests

open System.IO
open System.Text
open System.Threading.Tasks
open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Fact>]
let ``Rider version is read``(): Task = task {
    let! version = Ide.ReadLatestVersion IdeKind.Rider Release (Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4")))
    let expected = {
        Wave = YearBased(2024, 1)
        FullVersion = FullVersion.Parse "2024.1.4"
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
        FullVersion = FullVersion.Parse "2024.1.4"
        Flavor = Stable
        IsSnapshot = false
    }
    Assert.Equal(expected, version)
}

[<Fact>]
let ``IntelliJ versions are supported``(): Task = task {
    let xml = """<metadata>
    <groupId>com.jetbrains.intellij.idea</groupId>
    <artifactId>ideaIC</artifactId>
    <versioning>
        <latest>242.20224.159-EAP-SNAPSHOT</latest>
        <versions>
            <version>LATEST-EAP-SNAPSHOT</version>
            <version>231-EAP-SNAPSHOT</version>
            <version>231.9423-EAP-CANDIDATE-SNAPSHOT</version>
        </versions>
    </versioning>
</metadata>"""
    let expectedVersions = [|
        { Wave = Latest; FullVersion = FullVersion.None; Flavor = RollingEAP; IsSnapshot = true }
        { Wave = YearBasedVersion(231); FullVersion = FullVersion.Parse "231"; Flavor = RollingEAP; IsSnapshot = true }
        { Wave = YearBasedVersion(231)
          FullVersion = FullVersion.Parse "231.9423"
          Flavor = RollingEAPCandidate
          IsSnapshot = true }
    |]

    use stream = new MemoryStream(Encoding.UTF8.GetBytes xml)
    let! versions = Ide.ReadVersionsFromStream(stream)
    Assert.Equal<IdeVersion>(expectedVersions, versions)
}

[<Fact>]
let ``IntelliJ versions are properly selected``(): unit =
    let latest = { Wave = Latest; FullVersion = FullVersion.None; Flavor = Snapshot; IsSnapshot = true }
    let rollingEap = { Wave = YearBasedVersion 231
                       FullVersion = FullVersion.Parse "231.0"
                       Flavor = RollingEAP
                       IsSnapshot = true }
    let eap = { Wave = YearBasedVersion 231
                FullVersion = FullVersion.Parse "231.9423"
                Flavor = RollingEAP
                IsSnapshot = true }
    let versions = [|
        latest
        rollingEap
        eap
    |]
    let latestNightly = Ide.SelectLatestVersion UpdateFlavor.Nightly None versions
    let latestEap = Ide.SelectLatestVersion UpdateFlavor.EAP None versions
    Assert.Equal(latest, latestNightly)
    Assert.Equal(eap, latestEap)
