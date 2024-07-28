// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module ConfigurationTests

open IntelliJUpdater
open IntelliJUpdater.Versioning
open System.IO
open System.Text
open System.Threading.Tasks
open Xunit

[<Fact>]
let ``Config is read correctly``(): Task =
    let content = """
{
    "updates": [ {
    "file": "testData/config.toml",
    "field": "riderSdkVersion",
    "kind": "rider",
    "versionFlavor": "release",
    "versionConstraint": "<=2024.1.4"
    }]
}
"""
    let expectedConfig = {
        Updates = [|
            {
                File = "testData/config.toml"
                Field = "riderSdkVersion"
                Kind = Ide "rider"
                VersionFlavor = Release
                VersionConstraint = Some (LessOrEqualTo (IdeVersion.Parse "2024.1.4"))
            }
        |]
    }
    task {
        use stream = new MemoryStream(Encoding.UTF8.GetBytes(content))
        let! config = Configuration.Read stream
        Assert.Equal(expectedConfig, config)
    }
