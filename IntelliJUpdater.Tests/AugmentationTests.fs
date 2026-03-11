// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Tests.AugmentationTests

open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Theory>]
[<InlineData("2024.1", "242.*")>]
[<InlineData("2024.3", "251.*")>]
[<InlineData("261", "262.*")>]
[<InlineData("263", "271.*")>]
let ``NextMajor works correctly``(prev: string, expectedNext: string): unit =
    let actual = Augmentations.Augment (Some NextMajor) (EntityVersion.Ide <| IdeVersion.Parse prev)
    let wave = match actual with | EntityVersion.NextMajor wave -> wave | _ -> failwith "Expected NextMajor"
    Assert.Equal(expectedNext, $"{wave}.*")
