<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

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
4. Make sure there's a properly formed version entry in the `CHANGELOG.md` file.
5. Merge the changes via a pull request.
6. Push a tag named `v<VERSION>` to GitHub.
7. Create a new [release][releases] on GitHub.
   - Release title should be "ChangelogAutomation.action v<VERSION>"
   - Release notes could be copy-pasted from the `CHANGELOG.md` file
8. Make sure to also update the current rolling major release tag (say, `v1`) to the new version.

[semver]: https://semver.org/spec/v2.0.0.html
[releases]: https://github.com/ForNeVeR/intellij-updater/releases
