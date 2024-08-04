<!--
SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Contributor Guide
=================

Prerequisites
-------------
[Install .NET SDK][dotnet] version 8 or later.

Build
-----
Execute the following shell command:
```console
$ dotnet build
```

Test
----
Execute the following shell command:
```console
$ dotnet test
```

License Automation
------------------
If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```csharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: MIT
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license MIT --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```

(Feel free to attribute the changes to "intellij-updater contributors <https://github.com/ForNeVeR/intellij-updater>" instead of your name in a multi-author file, or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

[dotnet]: https://dot.net/
[reuse]: https://reuse.software/
