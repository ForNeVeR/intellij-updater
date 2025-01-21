// SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
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
    let! version = Ide.ReadLatestVersion IdeKind.Rider Release (Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4"))) Newest
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
    let! version =
        Ide.ReadLatestVersion
            IdeKind.IntelliJIdeaCommunity
            Release
            (Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4")))
            Newest
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
    let latestNightly = Ide.SelectVersion UpdateFlavor.Nightly None versions Newest
    let latestEap = Ide.SelectVersion UpdateFlavor.EAP None versions Newest
    Assert.Equal(latest, latestNightly)
    Assert.Equal(eap, latestEap)

[<Fact>]
let ``Flavor constraint gets applied before latest wave constraint``(): unit =
    let versions = [|
        IdeVersion.Parse "2024.1.4"
        IdeVersion.Parse "2024.1.3"
        IdeVersion.Parse "2024.2-EAP1-SNAPSHOT"
        IdeVersion.Parse "2024.2-EAP2-SNAPSHOT"
    |]
    let latestEap = Ide.SelectVersion UpdateFlavor.EAP (Some LatestWave) versions Newest
    let latestRelease = Ide.SelectVersion UpdateFlavor.Release (Some LatestWave) versions Newest
    Assert.Equal(IdeVersion.Parse "2024.2-EAP2-SNAPSHOT", latestEap)
    Assert.Equal(IdeVersion.Parse "2024.1.4", latestRelease)

[<Fact>]
let ``Only year-based versions are read from the releases repository``(): unit =
    let _, filter = Ide.ReleaseMetadata("idea/ideaIC")
    Assert.True <| filter(IdeWave.YearBased(2024, 2))
    Assert.False <| filter(IdeWave.YearBasedVersion(243))
