# SkiaSharp 4 Upgrade Runtime Parity Review

Date: 2026-06-30

## Scope

This review covers the upgrade from SkiaSharp `3.119.2` to `4.148.0`.
SkiaSharp 4.148.0 is the first stable SkiaSharp v4 release and updates the
native Skia engine to milestone m148. The release also promotes remaining
pre-v4 obsolete APIs to compile-time errors and removes obsolete enums from
reference assemblies.

## Local Validation

Commands run:

- `dotnet build src/PdfBox.Net.SkiaSharp/PdfBox.Net.SkiaSharp.csproj`
- `dotnet build src/PdfBox.Net.MauiGraphics/PdfBox.Net.MauiGraphics.csproj`
- `dotnet test tests/PdfBox.Net.Tests/PdfBox.Net.Tests.csproj --logger "console;verbosity=minimal"`
- `dotnet test tests/PdfBox.Net.Examples.Tests/PdfBox.Net.Examples.Tests.csproj --logger "console;verbosity=minimal"`
- `python3 tools/parity/runtime/run_runtime_parity.py --manifest tools/parity/runtime/corpus-manifest.txt --pdfbox-root /Users/erik/src/Repos/apache/pdfbox --java-classpath /Users/erik/src/Repos/apache/pdfbox/app/target/pdfbox-app-4.0.0-SNAPSHOT.jar --java-home /opt/homebrew/Cellar/openjdk@17/17.0.19/libexec/openjdk.jdk/Contents/Home --out-dir /tmp/pdfbox-skiasharp4-clean-runtime-parity --ratchet-baseline tools/parity/runtime/ratchet-baseline.json --gate-mode ratchet --fail-on-unexpected`

Runtime parity result before ratchet update:

- `match`: 1027
- `known`: 0
- `unexpected`: 0

After updating `tools/parity/runtime/ratchet-baseline.json`, the same corpus
passed the ratchet gate with the same `1027`/`0`/`0` result.
GitHub Actions on Linux also produced `1027`/`0`/`0`, but classified several
render rows into more specialized accepted buckets than the local macOS run.
The ratchet baseline therefore records the maximum observed count for each
accepted render-equivalence bucket across those platforms.

The local macOS run produced these accepted rendering equivalence bucket shifts
caused by the Skia engine update:

- `render-visual-equivalence-match`: 62 -> 71
- `render-form-widget-raster-equivalence-match`: 2 -> 3
- `render-foreground-shape-equivalence-match`: 38 -> 36
- `render-glyph-raster-equivalence-match`: 5 -> 4
- `render-global-resource-text-raster-equivalence-match`: 4 -> 0
- `render-image-mask-shape-equivalence-match`: 2 -> 0
- `render-lossy-jpeg-decoder-equivalence-match`: 2 -> 1

Rows whose render equivalence bucket changed:

| PDF | Previous bucket | New bucket |
|---|---|---|
| `AcroFormsRotation.pdf` | `render-foreground-shape-equivalence-match` | `render-form-widget-raster-equivalence-match` |
| `PDFBOX-2984-rotations.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBOX-3038-001033-p2.pdf` | `render-glyph-raster-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `PDFBOX-5002.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBOX-5762-722238.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBOX-5797-SO79271803.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBOX-5840-410609.pdf` | `render-image-mask-shape-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `PDFBOX-6049-ExpectedResult.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBOX-6049-Source.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf` | `render-global-resource-text-raster-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `PDFBox.GlobalResourceMergeTest.Doc01.pdf` | `render-global-resource-text-raster-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf` | `render-global-resource-text-raster-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `PDFBox.GlobalResourceMergeTest.Doc02.pdf` | `render-global-resource-text-raster-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `acroform.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `data-000001.pdf` | `render-image-mask-shape-equivalence-match` | `render-foreground-shape-equivalence-match` |
| `embedded_zip.pdf` | `render-foreground-shape-equivalence-match` | `render-visual-equivalence-match` |
| `jpeg_demo.pdf` | `render-lossy-jpeg-decoder-equivalence-match` | `render-visual-equivalence-match` |

## Assessment

The upgrade is acceptable. There are no new known or unexpected runtime parity
failures, and the observed changes are bounded to reviewed render-equivalence
buckets. The SkiaSharp backend and partial MauiGraphics backend were also moved
off the SkiaSharp v4 obsolete API warnings for bitmap drawing, path
construction, and fallback text drawing.
