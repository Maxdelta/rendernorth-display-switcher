[CmdletBinding()]
param(
    [string]$Version = '0.2.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$publishFolder = Join-Path $projectRoot 'artifacts\publish\win-x64'
$releaseRoot = Join-Path $projectRoot 'artifacts\release'
$runtimeFolder = Join-Path $releaseRoot 'launcher-runtime'
$releaseName = "RenderNorth-Display-Switcher-v$Version-win-x64"
$releaseFolder = Join-Path $releaseRoot $releaseName
$zipPath = Join-Path $releaseRoot "$releaseName.zip"

& (Join-Path $projectRoot 'publish.ps1')
if ($LASTEXITCODE -ne 0) { throw "publish.ps1 failed with exit code $LASTEXITCODE" }

if (Test-Path -LiteralPath $releaseFolder) { Remove-Item -LiteralPath $releaseFolder -Recurse -Force }
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
if (Test-Path -LiteralPath $runtimeFolder) { Remove-Item -LiteralPath $runtimeFolder -Recurse -Force }
New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null
New-Item -ItemType Directory -Path $runtimeFolder -Force | Out-Null

$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget\packages'
$launcherProjects = @(
    'Launchers\RenderNorthGameMode\RenderNorthGameMode.csproj',
    'Launchers\RenderNorthScriptMode\RenderNorthScriptMode.csproj'
)
foreach ($launcherProject in $launcherProjects) {
    dotnet publish (Join-Path $projectRoot $launcherProject) --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=false --configfile (Join-Path $projectRoot 'NuGet.Config') --output $runtimeFolder
    if ($LASTEXITCODE -ne 0) { throw "Runtime-inclusive launcher publish failed for $launcherProject with exit code $LASTEXITCODE" }
}

Copy-Item -LiteralPath (Join-Path $publishFolder 'RenderNorthDisplaySwitcher.exe') -Destination $releaseFolder
Get-ChildItem -LiteralPath $runtimeFolder -File | Copy-Item -Destination $releaseFolder

Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $releaseFolder

$quickStart = @'
RenderNorth Display Switcher v0.2.0 - Quick Start
=================================================

1. Extract the entire ZIP to a writable folder. Do not run from inside the ZIP.
2. Run RenderNorthDisplaySwitcher.exe.
3. Configure Game Mode in Windows Display Settings, then save it in the app.
4. Configure Script Mode in Windows Display Settings, then save it in the app.
5. Test both Activate buttons before using the profiles live.
6. In Stream Deck, assign System > Open actions to:
     RenderNorthGameMode.exe
     RenderNorthScriptMode.exe

Keep all files in the extracted release folder together. Required .NET 8
runtime files are included; no separate .NET installation is needed.

Profiles and logs are stored beside the application. For troubleshooting,
read README.txt and the daily file in the logs folder.
'@
Set-Content -LiteralPath (Join-Path $releaseFolder 'Quick Start.txt') -Value $quickStart -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $releaseFolder 'README.txt')

$iconFolder = Join-Path $projectRoot 'docs\stream-deck-icons'
if (Test-Path -LiteralPath $iconFolder) {
    Copy-Item -LiteralPath $iconFolder -Destination (Join-Path $releaseFolder 'Stream Deck Icons') -Recurse
}

Compress-Archive -Path (Join-Path $releaseFolder '*') -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Release folder: $releaseFolder"
Write-Host "Release ZIP: $zipPath"
