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
    $expectedContent = (Get-Content -LiteralPath $expected -Raw).Trim().ReplaceLineEndings("`n")
    $actualContent = (Get-Content -LiteralPath $actual -Raw).Trim().ReplaceLineEndings("`n")

    if ($expectedContent -ne $actualContent) {
        Write-Output "Files $expected and $actual are different. Expected:"
        Write-Output $expectedContent
        Write-Output "Actual:"
        Write-Output $actualContent

        throw "Differences detected."
    }
}

CompareFiles $ExpectedFile $ActualFile
foreach ($actualFile in $AdditionalFiles) {
    $expectedFile = [IO.Path]::ChangeExtension($actualFile, '.gold' + [IO.Path]::GetExtension($actualFile))
    CompareFiles $expectedFile $actualFile
}
