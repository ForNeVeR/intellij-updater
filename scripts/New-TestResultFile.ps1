# SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: MIT

param (
    [Parameter(Mandatory = $true)]
    [string] $HasChanges,
    [Parameter(Mandatory = $true)]
    [string] $BranchName,
    [Parameter(Mandatory = $true)]
    [string] $AuthorName,
    [Parameter(Mandatory = $true)]
    [string] $CommitMessage,
    [Parameter(Mandatory = $true)]
    [string] $PrTitle,
    [Parameter(Mandatory = $true)]
    [string] $PrBodyPath,
    [Parameter(Mandatory = $true)]
    [string] $OutFile
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$prBody = Get-Content -LiteralPath $PrBodyPath

@"
has-changes=$HasChanges
branch-name=$BranchName
author-name=$AuthorName
commit-message=$CommitMessage
pr-title=$PrTitle
pr-body=$PrBody
"@ | Out-File -LiteralPath $OutFile
