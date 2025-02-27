// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Maven

open System.Xml.Linq
open System.Xml.XPath

let ReadVersionsFromMetadata(content: string): string seq =
    let document = XDocument.Parse content
    document.XPathSelectElements "//metadata//versioning//versions//version"
    |> Seq.map(fun version ->
        let version = version.Value
        printfn $"Version found: {version}"
        version
    )
