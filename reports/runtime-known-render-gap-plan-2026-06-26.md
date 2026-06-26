# Runtime Known Render Gap Closure Plan

Generated UTC: 2026-06-26

## Scope

This report records the current Java-vs-.NET runtime parity state for the
remaining known gaps in PdfBox.Net and proposes the issue breakdown needed to
drive the runtime gate to zero known divergences.

Inputs:

- PdfBox.Net checkout: `/Users/erik/src/Repos/erikbra/pdfbox-net`
- Apache PDFBox checkout: `/Users/erik/src/Repos/apache/pdfbox`
- Apache app jar: `/Users/erik/src/Repos/apache/pdfbox/app/target/pdfbox-app-4.0.0-SNAPSHOT.jar`
- Corpus manifest: `tools/parity/runtime/corpus-manifest.txt`
- Known-failure ledger: `tools/parity/runtime/known-failures.json`
- Ratchet baseline: `tools/parity/runtime/ratchet-baseline.json`
- Local output directory: `/tmp/pdfbox-net-known-gap-plan-2026-06-26`

Command used:

```bash
python3 tools/parity/runtime/run_runtime_parity.py \
  --manifest tools/parity/runtime/corpus-manifest.txt \
  --pdfbox-root /Users/erik/src/Repos/apache/pdfbox \
  --java-classpath /Users/erik/src/Repos/apache/pdfbox/app/target/pdfbox-app-4.0.0-SNAPSHOT.jar \
  --java-home /opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home \
  --known-failures tools/parity/runtime/known-failures.json \
  --corpus-categories tools/parity/runtime/corpus-categories.json \
  --ratchet-baseline tools/parity/runtime/ratchet-baseline.json \
  --gate-mode ratchet \
  --out-dir /tmp/pdfbox-net-known-gap-plan-2026-06-26 \
  --fail-on-unexpected
```

The Java probe emitted diagnostic stack frames for malformed corpus documents,
but the harness collapsed them into one summary line and wrote the full detail
to `/tmp/pdfbox-net-known-gap-plan-2026-06-26/java-ignored-output.txt`.

## Runtime Summary

| Metric | Count |
|---|---:|
| `match` | 976 |
| `known` | 51 |
| `unexpected` | 0 |

Corpus scale:

| Item | Count |
|---|---:|
| PDFs | 161 |
| Merge pairs | 80 |

All remaining known rows are render `detail-mismatch` rows. There are no text
mismatches, no render placeholders, no unexpected divergences, and all
successful save and merge rows are byte-identical or structurally equivalent.

The checked-in ratchet currently permits `known <= 53` and
`detail-mismatch <= 53`. This local run is at 51. CI has previously shown 52
known rows, so the extra hosted row should be identified from CI artifacts
before setting the next baseline.

## Known Rows By Corpus Category

| Corpus category | Known rows |
|---|---:|
| `uncategorized` | 28 |
| `rendering` | 7 |
| `merge/split/save` | 5 |
| `forms` | 4 |
| `malformed PDFs` | 3 |
| `text extraction` | 2 |
| `encryption` | 1 |
| `fonts and subsetting` | 1 |

## Source Findings

The current source inventory is not the blocker. The upstream Java source is
mapped, and the unsupported API audit passes.

Remaining work is implementation fidelity in already-ported code:

- `src/PdfBox.Net/PDModel/Graphics/Image/SampledImageReader.cs` is still marked
  as an adapted stub and only supports 1- to 8-bit sampled images.
- `src/PdfBox.Net/Rendering/PageDrawer.cs` still has simplified behavior for
  transfer functions, shading fills, annotation handling, form XObjects,
  transparency groups, blend-mode discovery, and gray detection compared with
  Apache PDFBox `PageDrawer.java`.
- `src/PdfBox.Net/PDModel/Interactive/Form/AppearanceGeneratorHelper.cs`
  generates usable widget appearances, but the remaining form fixtures indicate
  incomplete Java parity for auto font sizing, multiline wrapping, baseline
  placement, rotation, and default appearance resource behavior.

## Gap Workstreams

### 1. Measurement, CI Variance, And Ratchet

Purpose:

- Make every known row traceable to a root-cause bucket instead of the current
  broad `render-quality-gaps` allowance.
- Identify why local parity reports 51 known rows while CI can report 52.
- Lower `tools/parity/runtime/ratchet-baseline.json` after each closure.

Acceptance criteria:

- The report tooling emits known rows grouped by root cause, file, operation,
  corpus category, Java/.NET image metrics, and artifact paths.
- The CI-only extra row, if still present, is either fixed or documented as
  environment variance with a dedicated owner.
- `known` and `detail-mismatch` ratchets are lowered whenever a gap PR lands.

### 2. Form And Widget Appearance Rendering

Representative fixtures:

