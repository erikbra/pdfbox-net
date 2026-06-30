# SkiaSharp 4 DCT Image Sampling Parity Follow-Up

Date: 2026-06-30

Issue: #583

## Scope

This follow-up investigated whether SkiaSharp 4 sampling APIs could move PDFBox.Net
rendering closer to Java PDFBox after the SkiaSharp 4.148.0 upgrade.

The candidate area was sampled image rendering. Java PDFBox defaults to high-quality
image interpolation and only switches to nearest-neighbor for non-interpolated images
when they are scaled up. PDFBox.Net already preserved the nearest-neighbor scaled-up
case, but used linear/mipmap sampling for the general SkiaSharp image path.

## Options Tested

### Pattern shader sampling

`SKShader.CreateBitmap(...)` was experimentally replaced with
`SKShader.CreateImage(..., SKSamplingOptions, ...)` for tiling pattern shaders.

Result: no targeted corpus render output changed. This was rejected as a cosmetic API
change with no measured parity value.

### Global cubic image sampling

Both `SKCubicResampler.CatmullRom` and `SKCubicResampler.Mitchell` were tested as the
global image sampling default.

Targeted image rows improved, but the full runtime corpus produced three unexpected
render detail mismatches:

- `JBIG2Image.pdf`
- `ccitt4-cib-test.pdf`
- `sample_fonts_solidconvertor.pdf`

Result: rejected. A global cubic default improves color image rows but regresses
bitonal/font-sensitive rendering.

### DCT-only Mitchell sampling

DCTDecode image XObjects now use `SKCubicResampler.Mitchell`; all other images keep
the existing linear/mipmap sampling. The existing nearest-neighbor scaled-up rule for
`/Interpolate false` remains higher priority.

Result: accepted. This is the narrowest change that improved JPEG parity without
introducing unexpected corpus differences.

## Validation

Targeted manifest:

- `acroform.pdf`
- `AcroFormsRotation.pdf`
- `data-000001.pdf`
- `jpeg_demo.pdf`
- `jpegrgb.pdf`
- `PDFBOX-5840-410609.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc01.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc02.pdf`
- `raw_image_demo.pdf`
- `survey.pdf`
- `tiger-as-form-xobject.pdf`

Targeted result:

- `match`: 84
- `known`: 0
- `unexpected`: 0

Full runtime corpus result before ratchet update:

- `match`: 1027
- `known`: 0
- `unexpected`: 0

The ratchet failed only because `render-visual-equivalence-match` increased from 71
to 72 after the JPEG row moved into the generic visual-equivalence bucket. The
accepted `render-lossy-jpeg-decoder-equivalence-match` ceiling was left in place as
part of the cross-platform envelope, even though the local macOS run produced zero
rows in that category.

## Notable Targeted Metric Changes

| PDF | Previous category | New category | Previous mean/RMS | New mean/RMS |
|---|---|---|---:|---:|
| `jpeg_demo.pdf` | `render-visual-equivalence-match` | `render-visual-equivalence-match` | 0.584745 / 1.782258 | 0.296877 / 1.287267 |
| `jpegrgb.pdf` | `render-lossy-jpeg-decoder-equivalence-match` | `render-visual-equivalence-match` | 1.334478 / 5.589949 | 0.217570 / 1.076207 |
| `acroform.pdf` | `render-visual-equivalence-match` | `render-visual-equivalence-match` | 0.199371 / 2.620312 | 0.202893 / 2.654111 |

The `acroform.pdf` DCT-backed form row drifted slightly farther from Java but remained
inside the same accepted visual-equivalence category. The JPEG-specific improvements
were substantially larger than that small regression.

## Decision

Use cubic Mitchell sampling only for DCTDecode image XObjects. Do not broaden this to
all images or pattern shaders without new corpus evidence.
