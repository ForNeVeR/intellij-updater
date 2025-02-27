// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.RdGen

open System
open System.Net.Http
open System.Threading.Tasks
open IntelliJUpdater.Versioning

let ForIde(wave: IdeWave): Task<Version> =
    let waveYear = wave.NormalizedMajorNumber
    let major = 2000 + waveYear / 10
    let minor = waveYear % 10
    task {
        use httpClient = new HttpClient()
        let! response = httpClient.GetAsync "https://repo1.maven.org/maven2/com/jetbrains/rd/rd-gen/maven-metadata.xml"
        let! content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync()
        let versions = Maven.ReadVersionsFromMetadata content |> Seq.map Version.Parse |> Seq.sort
        let filteredVersions = versions |> Seq.filter(fun v -> v.Major = major && v.Minor = minor) |> Seq.cache
        return
            match Seq.tryLast filteredVersions with
            | Some version -> version
            | None -> Seq.last versions
    }
