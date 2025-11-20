# ---------------------------------------------
# CreateCleanZip.ps1
# Zips the solution and project folders excluding bin/obj/.vs
# Run from the solution root directory.
# ---------------------------------------------

$ErrorActionPreference = "Stop"

# Define paths relative to script location
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$includePaths = @(
    "AdventureGame.slnx",
    "AdventureGame",
    "AdventureGame.Engine",
    "AdventureGame.Engine.Tests"
)

$zipName = "AdventureGame-Clean.zip"
$zipPath = Join-Path $root $zipName

# Delete existing zip
if (Test-Path $zipPath) {
    Write-Host "Deleting previous zip: $zipPath"
    Remove-Item $zipPath -Force
}

Write-Host "Creating zip: $zipName"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")

foreach ($path in $includePaths) {

    $fullPath = Join-Path $root $path

    if (!(Test-Path $fullPath)) {
        Write-Host "⚠️ Skipping missing path: $path"
        continue
    }

    # Collect all files except bin/obj/.vs
    $files = Get-ChildItem $fullPath -Recurse -File |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
            $_.FullName -notmatch "\\obj\\" -and
            $_.FullName -notmatch "\\.vs\\"
        }

    foreach ($file in $files) {
        # Build the relative path for proper folder structure in the zip
        $relative = $file.FullName.Substring($root.Length + 1)
        Write-Host "Adding $relative"

        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, 
            $file.FullName, 
            $relative
        ) | Out-Null
    }
}

$zip.Dispose()

Write-Host "---------------------------------------"
Write-Host "Zip created successfully: $zipPath"
Write-Host "---------------------------------------"
