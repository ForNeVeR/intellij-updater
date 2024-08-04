module IntelliJUpdater.Tests.AugmentationTests

open IntelliJUpdater
open IntelliJUpdater.Versioning
open Xunit

[<Theory>]
[<InlineData("2024.1", "242.*")>]
[<InlineData("2024.3", "251.*")>]
let ``NextMajor works correctly``(prev: string, expectedNext: string): unit =
    let actual = Augmentations.Augment (Some NextMajor) (EntityVersion.Ide <| IdeVersion.Parse prev)
    let wave = match actual with | EntityVersion.NextMajor wave -> wave | _ -> failwith "Expected NextMajor"
    Assert.Equal(expectedNext, $"{wave}.*")
