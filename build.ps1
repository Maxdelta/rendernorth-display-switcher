[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$output = Join-Path $projectRoot 'artifacts\build'
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget\packages'

dotnet restore (Join-Path $projectRoot 'RenderNorthDisplaySwitcher.csproj') --configfile (Join-Path $projectRoot 'NuGet.Config')
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }
dotnet build (Join-Path $projectRoot 'RenderNorthDisplaySwitcher.csproj') --configuration Release --no-restore --output $output
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
Write-Host "Build succeeded: $output"
