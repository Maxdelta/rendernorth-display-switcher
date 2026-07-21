[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$output = Join-Path $projectRoot 'artifacts\publish\win-x64'
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget\packages'

dotnet publish (Join-Path $projectRoot 'RenderNorthDisplaySwitcher.csproj') --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true --configfile (Join-Path $projectRoot 'NuGet.Config') --output $output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
Write-Host "Published: $(Join-Path $output 'RenderNorthDisplaySwitcher.exe')"
