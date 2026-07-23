[CmdletBinding()]
param(
    [string]$Version = '0.4.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$publishFolder = Join-Path $projectRoot 'artifacts\publish\win-x64'
$releaseRoot = Join-Path $projectRoot 'artifacts\release'
$runtimeFolder = Join-Path $releaseRoot 'launcher-runtime'
$releaseName = "RenderNorth-Environments-Private-Beta-v$Version-win-x64-Portable-Manual-Updates"
$releaseFolder = Join-Path $releaseRoot $releaseName
$zipPath = Join-Path $releaseRoot "$releaseName.zip"
$velopackOutput = Join-Path $projectRoot 'artifacts\velopack'

& (Join-Path $projectRoot 'publish.ps1')
if ($LASTEXITCODE -ne 0) { throw "publish.ps1 failed with exit code $LASTEXITCODE" }

if (Test-Path -LiteralPath $releaseFolder) { Remove-Item -LiteralPath $releaseFolder -Recurse -Force }
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
if (Test-Path -LiteralPath $runtimeFolder) { Remove-Item -LiteralPath $runtimeFolder -Recurse -Force }
if (Test-Path -LiteralPath $velopackOutput) { Remove-Item -LiteralPath $velopackOutput -Recurse -Force }
New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null
New-Item -ItemType Directory -Path $runtimeFolder -Force | Out-Null
New-Item -ItemType Directory -Path $velopackOutput -Force | Out-Null

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

$quickStart = @"
RenderNorth Environments Private Beta v$Version - Quick Start
=================================================

SETUP EDITION (RECOMMENDED)
1. Download and run RenderNorth-Environments-Setup-v$Version.exe.
2. Open RenderNorth Environments from the Start menu.
3. Configure each layout in Windows Display Settings and save Game Mode and Script Mode.
4. In Stream Deck, add System > Open actions and select these Start menu shortcuts:
  RenderNorth Environments - Game Mode
  RenderNorth Environments - Script Mode
5. Test both buttons while watching the capture preview.
6. Use Check for Updates in the app when a newer release is available.

PORTABLE EDITION (MANUAL UPDATES)
1. Extract the entire ZIP to a writable folder. Do not run from inside the ZIP.
2. Run RenderNorthDisplaySwitcher.exe.
3. Point Stream Deck directly to RenderNorthGameMode.exe and RenderNorthScriptMode.exe
   inside the extracted folder. Keep the full folder together when replacing versions.

Keep all files in the extracted release folder together. Required .NET 8
runtime files are included; no separate .NET installation is needed.

The setup edition stores profiles and logs outside the versioned application files
so they survive updates. Read README.txt for full setup and troubleshooting details.
"@
Set-Content -LiteralPath (Join-Path $releaseFolder 'Quick Start.txt') -Value $quickStart -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $releaseFolder 'README.txt')

$iconFolder = Join-Path $projectRoot 'docs\stream-deck-icons'
if (Test-Path -LiteralPath $iconFolder) {
    Copy-Item -LiteralPath $iconFolder -Destination (Join-Path $releaseFolder 'Stream Deck Icons') -Recurse
}

Compress-Archive -Path (Join-Path $releaseFolder '*') -DestinationPath $zipPath -CompressionLevel Optimal
Copy-Item -LiteralPath $zipPath -Destination $velopackOutput

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed with exit code $LASTEXITCODE" }
dotnet tool run vpk pack --packId RenderNorth.DisplaySwitcher --packVersion $Version --packDir $releaseFolder --mainExe RenderNorthDisplaySwitcher.exe --packTitle "RenderNorth Environments Private Beta" --packAuthors RenderNorth --shortcuts None --releaseNotes (Join-Path $projectRoot "docs\releases\v$Version.md") --outputDir $velopackOutput
if ($LASTEXITCODE -ne 0) { throw "Velopack packaging failed with exit code $LASTEXITCODE" }

$setup = Get-ChildItem -LiteralPath $velopackOutput -Filter '*Setup.exe' | Select-Object -First 1
if ($null -ne $setup) { Copy-Item -LiteralPath $setup.FullName -Destination (Join-Path $velopackOutput "RenderNorth-Environments-Private-Beta-Setup-v$Version.exe") }
Copy-Item -LiteralPath $zipPath -Destination (Join-Path $velopackOutput "RenderNorth-Environments-Private-Beta-v$Version-win-x64-Portable-Manual-Updates.zip") -Force
$checksums = Get-ChildItem -LiteralPath $velopackOutput -File | ForEach-Object { "{0}  {1}" -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash,$_.Name }
Set-Content -LiteralPath (Join-Path $velopackOutput 'SHA256SUMS.txt') -Value $checksums -Encoding ASCII
Write-Host "Release folder: $releaseFolder"
Write-Host "Release ZIP: $zipPath"
Write-Host "Velopack assets: $velopackOutput"
