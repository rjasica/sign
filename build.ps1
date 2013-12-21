$ErrorActionPreference = "Stop"

.\.nuget\NuGet.exe install .\.nuget\packages.config -solutionDir "./" -NonInteractive
.\.nuget\NuGet.exe install .\sign\packages.config -solutionDir "./" -NonInteractive

msbuild .\sign.sln /p:Configuration=Release /p:Platform="Any CPU"

$versionline = Get-Content .\.nuget\packages.config | where{$_ -match "id=`"Pester`"" } | select -First 1
$regex=[regex]"(?<=version=`").*(?=`")"
$Version=$regex.Matches($versionline).Value

Import-Module .\packages\Pester.$version\tools\Pester.psm1

Invoke-Pester .\tests\Sign.Tests.ps1

$asmversion = [Reflection.Assembly]::LoadFile("$($pwd)\sign\bin\release\sign.exe").GetName().Version.ToString()

.\.nuget\NuGet.exe pack .\package.nuspec -Version $asmversion 