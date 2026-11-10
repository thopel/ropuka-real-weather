param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir,

    [Parameter(Mandatory = $true)]
    [string]$BepInExDir,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\RopukaRealWeather\RopukaRealWeather.csproj"
$tests = Join-Path $repoRoot "tests\RopukaRealWeather.Tests\RopukaRealWeather.Tests.csproj"
$artifacts = Join-Path $repoRoot "artifacts"
$stage = Join-Path $artifacts "RopukaRealWeather"
$pluginStage = Join-Path $stage "BepInEx\plugins\RopukaRealWeather"

if (-not (Test-Path -LiteralPath (Join-Path $GameDir "RopukaIdleIsland.exe"))) {
    throw "GameDir does not contain RopukaIdleIsland.exe."
}
if (-not (Test-Path -LiteralPath (Join-Path $BepInExDir "core\BepInEx.dll"))) {
    throw "BepInExDir does not contain core\BepInEx.dll."
}

$properties = @(
    "-p:GameDir=$GameDir",
    "-p:BepInExDir=$BepInExDir"
)

dotnet test $tests -c $Configuration @properties
if ($LASTEXITCODE -ne 0) { throw "Tests failed." }

dotnet build $project -c $Configuration @properties
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

if (Test-Path -LiteralPath $stage) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStage -Force | Out-Null

$dll = Join-Path $repoRoot "src\RopukaRealWeather\bin\$Configuration\netstandard2.1\RopukaRealWeather.dll"
Copy-Item -LiteralPath $dll -Destination $pluginStage
Copy-Item -LiteralPath (Join-Path $repoRoot "src\RopukaRealWeather\com.ropuka.realweather.cfg.example") -Destination $pluginStage
Copy-Item -LiteralPath (Join-Path $repoRoot "README.md") -Destination $stage
Copy-Item -LiteralPath (Join-Path $repoRoot "LICENSE") -Destination $stage
Copy-Item -LiteralPath (Join-Path $repoRoot "CHANGELOG.md") -Destination $stage

$archive = Join-Path $artifacts "RopukaRealWeather.zip"
if (Test-Path -LiteralPath $archive) {
    Remove-Item -LiteralPath $archive -Force
}
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $archive -CompressionLevel Optimal
Write-Host "Release archive: $archive"

