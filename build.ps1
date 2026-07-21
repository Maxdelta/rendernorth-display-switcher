[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$output = Join-Path $projectRoot 'artifacts\build'
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget\packages'

$projects = @(
    'RenderNorthDisplaySwitcher.csproj',
    'Launchers\RenderNorthGameMode\RenderNorthGameMode.csproj',
    'Launchers\RenderNorthScriptMode\RenderNorthScriptMode.csproj'
)

foreach ($project in $projects) {
    $projectPath = Join-Path $projectRoot $project
    dotnet restore $projectPath --configfile (Join-Path $projectRoot 'NuGet.Config')
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed for $project with exit code $LASTEXITCODE" }
    dotnet build $projectPath --configuration Release --no-restore --output $output
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed for $project with exit code $LASTEXITCODE" }
}
Write-Host "Build succeeded: $output"
