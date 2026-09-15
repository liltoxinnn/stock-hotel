<#
    Prepare l'application pour la livraison a un client.

    Produit une archive ZIP contenant un unique fichier .exe :
    le client n'a rien a installer, pas meme .NET.

    A lancer depuis Windows, dans PowerShell :
        .\publish.ps1
#>

$ErrorActionPreference = "Stop"

$publishDir = Join-Path $PSScriptRoot "publish"
$archive    = Join-Path $PSScriptRoot "GestionDeStock.zip"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $archive)    { Remove-Item $archive -Force }

dotnet publish (Join-Path $PSScriptRoot "StockManager") `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    --output $publishDir

if ($LASTEXITCODE -ne 0) { throw "La compilation a echoue." }

# Nom de fichier plus parlant pour le client.
Rename-Item (Join-Path $publishDir "StockManager.exe") "GestionDeStock.exe"

Copy-Item (Join-Path $PSScriptRoot "Lisez-moi.txt") $publishDir

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $archive

$sizeMo = (Get-Item $archive).Length / 1MB
Write-Host ""
Write-Host "Archive prete : $archive"
Write-Host ("Taille : {0:N0} Mo" -f $sizeMo)
Write-Host "Contenu : GestionDeStock.exe + Lisez-moi.txt"