- `AcroFormsBasicFields.pdf`
- `AcroFormsRotation.pdf`
- `Acroform-PDFBOX-2333.pdf`
- `MultilineFields.pdf`
- `PDFBOX-3835-input-acrobat-wrap.pdf`
- `PDFBOX3812-acrobat-multiline-auto.pdf`
- `acroform.pdf`
- `eu-001.pdf`

Likely source areas:

- `AppearanceGeneratorHelper`
- `PlainTextFormatter`
- `PDAppearanceContentStream`
- widget annotation placement in `PageDrawer`

Suspected gaps:

- Auto font-size calculation differs from Java.
- Multiline and single-line wrapping differ from Java.
- Baseline and leading calculation differ for generated field appearances.
- Widget rotation BBox/matrix handling is close but not complete.
- Default appearance resources and inherited field resources are not copied or
  resolved identically in all cases.

Acceptance criteria:

- The representative form fixtures leave the known-failure bucket.
- Generated appearance streams preserve Java-equivalent background, border,
  clipping, text placement, and resource behavior.

### 3. Page Geometry, Rotation, Overlay, And Clipping

Representative fixtures:

- `rot0.pdf`
- `rot90.pdf`
- `rot180.pdf`
- `rot270.pdf`
- `OverlayTestBaseRot0.pdf`
- `Overlayed-with-rot0.pdf`
- `Overlayed-with-rot90.pdf`
- `Overlayed-with-rot180.pdf`
- `Overlayed-with-rot270.pdf`
- `source.pdf`
- `PDFBOX-3110-poems-beads.pdf`
- `PDFBOX-3110-poems-beads-cropbox.pdf`

Likely source areas:

- `PDFStreamEngine`
- `PageDrawer`
- `Overlay`
- form XObject placement and clipping

Suspected gaps:

- Page and form initial matrix handling still differs in edge cases.
- Crop box and media box transforms are not fully Java-equivalent.
- Rotated page boundary strokes expose small but semantic edge differences.
- Overlay form placement and clipping differ from Java on rotated pages.

Acceptance criteria:

- Rotation and overlay fixtures leave the known-failure bucket.
- Page/form clipping and transformation probes match Java for the targeted
  fixtures.

### 4. Text, Glyph, And Font Render Fidelity

Representative fixtures:

- `AlignmentTests.pdf`
- `ControlCharacters.pdf`
- `PDFBOX-3044-010197-p5-ligatures.pdf`
- `PDFBOX-3656-SF1199AEG (Complete).pdf`
- `PDFBOX-4417-001031.pdf`
- `PDFBOX-4417-054080.pdf`
- `PDFBOX-5784.pdf`
- `PDFBOX-5792-240045.pdf`
- `arxiv-sample.pdf`
- `cweb.pdf`
- `unencrypted.pdf`

Likely source areas:

- `PageDrawer`
- `GlyphCache`
- `FontMappers`
- `PDType0Font`
- `PDTrueTypeFont`
- Standard 14 fallback handling

Suspected gaps:

- Some fallback text paths are used where Java renders font outlines.
- Glyph width, stretching, and fallback font selection differ from Java.
- Ligature and control-character handling creates visible glyph placement
  differences.
- SkiaSharp and Java2D antialiasing differences need semantic probes so true
  bugs are not hidden by backend raster drift.

Acceptance criteria:

- A glyph probe records Java/.NET glyph bounds, advance widths, and selected
  fallback font names for representative rows.
- Text-heavy fixtures leave the known-failure bucket or are reclassified into
  existing visual-equivalence categories only after semantic parity is proven.

### 5. Image, Color, Stencil, And Mask Rendering

Representative fixtures:

- `JBIG2Image.pdf`
- `png_demo.pdf`
- `testPDFPackage.pdf`
- `data-000001.pdf`
- `PDFBOX-5809-509329.pdf`
- `PDFBOX-5840-410609.pdf`
- `PDFBOX-6049-ExpectedResult.pdf`

Likely source areas:

- `SampledImageReader`
- `PDImageXObject`
- `PDColorSpace`
- image mask and soft mask handling in `PageDrawer`

Suspected gaps:

- High-bit-depth sampled image paths are incomplete.
- Decode arrays, indexed color, ICC/CMYK conversion, color-key masks, stencil
  masks, and interpolation/subsampling differ from Java.
- Java's image transfer-function and stencil-pattern paths are not fully
  mirrored in the .NET renderer.

Acceptance criteria:

- `SampledImageReader` handles Java-equivalent sampled image decoding beyond
  the current 1- to 8-bit path.
- Image-heavy fixtures leave the known-failure bucket without introducing load,
  text, save, or merge regressions.

### 6. Patterns, Shadings, Transparency Groups, And Form XObjects

Representative fixtures:

- `survey.pdf`
- `tiger-as-form-xobject.pdf`
- `custom-render-demo.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc01.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc02.pdf`
- `PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf`

