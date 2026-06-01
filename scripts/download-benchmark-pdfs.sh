#!/usr/bin/env bash
# Downloads the PDF fixtures required to run the PdfBox.Net benchmarks.
# Output directory defaults to target/pdfs (relative to the repo root), matching
# the paths used in LoadAndSave.cs, Rendering.cs and TextExtraction.cs.
#
# Usage:
#   ./scripts/download-benchmark-pdfs.sh [output-dir]
#
# The four files below are downloaded automatically.
# The Ghent PDF Output Suite (required by RenderingBenchmarks.RenderGhentCMYK*)
# is downloaded via a separate Playwright script — see scripts/download-ghent-pdf.mjs.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="${1:-$REPO_ROOT/target/pdfs}"

mkdir -p "$OUTPUT_DIR"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

sha512_of() {
    sha512sum "$1" | awk '{print $1}'
}

download_with_verify() {
    local url="$1"
    local dest="$2"
    local expected_sha512="$3"
    local filename
    filename="$(basename "$dest")"

    if [[ -f "$dest" ]]; then
        echo "Already present: $filename — verifying checksum..."
        if [[ "$(sha512_of "$dest")" == "$expected_sha512" ]]; then
            echo "  Checksum OK, skipping download."
            return 0
        else
            echo "  Checksum mismatch — re-downloading."
        fi
    else
        echo "Downloading: $filename"
    fi

    curl -fsSL --retry 3 --retry-delay 5 -o "$dest" "$url"

    local actual
    actual="$(sha512_of "$dest")"
    if [[ "$actual" != "$expected_sha512" ]]; then
        echo "ERROR: SHA-512 mismatch for $filename" >&2
        echo "  Expected: $expected_sha512" >&2
        echo "  Actual:   $actual" >&2
        rm -f "$dest"
        exit 1
    fi
    echo "  Downloaded and verified: $filename"
}

# ---------------------------------------------------------------------------
# PDF fixtures
# ---------------------------------------------------------------------------

download_with_verify \
    "https://crossasia-books.ub.uni-heidelberg.de/xasia/reader/download/849/849-42-94772-1-10-20210818.pdf" \
    "$OUTPUT_DIR/849-42-94772-1-10-20210818.pdf" \
    "78ef8c0f2a3027d44fdfb8afc63ef7dc2cac8ae8f6d35fab4a8782d1c99354a0d944ae9b38026e8a7d82c03142000f78b3715064bd4c52245d7e2feeb241654f"

download_with_verify \
    "https://crossasia-books.ub.uni-heidelberg.de/xasia/reader/download/506/506-42-86246-2-10-20190822.pdf" \
    "$OUTPUT_DIR/506-42-86246-2-10-20190822.pdf" \
    "ed2d295d0dfc702174bafd04df79ae4aaf56289f2befc981f217d3b7990e59106f8b7358fe147a9aeaf179dc1f2432c2cc064b0243c91cf18418df59be15bb96"

download_with_verify \
    "https://www.adobe.com/content/dam/acom/en/devnet/pdf/pdfs/PDF32000_2008.pdf" \
    "$OUTPUT_DIR/PDF32000_2008.pdf" \
    "690ce2177154a9526d378b0a6dec48cb2cf648fb7d3f2e43358e43e0b551a1af1b97c68e79b147c70b59c45687e7a98d5858159fca7bb93c3bb419070f7e4dae"

download_with_verify \
    "http://www.eci.org/lib/exe/fetch.php?media=downloads:altona_test_suite:eci_altona-test-suite-v2_technical2_x4.pdf" \
    "$OUTPUT_DIR/eci_altona-test-suite-v2_technical2_x4.pdf" \
    "11303a7b9c20f0fb67258715219f8cbdf4d0e52b394a16d21ab0f8517e2cb453337a216d65af35e28fabc56eafc64ed40c1ff4a4d40aef48e66168b9a3d0fc49"

# ---------------------------------------------------------------------------
# Ghent PDF Output Suite — automated via Playwright
# ---------------------------------------------------------------------------

GHENT_FILE="$OUTPUT_DIR/Ghent_PDF_Output_Suite_V50_Full/Categories/1-CMYK/Test pages/Ghent_PDF-Output-Test-V50_CMYK_X4.pdf"

if [[ -f "$GHENT_FILE" ]]; then
    echo "Ghent PDF already present, skipping."
elif command -v node &>/dev/null; then
    echo "Downloading Ghent PDF Output Suite V50 via Playwright..."
    node "$SCRIPT_DIR/download-ghent-pdf.mjs" "$OUTPUT_DIR"
else
    cat <<'EOF'

------------------------------------------------------------------------
MANUAL STEP REQUIRED — Ghent PDF Output Suite V50
------------------------------------------------------------------------
Node.js was not found; the automated Playwright download was skipped.

To download manually:
  1. Visit: https://gwg.org/download/ghentpdfoutputsuitev50/
  2. Accept the license and download the ZIP.
  3. Unpack it inside the output directory so that the path below exists:

       target/pdfs/Ghent_PDF_Output_Suite_V50_Full/Categories/1-CMYK/Test pages/Ghent_PDF-Output-Test-V50_CMYK_X4.pdf

Or install Node.js and run:
  node scripts/download-ghent-pdf.mjs

------------------------------------------------------------------------
EOF
fi

echo "PDF fixtures are in: $OUTPUT_DIR"
