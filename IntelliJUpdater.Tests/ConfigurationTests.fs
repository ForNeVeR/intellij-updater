// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module ConfigurationTests

open IntelliJUpdater
open IntelliJUpdater.Versioning
open System.IO
open System.Text
open System.Threading.Tasks
open TruePath
open Xunit

[<Fact>]
let ``Config is read correctly``(): Task =
    let content = """
{
    "updates": [{
        "file": "testData/config.toml",
        "field": "riderSdkVersion",
        "kind": "rider",
        "versionFlavor": "release",
        "versionConstraint": "<=2024.1.4"
    }, {
        "file": "testData/config.properties",
        "field": "untilBuildVersion",
        "kind": "rider",
        "versionFlavor": "release",
        "versionConstraint": "<=2024.1.4",
        "augmentation": "nextMajor"
    }],
    "prBodyPrefix": "test"
}
"""
    let expectedConfig = {
        PrBodyPrefix = Some "test"
        Updates = [|
            {
                File = LocalPath "testData/config.toml"
                Field = "riderSdkVersion"
                Kind = Ide IdeKind.Rider
                VersionFlavor = Release
                VersionConstraint = Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4"))
                Augmentation = None
            }
            {
                File = LocalPath "testData/config.properties"
                Field = "untilBuildVersion"
                Kind = Ide IdeKind.Rider
                VersionFlavor = Release
                VersionConstraint = Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4"))
                Augmentation = Some NextMajor
            }
        |]
    }
    task {
        use stream = new MemoryStream(Encoding.UTF8.GetBytes(content))
        let! config = Configuration.Read(LocalPath "testData", stream)
        Assert.Equal(expectedConfig, config)
    }

[<Fact>]
let ``Latest wave constraint is read``(): Task =
    let content = """
{
    "updates": [{
        "file": "testData/config.toml",
        "field": "riderSdkVersion",
        "kind": "rider",
        "versionFlavor": "release",
        "versionConstraint": "latestWave",
        "order": "oldest"
    }],
    "prBodyPrefix": "test"
}
"""
    let expectedConfig = {
        PrBodyPrefix = Some "test"
        Updates = [|
            {
                File = LocalPath "testData/config.toml"
                Field = "riderSdkVersion"
                Kind = Ide IdeKind.Rider
                VersionFlavor = Release
                VersionConstraint = Some LatestWave
                Augmentation = None
            }
        |]
    }
    task {
        use stream = new MemoryStream(Encoding.UTF8.GetBytes(content))
        let! config = Configuration.Read(LocalPath "testData", stream)
        Assert.Equal(expectedConfig, config)
    }
