// SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module IntelliJUpdater.Augmentations

open IntelliJUpdater.Versioning

let private NextVersion year number =
    if number <= 2 then
        (year, number + 1)
    else
        (year + 1, 1)

let Augment (augmentation: Augmentation option) (entityVersion: EntityVersion): EntityVersion =
    match entityVersion, augmentation with
    | EntityVersion.Ide version, Some NextMajor ->
        let year, number =
            match version.Wave with
            | YearBased(year, number) -> year, number
            | _ -> failwithf $"Unsupported IDE version wave: {version.Wave}."
        let nextYear, nextNumber = NextVersion year number
        let wave = nextYear % 100 * 10 + nextNumber
        EntityVersion.NextMajor wave
    | _, None -> entityVersion
    | _ -> failwithf $"Unsupported entity version and augmentation: {entityVersion} with {augmentation}."
