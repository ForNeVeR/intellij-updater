// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Ide

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Threading.Tasks
open System.Xml.Linq
open System.Xml.XPath
open IntelliJUpdater.Versioning

let private GetIdeKey = function
    | IdeKind.Rider -> "rider/riderRD"
    | IdeKind.IntelliJIdeaCommunity -> "idea/ideaIC"

let private SnapshotMetadataUrl ideKey =
    Uri $"https://www.jetbrains.com/intellij-repository/snapshots/com/jetbrains/intellij/{ideKey}/maven-metadata.xml"
let private ReleaseMetadataUrl ideKey =
    Uri $"https://www.jetbrains.com/intellij-repository/releases/com/jetbrains/intellij/{ideKey}/maven-metadata.xml"

let private GetUri ideKey = function
    | Release -> ReleaseMetadataUrl ideKey
    | Nightly | UpdateFlavor.EAP -> SnapshotMetadataUrl ideKey

let private CreateFlavorFilter = function
    | Release -> fun version ->
        match version.Flavor with
        | Snapshot -> false
        | RollingEAP -> false
        | RollingEAPCandidate -> false
        | EAP _ -> true
        | RC _ -> true
        | Stable -> true
    | UpdateFlavor.EAP -> fun version ->
        match version.Flavor with
        | Snapshot -> false
        | RollingEAPCandidate | RollingEAP ->
            // For rolling EAP, the rules are interesting:
            // - consider releases 231.1111-EAP as "EAP" ones
            // - consider releases 231-EAP as "not EAP" ones, i.e. only snapshots
            // This allows to handle such updates practically, i.e. generate actual pull requests on new EAP update.
            match version.Wave with
            | YearBasedVersion(_, version) -> version > 0
            | _ -> false
        | EAP _ -> true
        | RC _ -> true
        | Stable -> true
    | Nightly -> fun _ -> true

let private CreateConstraintFilter = function
    | None -> fun _ -> true
    | Some (LessOrEqualTo other) -> fun version -> version <= other

let ReadVersionsFromStream(stream: Stream): Task<IdeVersion []> = task {
    use reader = new StreamReader(stream)
    let! content = reader.ReadToEndAsync()
    let document = XDocument.Parse content
    let versions =
        document.XPathSelectElements "//metadata//versioning//versions//version"
        |> Seq.map(fun version ->
            let version = version.Value
            printfn $"Version found: {version}"
            version
        )
        |> Seq.map IdeVersion.Parse
        |> Seq.toArray
    return versions
}

let private ReadVersions(url: Uri) = task {
    printfn $"Loading document \"{url}\"."
    let sw = Stopwatch.StartNew()

    use http = new HttpClient()
    let! response = http.GetAsync(url)
    response.EnsureSuccessStatusCode() |> ignore

    let! content = response.Content.ReadAsStreamAsync()
    let! versions = ReadVersionsFromStream content
    printfn $"Loaded and processed the document in {sw.ElapsedMilliseconds} ms."
    return versions
}

let SelectLatestVersion (flavor: UpdateFlavor)
                        (constr: IdeVersionConstraint option)
                        (versions: IdeVersion[]): IdeVersion =
    let fFilter = CreateFlavorFilter flavor
    let cFilter = CreateConstraintFilter constr

    versions
    |> Seq.filter fFilter
    |> Seq.filter cFilter
    |> Seq.max

let ReadLatestVersion (kind: IdeKind)
                      (flavor: UpdateFlavor)
                      (constr: IdeVersionConstraint option): Task<IdeVersion> = task {
    let key = GetIdeKey kind
    let uri = GetUri key flavor
    let! allVersions = ReadVersions uri
    if allVersions.Length = 0 then failwithf $"No SDK versions found for {kind}."
    return SelectLatestVersion flavor constr allVersions
}
