param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'src\Xbox360DeploymentToolkit\Xbox360DeploymentToolkit.csproj'
dotnet restore $project
dotnet build $project -c $Configuration --no-restore
dotnet publish $project -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --no-restore -o (Join-Path $PSScriptRoot 'dist')
Write-Host "Ejecutable: $(Join-Path $PSScriptRoot 'dist\Xbox360DeploymentToolkit.exe')"
