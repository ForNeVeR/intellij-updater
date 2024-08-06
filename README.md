<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

intellij-updater [![Status Aquana][status-aquana]][andivionian-status-classifier]
================
This is a small tool that will help you to manage IntelliJ-based IDE version update in a plugin repository.

Missing a feature? Report to [the issues][issues].

Want to contribute? Check out [the contributing guide][docs.contributing].

Usage
-----
### Example
See a live example [in AvaloniaRider repository][example.avalonia-rider].

Add a following file, `intellij-updater.json`, to your repository:
```json
{
    "updates": [{
        "file": "gradle/libs.versions.toml",
        "field": "riderSdk",
        "kind": "rider",
        "versionFlavor": "release"
    }, {
        "file": "gradle/libs.versions.toml",
        "field": "riderSdkPreview",
        "kind": "rider",
        "versionFlavor": "eap"
    }, {
        "file": "gradle.properties",
        "field": "untilBuildVersion",
        "kind": "rider",
        "versionFlavor": "release",
        "augmentation": "nextMajor"
    }],
    "prBodyPrefix": "## Maintainer Note\n> [!WARNING]\n> This PR will not trigger CI by default. Please **close it and reopen manually** to trigger the CI.\n>\n> Unfortunately, this is a consequence of the current GitHub Action security model (by default, PRs created automatically aren't allowed to trigger other automation)."
}
```

And then start it periodically on CI and make it generate a PR using any PR-generating action you like, e.g.:
```yaml
name: "Dependency Checker"
on:
  schedule:
    - cron: '0 0 * * *' # Every day
  push:
    branches:
      - main
  pull_request:
    branches:
      - main
  workflow_dispatch:

jobs:
  main:
    permissions:
      contents: write
      pull-requests: write

    runs-on: ubuntu-22.04
    timeout-minutes: 15
    steps:
      - name: "Check out the sources"
        uses: actions/checkout@v4

      - id: update
        uses: ForNeVeR/intellij-updater@v1
        name: "Update the dependency versions"
        with:
          config-file: ./intellij-updater.json # the default

      - if: steps.update.outputs.has-changes == 'true' && (github.event_name == 'schedule' || github.event_name == 'workflow_dispatch')
        name: "Create a PR"
        uses: peter-evans/create-pull-request@v6
        with:
          branch: ${{ steps.update.outputs.branch-name }}
          author: "Automation <your@email>"
          commit-message: ${{ steps.update.outputs.commit-message }}
          title: ${{ steps.update.outputs.pr-title }}
          body-path: ${{ steps.update.outputs.pr-body-path }}
```

This will perform the following operation every day:
1. Check the latest versions of Rider available in the JetBrains Maven repository.
2. Update the `riderSdk` to the latest stable Rider version and `riderSdkPreview` to the latest EAP Rider version in the `config.toml` file.
3. Update the `untilBuildVersion` to the _next major Rider wave_ in the `gradle.properties` file (e.g. if the current in 2024.1 aka 241, then it will be updated to `242.*`, for your plugin to be auto-compatible with the next version).
4. Create a PR with the changes (if started manually or by schedule; otherwise, a "dry run" is performed, where the action checks the validity of the current configuration).

> [!NOTE]
> The `prBodyPrefix` value in this example is added to the pull request title. In this example, we are adding a note for the maintainer to bootstrap the CI manually, because this is one of the current recommendations from the [create-pull-request][] action. This is not inherently required by this action: follow the recommendation of the PR-creating action of your choice.

### Configuration
The action itself accepts only one optional parameter: `config-file`. If not passed, it will default to `./intellij-updater.json`.

The configuration file spec:
```json
{
    "updates": [
        {
            "file": "File path relative to this config file's parent directory. Accepts .toml or Java .properties files.",
            "field": "Field in the configuration file. Only field name, no sections or structure. Action includes an extremely simple parser for supported file formats and doesn't support any kind of disambiguation in case there are several identically-named properties.",
            "kind": "kotlin | intellij-idea-community | rider",
            "versionFlavor": "release | eap | nightly",
            "versionConstraint": "<=SomeValidVersion (only one kind of constraint is supported for now)",
            "augmentation": "optional field, might contain 'nextMajor'"
        }
    ],
    "prBodyPrefix": "optional string"
}
```

A `kind` of `kotlin` will update the corresponding field to the correct Kotlin version used by a particular IDE version, see [this table][intellij.kotlin] for details.

A more detailed description of the `versionFlavor` field:
- `release` takes the latest _stable_ IDE version released (no EAP, no preview, no snapshot);
- `nightly` takes the latest possible version, no questions asked;
- `eap` takes the latest _numbered_ EAP version, meaning it won't take `231-EAP-SNAPSHOT` from IntelliJ (because these are not numbered).

The point of this is to have a tested PR each time a new EAP IDE version is released, to avoid silent snapshot updates breaking compilation and tests.

### Parameters
Action's output parameters are documented in [the action file itself][action-yml], check the `outputs` section.


### Notes
Please note that this action installs .NET SDK during its execution. It's recommended to isolate it from other build steps in your CI.

Documentation
-------------
- [Changelog][docs.changelog]
- [Contributor Guide][docs.contributing]
- [Maintainer Guid][docs.maintaining]

License
-------
The project is distributed under the terms of [the MIT license][docs.license]
(unless a particular file states otherwise).

The license indication in the project's sources is compliant with the [REUSE specification v3.2][reuse.spec].

[action-yml]: action.yml
[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-aquana-
[create-pull-request]: https://github.com/peter-evans/create-pull-request
[docs.changelog]: CHANGELOG.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.md
[docs.maintaining]: MAINTAINING.md
[example.avalonia-rider]: https://github.com/ForNeVeR/AvaloniaRider/blob/HEAD/.github/workflows/dependencies.yml
[intellij.kotlin]: https://plugins.jetbrains.com/docs/intellij/using-kotlin.html#kotlin-standard-library
[issues]: https://github.com/ForNeVeR/intellij-updater/issues
[reuse.spec]: https://reuse.software/spec-3.2/
[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
