[CmdletBinding()]
param(
    [string]$Version = '0.3.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$publishFolder = Join-Path $projectRoot 'artifacts\publish\win-x64'
$releaseRoot = Join-Path $projectRoot 'artifacts\release'
$runtimeFolder = Join-Path $releaseRoot 'launcher-runtime'
$releaseName = "RenderNorth-Display-Switcher-v$Version-win-x64-portable"
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

$quickStart = @'
RenderNorth Display Switcher v0.3.0 - Quick Start
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
Copy-Item -LiteralPath $zipPath -Destination $velopackOutput

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed with exit code $LASTEXITCODE" }
dotnet tool run vpk pack --packId RenderNorth.DisplaySwitcher --packVersion $Version --packDir $releaseFolder --mainExe RenderNorthDisplaySwitcher.exe --packTitle "RenderNorth Display Switcher" --packAuthors RenderNorth --outputDir $velopackOutput
if ($LASTEXITCODE -ne 0) { throw "Velopack packaging failed with exit code $LASTEXITCODE" }

$setup = Get-ChildItem -LiteralPath $velopackOutput -Filter '*Setup.exe' | Select-Object -First 1
if ($null -ne $setup) { Copy-Item -LiteralPath $setup.FullName -Destination (Join-Path $velopackOutput "RenderNorth-Display-Switcher-Setup-v$Version.exe") }
$checksums = Get-ChildItem -LiteralPath $velopackOutput -File | ForEach-Object { "{0}  {1}" -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash,$_.Name }
Set-Content -LiteralPath (Join-Path $velopackOutput 'SHA256SUMS.txt') -Value $checksums -Encoding ASCII
Write-Host "Release folder: $releaseFolder"
Write-Host "Release ZIP: $zipPath"
Write-Host "Velopack assets: $velopackOutput"
