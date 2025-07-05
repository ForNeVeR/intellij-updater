<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Changelog
=========

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.8.0] - 2025-07-05
### Changed
- Minor updates to the project dependencies.

### Fixed
- [#76](https://github.com/ForNeVeR/intellij-updater/issues/76): `"versionFlavor": "eap"` should not return EAP-CANDIDATE builds.

## [1.7.0] - 2025-05-03
### Added
- Information on Kotlin versions used by IntelliJ Platform 252 and 221–223.

### Changed
- Update the dependencies used.

## [1.6.1] - 2025-03-04
### Fixed
- rd-gen version wasn't correctly read from the stored versioning configuration.

## [1.6.0] - 2025-02-27
### Added
- Allow to update rd-gen version.

## [1.5.0] - 2025-02-15
### Added
- Allow to detect Kotlin versions from IDE versions like `251`, not only `2025.1`.

## [1.4.0] - 2025-02-15
### Changed
- Update Kotlin for IntelliJ 2025.1 to 2.1.10.

## [1.3.0] - 2025-01-21
### Changed
- Update the application to .NET 9.

### Fixed
- [#38: Cannot find EAP versions of IDEA Community](https://github.com/ForNeVeR/intellij-updater/issues/38).

## [1.2.1] - 2024-11-21
### Changed
- Update Kotlin to 2.0.21 in IntelliJ 2024.3.
- Bump some dependencies.

## [1.2.0] - 2024-09-27
### Added
- Preliminary Kotlin version update for IntelliJ wave 2024.3.

### Changed
- Dependency version upgrades.

## [1.1.0] - 2024-08-31
### Added
- New `versionConstraint`: `latestWave`.
- New field in the update descriptor: `order`.

## [1.0.4] - 2024-08-23
### Fixed
- [#26](https://github.com/ForNeVeR/intellij-updater/issues/26): less stable releases like EAP now are able to update to more stable ones.

## [1.0.3] - 2024-08-15
### Fixed
- [#21: IDEA 2024.2.0.1 is not supported](https://github.com/ForNeVeR/intellij-updater/issues/21).

### Changed
- Minor dependency updates.

## [1.0.2] - 2024-08-06
### Fixed
- Error on printing IntelliJ EAP versions.

## [1.0.1] - 2024-08-06
### Fixed
- [#13](https://github.com/ForNeVeR/intellij-updater/issues/13): Properly process the snapshot feed of IntelliJ IDEA and choose the correct EAP builds.

## [1.0.0] - 2024-08-04
### Added
This is the initial release of the action. It supports updating of versions for Rider and IntelliJ IDEA Community.

[1.0.0]: https://github.com/ForNeVeR/intellij-updater/releases/tag/v1.0.0
[1.0.1]: https://github.com/ForNeVeR/intellij-updater/compare/v1.0.0...v1.0.1
[1.0.2]: https://github.com/ForNeVeR/intellij-updater/compare/v1.0.1...v1.0.2
[1.0.3]: https://github.com/ForNeVeR/intellij-updater/compare/v1.0.2...v1.0.3
[1.0.4]: https://github.com/ForNeVeR/intellij-updater/compare/v1.0.3...v1.0.4
[1.1.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.0.4...v1.1.0
[1.2.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.1.0...v1.2.0
[1.2.1]: https://github.com/ForNeVeR/intellij-updater/compare/v1.2.0...v1.2.1
[1.3.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.2.1...v1.3.0
[1.4.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.3.0...v1.4.0
[1.5.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.4.0...v1.5.0
[1.6.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.5.0...v1.6.0
[1.6.1]: https://github.com/ForNeVeR/intellij-updater/compare/v1.6.0...v1.6.1
[1.7.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.6.1...v1.7.0
[1.8.0]: https://github.com/ForNeVeR/intellij-updater/compare/v1.7.0...v1.8.0
[Unreleased]: https://github.com/ForNeVeR/intellij-updater/compare/v1.8.0...HEAD
