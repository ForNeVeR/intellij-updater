<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Maintainer Guide
================

Release
-------

To release a new version:
1. Update the project status in the `README.md` file, if required.
2. Update the copyright year in the `LICENSE.md` file, if required.
3. Choose the new version according to [Semantic Versioning][semver]. It should consist of three numbers (e.g. `1.0.0`).
4. Update the `version` field in the `Directory.Build.props` file.
5. Update the action version (`ForNeVeR/intellij-updater@`) in the `README.md` file, if required.
6. Make sure there's a properly formed version entry in the `CHANGELOG.md` file.
7. Merge the changes via a pull request.
8. Push a tag named `v<VERSION>` to GitHub.
9. GitHub Actions will do the rest.

[semver]: https://semver.org/spec/v2.0.0.html
