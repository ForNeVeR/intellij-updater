// SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Ide

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Threading.Tasks
open IntelliJUpdater.Versioning

let private GetIdeKey = function
    | IdeKind.Rider -> "rider/riderRD"
    | IdeKind.IntelliJIdeaCommunity -> "idea/ideaIC"

let internal SnapshotMetadata(ideKey: string): Uri * (IdeWave -> bool) =
    Uri $"https://www.jetbrains.com/intellij-repository/snapshots/com/jetbrains/intellij/{ideKey}/maven-metadata.xml",
    fun _ -> true
let internal ReleaseMetadata(ideKey: string): Uri * (IdeWave -> bool) =
    Uri $"https://www.jetbrains.com/intellij-repository/releases/com/jetbrains/intellij/{ideKey}/maven-metadata.xml",
    function | YearBased _ -> true | _ -> false

let private GetUris ideKey flavor =
    let releaseOnly = [| ReleaseMetadata ideKey |]
    let releaseAndSnapshot = [| SnapshotMetadata ideKey; ReleaseMetadata ideKey |]
    match flavor with
    | Release -> releaseOnly
    | Nightly | UpdateFlavor.EAP -> releaseAndSnapshot

let private CreateFlavorFilter = function
    | Release -> fun version ->
        match version.Flavor with
        | Snapshot -> false
        | RollingEAP -> false
        | RollingEAPCandidate -> false
        | EAP _ -> false
        | RC _ -> false
        | Stable -> true
    | UpdateFlavor.EAP -> fun version ->
        match version.Flavor with
        | Snapshot | RollingEAPCandidate -> false
        | RollingEAP ->
            // For rolling EAP, the rules are interesting:
            // - consider releases 231.1111-EAP as "EAP" ones
            // - consider releases 231-EAP as "not EAP" ones, i.e. only snapshots
            // This allows to handle such updates practically, i.e. generate actual pull requests on new EAP update.
            match version.Wave, version.FullVersion with
            | YearBasedVersion _, FullVersion(_, Some minor, _, _) -> minor > 0
            | _ -> false
        | EAP _ -> true
        | RC _ -> true
        | Stable -> true
    | Nightly -> fun _ -> true

let private CreateConstraintFilter allVersions = function
    | None -> fun _ -> true
    | Some (LessOrEqualTo other) -> fun version -> version <= other
    | Some LatestWave ->
        let lastWave =
            allVersions
            |> Seq.map _.Wave
            |> Seq.max
        fun v -> v.Wave = lastWave

let ReadVersionsFromStream(stream: Stream): Task<IdeVersion []> = task {
    use reader = new StreamReader(stream)
    let! content = reader.ReadToEndAsync()
    let versions =
        content
        |> Maven.ReadVersionsFromMetadata
        |> Seq.map IdeVersion.Parse
        |> Seq.toArray
    return versions
}

let private ReadVersions(url: Uri, waveFilter) = task {
    printfn $"Loading document \"{url}\"."
    let sw = Stopwatch.StartNew()

    use http = new HttpClient()
    let! response = http.GetAsync(url)
    response.EnsureSuccessStatusCode() |> ignore

    let! content = response.Content.ReadAsStreamAsync()
    let! versions = ReadVersionsFromStream content
    printfn $"Loaded and processed the document in {sw.ElapsedMilliseconds} ms."
    return versions |> Array.filter(fun x -> waveFilter x.Wave)
}

let SelectVersion (flavor: UpdateFlavor)
                  (constr: IdeVersionConstraint option)
                  (versions: IdeVersion[])
                  (order: IdeVersionOrder): IdeVersion =
    let fFilter = CreateFlavorFilter flavor

    let flavorFilteredVersions =
        versions
        |> Seq.filter fFilter
        |> ResizeArray
    let cFilter = CreateConstraintFilter flavorFilteredVersions constr
    let ordering =
        match order with
        | Oldest -> Seq.min
        | Newest -> Seq.max

    flavorFilteredVersions
    |> Seq.filter cFilter
    |> ordering

let ReadLatestVersion (kind: IdeKind)
                      (flavor: UpdateFlavor)
                      (constr: IdeVersionConstraint option)
                      (order: IdeVersionOrder): Task<IdeVersion> = task {
    let key = GetIdeKey kind
    let uris = GetUris key flavor
    let! allVersionBatches = uris |> Seq.map ReadVersions |> Task.WhenAll
    let allVersions = allVersionBatches |> Array.concat
    if allVersions.Length = 0 then failwithf $"No SDK versions found for {kind}."
    return SelectVersion flavor constr allVersions order
}