Likely source areas:

- `PageDrawer`
- `TilingPaint`
- `TilingPaintFactory`
- `PDShading`
- `PDTransparencyGroup`
- `TransparencyGroup`

Suspected gaps:

- Shading BBox intersection and current clipping behavior differ from Java.
- Tiling pattern matrix handling is incomplete for some color spaces.
- True transparency group rasterization and compositing are still simplified.
- Blend-mode and soft-mask behavior is not fully Java-equivalent.

Acceptance criteria:

- Pattern, shading, transparency, and form XObject representative fixtures leave
  the known-failure bucket.
- Java-equivalent clipping, matrix, blend, and soft-mask behavior is covered by
  focused regression tests.

## Known Render Fixtures

Full known-row list from the local corpus run:

| Fixture | Corpus category |
|---|---|
| `AcroFormsBasicFields.pdf` | `forms` |
| `AcroFormsRotation.pdf` | `forms` |
| `Acroform-PDFBOX-2333.pdf` | `text extraction` |
| `AlignmentTests.pdf` | `uncategorized` |
| `ControlCharacters.pdf` | `uncategorized` |
| `JBIG2Image.pdf` | `uncategorized` |
| `MultilineFields.pdf` | `uncategorized` |
| `OverlayTestBaseRot0.pdf` | `rendering` |
| `Overlayed-with-rot0.pdf` | `rendering` |
| `Overlayed-with-rot180.pdf` | `rendering` |
| `Overlayed-with-rot270.pdf` | `rendering` |
| `Overlayed-with-rot90.pdf` | `rendering` |
| `PDFBOX-2725-878725.pdf` | `uncategorized` |
| `PDFBOX-3038-001033-p2.pdf` | `malformed PDFs` |
| `PDFBOX-3042-003177-p2.pdf` | `malformed PDFs` |
| `PDFBOX-3044-010197-p5-ligatures.pdf` | `fonts and subsetting` |
| `PDFBOX-3062-002207-p1.pdf` | `uncategorized` |
| `PDFBOX-3062-005717-p1.pdf` | `uncategorized` |
| `PDFBOX-3110-poems-beads-cropbox.pdf` | `uncategorized` |
| `PDFBOX-3110-poems-beads.pdf` | `uncategorized` |
| `PDFBOX-3656-SF1199AEG (Complete).pdf` | `uncategorized` |
| `PDFBOX-3835-input-acrobat-wrap.pdf` | `uncategorized` |
| `PDFBOX-4417-001031.pdf` | `uncategorized` |
| `PDFBOX-4417-054080.pdf` | `uncategorized` |
| `PDFBOX-5784.pdf` | `uncategorized` |
| `PDFBOX-5792-240045.pdf` | `uncategorized` |
| `PDFBOX-5809-509329.pdf` | `uncategorized` |
| `PDFBOX-5811-362972.pdf` | `uncategorized` |
| `PDFBOX-5840-410609.pdf` | `uncategorized` |
| `PDFBOX-6049-ExpectedResult.pdf` | `uncategorized` |
| `PDFBOX3812-acrobat-multiline-auto.pdf` | `uncategorized` |
| `PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf` | `merge/split/save` |
| `PDFBox.GlobalResourceMergeTest.Doc01.pdf` | `merge/split/save` |
| `PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf` | `merge/split/save` |
| `PDFBox.GlobalResourceMergeTest.Doc02.pdf` | `merge/split/save` |
| `acroform.pdf` | `forms` |
| `arxiv-sample.pdf` | `text extraction` |
| `custom-render-demo.pdf` | `uncategorized` |
| `cweb.pdf` | `uncategorized` |
| `data-000001.pdf` | `malformed PDFs` |
| `eu-001.pdf` | `forms` |
| `png_demo.pdf` | `uncategorized` |
| `rot0.pdf` | `uncategorized` |
| `rot180.pdf` | `uncategorized` |
| `rot270.pdf` | `uncategorized` |
| `rot90.pdf` | `uncategorized` |
| `source.pdf` | `merge/split/save` |
| `survey.pdf` | `rendering` |
| `testPDFPackage.pdf` | `uncategorized` |
| `tiger-as-form-xobject.pdf` | `rendering` |
| `unencrypted.pdf` | `encryption` |

## Sequencing

Recommended sequence:

1. Measurement, CI variance, and known-failure bucketing.
2. Form appearance rendering.
3. Page geometry, rotation, overlay, and clipping.
4. Text, glyph, and font render fidelity.
5. Image, color, stencil, and mask rendering.
6. Patterns, shadings, transparency groups, and form XObjects.

Each implementation PR should rerun the full runtime corpus, remove or narrow
known-failure entries, and lower the ratchet baseline when rows leave the known
bucket. The endpoint is strict runtime parity: zero known and zero unexpected
runtime divergences.
