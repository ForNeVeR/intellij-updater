# SPDX-FileCopyrightText: 2024 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: MIT

param (
    [Parameter(Mandatory = $true)]
    [string] $ExpectedFile,
    [Parameter(Mandatory = $true)]
    [string] $ActualFile,
    [Parameter(Mandatory = $true)]
    [string[]] $AdditionalFiles
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function CompareFiles($expected, $actual) {
    Write-Output "Comparing files $expected and $actual"
    $expectedContent = Get-Content -LiteralPath $expected
    $actualContent = Get-Content -LiteralPath $actual
    $diff = Compare-Object -ReferenceObject $expectedContent -DifferenceObject $actualContent -SyncWindow 0

    if ($diff) {
        Write-Output "Files $expected and $actual are different. Expected:"
        Write-Output $expectedContent

        throw "Differences detected."
    }
}

CompareFiles $ExpectedFile $ActualFile
foreach ($actualFile in $AdditionalFiles) {
    $expectedFile = [IO.Path]::ChangeExtension($actualFile, '.gold' + [IO.Path]::GetExtension($actualFile))
    CompareFiles $expectedFile $actualFile
}
