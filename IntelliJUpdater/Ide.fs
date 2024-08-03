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

let ReadLatestVersion (kind: IdeKind)
                      (flavor: UpdateFlavor)
                      (constr: IdeVersionConstraint option) : Task<IdeVersion> = task {

    let specs = failwithf "TODO"
    let kotlinKey = failwithf "TODO"
    let untilKey = failwithf "TODO"
    use http = new HttpClient()
    let readVersions (url: Uri) filter = task {
        printfn $"Loading document \"{url}\"."
        let sw = Stopwatch.StartNew()

        let! response = http.GetAsync(url)
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync()
        let document = XDocument.Parse content
        printfn $"Loaded and processed the document in {sw.ElapsedMilliseconds} ms."

        let versions =
            document.XPathSelectElements "//metadata//versioning//versions//version"
            |> Seq.toArray
        if versions.Length = 0 then failwithf "No Rider SDK versions found."
        let maxVersion =
            versions
            |> Seq.map(fun version ->
                let version = version.Value
                printfn $"Version found: {version}"
                version
            )
            |> Seq.map IdeVersion.Parse
            |> Seq.filter filter
            |> Seq.max

        return maxVersion
    }

    let! pairs =
        specs
        |> Map.toSeq
        |> Seq.map(fun(id, (url, filter)) -> task {
            let! versions = readVersions url filter
            return id, versions
        })
        |> Task.WhenAll

    let ideVersions = Map.ofArray pairs
    let ideVersionForKotlin = ideVersions |> Map.find kotlinKey
    let ideVersionForUntilBuild = ideVersions |> Map.find untilKey

    return failwithf "TODO"
}
