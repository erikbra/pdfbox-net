<#
.SYNOPSIS
    Downloads the PDF fixtures required to run the PdfBox.Net benchmarks.

.DESCRIPTION
    Downloads benchmark PDF files to the target/pdfs directory (or a path you
    specify), verifying each file against its expected SHA-512 checksum.

    The Ghent PDF Output Suite cannot be downloaded automatically because it
    requires accepting a license agreement. Instructions are printed at the end.

.PARAMETER OutputDir
    Destination directory for downloaded PDFs.
    Defaults to "target/pdfs" relative to the repository root.

.EXAMPLE
    .\scripts\download-benchmark-pdfs.ps1
    .\scripts\download-benchmark-pdfs.ps1 -OutputDir C:\bench\pdfs
#>
param(
    [string]$OutputDir = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "target\pdfs"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Get-Sha512([string]$filePath) {
    $hash = Get-FileHash -Algorithm SHA512 -Path $filePath
    return $hash.Hash.ToLowerInvariant()
}

function Download-WithVerify([string]$url, [string]$dest, [string]$expectedSha512) {
    $filename = Split-Path $dest -Leaf

    if (Test-Path $dest) {
        Write-Host "Already present: $filename — verifying checksum..."
        $actual = Get-Sha512 $dest
        if ($actual -eq $expectedSha512) {
            Write-Host "  Checksum OK, skipping download."
            return
        }
        Write-Host "  Checksum mismatch — re-downloading."
    } else {
        Write-Host "Downloading: $filename"
    }

    $tempFile = "$dest.tmp"
    try {
        Invoke-WebRequest -Uri $url -OutFile $tempFile -UseBasicParsing
        $actual = Get-Sha512 $tempFile
        if ($actual -ne $expectedSha512) {
            throw "SHA-512 mismatch for ${filename}:`n  Expected: $expectedSha512`n  Actual:   $actual"
        }
        Move-Item -Force $tempFile $dest
        Write-Host "  Downloaded and verified: $filename"
    } catch {
        Remove-Item -Force -ErrorAction SilentlyContinue $tempFile
        throw
    }
}

# ---------------------------------------------------------------------------
# PDF fixtures
# ---------------------------------------------------------------------------

Download-WithVerify `
    "https://crossasia-books.ub.uni-heidelberg.de/xasia/reader/download/849/849-42-94772-1-10-20210818.pdf" `
    (Join-Path $OutputDir "849-42-94772-1-10-20210818.pdf") `
    "78ef8c0f2a3027d44fdfb8afc63ef7dc2cac8ae8f6d35fab4a8782d1c99354a0d944ae9b38026e8a7d82c03142000f78b3715064bd4c52245d7e2feeb241654f"

Download-WithVerify `
    "https://crossasia-books.ub.uni-heidelberg.de/xasia/reader/download/506/506-42-86246-2-10-20190822.pdf" `
    (Join-Path $OutputDir "506-42-86246-2-10-20190822.pdf") `
    "ed2d295d0dfc702174bafd04df79ae4aaf56289f2befc981f217d3b7990e59106f8b7358fe147a9aeaf179dc1f2432c2cc064b0243c91cf18418df59be15bb96"

Download-WithVerify `
    "https://www.adobe.com/content/dam/acom/en/devnet/pdf/pdfs/PDF32000_2008.pdf" `
    (Join-Path $OutputDir "PDF32000_2008.pdf") `
    "690ce2177154a9526d378b0a6dec48cb2cf648fb7d3f2e43358e43e0b551a1af1b97c68e79b147c70b59c45687e7a98d5858159fca7bb93c3bb419070f7e4dae"

Download-WithVerify `
    "http://www.eci.org/lib/exe/fetch.php?media=downloads:altona_test_suite:eci_altona-test-suite-v2_technical2_x4.pdf" `
    (Join-Path $OutputDir "eci_altona-test-suite-v2_technical2_x4.pdf") `
    "11303a7b9c20f0fb67258715219f8cbdf4d0e52b394a16d21ab0f8517e2cb453337a216d65af35e28fabc56eafc64ed40c1ff4a4d40aef48e66168b9a3d0fc49"

# ---------------------------------------------------------------------------
# Ghent PDF Output Suite (manual step)
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "------------------------------------------------------------------------"
Write-Host "MANUAL STEP REQUIRED — Ghent PDF Output Suite V50"
Write-Host "------------------------------------------------------------------------"
Write-Host "The Ghent PDF Output Suite cannot be downloaded automatically because it"
Write-Host "requires accepting a license agreement."
Write-Host ""
Write-Host "To enable RenderingBenchmarks.RenderGhentCMYK / RenderGhentCMYKNoOutput:"
Write-Host ""
Write-Host "  1. Visit: https://gwg.org/download/ghentpdfoutputsuitev50/"
Write-Host "  2. Accept the license and download the ZIP."
Write-Host "  3. Unpack it inside the output directory so that the path below exists:"
Write-Host ""
Write-Host "       target\pdfs\Ghent_PDF_Output_Suite_V50_Full\Categories\1-CMYK\Test pages\Ghent_PDF-Output-Test-V50_CMYK_X4.pdf"
Write-Host ""
Write-Host "------------------------------------------------------------------------"
Write-Host ""
Write-Host "All automatically downloadable PDFs are in: $OutputDir"
