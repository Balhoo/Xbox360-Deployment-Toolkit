param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot 'Xbox360DeploymentToolkit.sln'
$project = Join-Path $PSScriptRoot 'src\Xbox360DeploymentToolkit\Xbox360DeploymentToolkit.csproj'
$nugetConfig = Join-Path $PSScriptRoot 'NuGet.Config'
dotnet restore $solution --configfile $nugetConfig
if ($LASTEXITCODE -ne 0) { throw "dotnet restore falló con código $LASTEXITCODE" }
dotnet test $solution -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet test falló con código $LASTEXITCODE" }
dotnet publish $project -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --no-restore -o (Join-Path $PSScriptRoot 'dist')
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falló con código $LASTEXITCODE" }
Write-Host "Ejecutable: $(Join-Path $PSScriptRoot 'dist\Xbox360DeploymentToolkit.exe')"
