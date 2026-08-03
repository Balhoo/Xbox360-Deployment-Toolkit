param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'src\Xbox360DeploymentToolkit\Xbox360DeploymentToolkit.csproj'
$nugetConfig = Join-Path $PSScriptRoot 'NuGet.Config'
dotnet restore $project -r win-x64 --configfile $nugetConfig
if ($LASTEXITCODE -ne 0) { throw "dotnet restore falló con código $LASTEXITCODE" }
dotnet build $project -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build falló con código $LASTEXITCODE" }
dotnet publish $project -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --no-restore -o (Join-Path $PSScriptRoot 'dist')
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falló con código $LASTEXITCODE" }
Write-Host "Ejecutable: $(Join-Path $PSScriptRoot 'dist\Xbox360DeploymentToolkit.exe')"
