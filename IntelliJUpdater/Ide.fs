// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Ide

open System
open System.Diagnostics
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
        | EAP _ -> true
        | RC _ -> true
        | Stable -> true
    | UpdateFlavor.EAP -> fun version ->
        match version.Flavor with
        | Snapshot -> false
        | EAP _ -> true
        | RC _ -> true
        | Stable -> true
    | Nightly -> fun _ -> true

let private CreateConstraintFilter = function
    | None -> fun _ -> true
    | Some (LessOrEqualTo other) -> fun version -> version <= other

let private ReadVersions(url: Uri) = task {
    printfn $"Loading document \"{url}\"."
    let sw = Stopwatch.StartNew()

    use http = new HttpClient()
    let! response = http.GetAsync(url)
    response.EnsureSuccessStatusCode() |> ignore

    let! content = response.Content.ReadAsStringAsync()
    let document = XDocument.Parse content
    printfn $"Loaded and processed the document in {sw.ElapsedMilliseconds} ms."

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

let ReadLatestVersion (kind: IdeKind)
                      (flavor: UpdateFlavor)
                      (constr: IdeVersionConstraint option) : Task<IdeVersion> = task {
    let key = GetIdeKey kind
    let uri = GetUri key flavor
    let fFilter = CreateFlavorFilter flavor
    let cFilter = CreateConstraintFilter constr
    let! allVersions = ReadVersions uri
    if allVersions.Length = 0 then failwithf $"No SDK versions found for {kind}."

    let maxVersion =
        allVersions
        |> Seq.filter fFilter
        |> Seq.filter cFilter
        |> Seq.max

    return maxVersion
}
