# Render Equivalence Bucket Review

Issue: #541

Source: GitHub Actions runtime-parity artifact from PR #557 run 28330625128
Runtime parity output: `artifacts/runtime-parity-issue-541-download/runtime-parity-28330625128-1`
Runtime comparison generated UTC: `2026-06-28T18:00:01Z`
Manifest: `tools/parity/runtime/corpus-manifest.txt`

## Ratchet Lowering

| Category | Previous ceiling | Current count | New ceiling |
|---|---:|---:|---:|
| `render-visual-equivalence-match` | 68 | 62 | 62 |
| `render-foreground-shape-equivalence-match` | 48 | 47 | 47 |
| `render-image-mask-shape-equivalence-match` | 4 | 3 | 3 |
| `render-pattern-transparency-raster-equivalence-match` | 6 | 5 | 5 |
| `render-form-widget-raster-equivalence-match` | 5 | 3 | 3 |
| `render-glyph-layout-equivalence-match` | 10 | 7 | 7 |
| `render-sparse-equivalence-match` | 9 | 1 | 1 |
| `render-low-ink-equivalence-match` | 1 | 0 | 0 |
| `render-near-blank-threshold-equivalence-match` | 1 | 0 | 0 |
| `render-low-mean-raster-drift-equivalence-match` | 7 | 0 | 0 |

## Bucket Summary

| Category | Count | Owner / source area | Decision | Follow-up |
|---|---:|---|---|---|
| `render-visual-equivalence-match` | 62 | backend antialiasing / PageDrawer | Accepted backend raster adaptation; lower the ceiling to the current corpus count. | No implementation issue unless a fixture shows semantic drift beyond antialiasing. |
| `render-foreground-shape-equivalence-match` | 47 | PageDrawer geometry, clipping, and backend rasterization | Accepted shape-equivalence bucket with real geometry/clipping slices split out. | #562 |
| `render-image-mask-shape-equivalence-match` | 3 | sampled images, stencil masks, and image masks | Real renderer gap; reduce fixture by fixture. | #559 |
| `render-pattern-transparency-raster-equivalence-match` | 5 | patterns, transparency groups, and form XObjects | Real renderer gap; reduce fixture by fixture. | #560 |
| `render-form-widget-raster-equivalence-match` | 3 | forms and widget appearance rendering | Real form/widget renderer gap; reduce fixture by fixture. | #558 |
| `render-glyph-layout-equivalence-match` | 7 | fonts, glyph layout, and fallback rendering | Real glyph/font render fidelity gap; reduce fixture by fixture. | #561 |
| `render-sparse-equivalence-match` | 1 | backend antialiasing on sparse content | Accepted backend raster adaptation; lower the ceiling to the current corpus count. | No implementation issue unless the sparse fixture shows semantic drift. |
| `render-low-ink-equivalence-match` | 0 | backend antialiasing on near-blank content | No current rows; lower the ceiling to zero. | None. |
| `render-near-blank-threshold-equivalence-match` | 0 | backend near-blank threshold variance | No current rows; lower the ceiling to zero. | None. |
| `render-low-mean-raster-drift-equivalence-match` | 0 | backend antialiasing / color-management drift | No current rows; lower the ceiling to zero. | None. |
| `render-lossy-jpeg-decoder-equivalence-match` | 2 | JPEG decoder and color conversion | Accepted decoder adaptation; keep the current ceiling. | No implementation issue from #541. |
| `render-java-optional-jpx-reader-missing-match` | 3 | optional Java JPEG 2000 provider | Accepted optional-runtime difference for now. | #542 |

## Source Area Grouping

| Category | Source area | Rows |
|---|---|---:|
| `render-foreground-shape-equivalence-match` | backend antialiasing | 22 |
| `render-foreground-shape-equivalence-match` | geometry/clipping | 14 |
| `render-foreground-shape-equivalence-match` | fonts/glyphs | 4 |
| `render-foreground-shape-equivalence-match` | forms | 3 |
| `render-foreground-shape-equivalence-match` | PageDrawer patterns/transparency | 2 |
| `render-foreground-shape-equivalence-match` | sampled images/masks | 2 |
| `render-form-widget-raster-equivalence-match` | forms | 3 |
| `render-glyph-layout-equivalence-match` | fonts/glyphs | 7 |
| `render-image-mask-shape-equivalence-match` | sampled images/masks | 3 |
| `render-java-optional-jpx-reader-missing-match` | optional runtime provider | 3 |
| `render-lossy-jpeg-decoder-equivalence-match` | sampled images/masks | 2 |
| `render-pattern-transparency-raster-equivalence-match` | PageDrawer patterns/transparency | 5 |
| `render-sparse-equivalence-match` | backend antialiasing | 1 |
| `render-visual-equivalence-match` | backend antialiasing | 47 |
| `render-visual-equivalence-match` | forms | 9 |
| `render-visual-equivalence-match` | fonts/glyphs | 2 |
| `render-visual-equivalence-match` | geometry/clipping | 2 |
| `render-visual-equivalence-match` | sampled images/masks | 2 |

## Artifact Evidence

### `render-visual-equivalence-match`

Owner/root cause: Same dimensions and bounded pixel drift under the generic visual-equivalence thresholds.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `AcroFormForMerge-DifferentExportValues.pdf` | forms | 0.112559 | 2.50735 | 0.00357331 | 0.00126263 | `java/AcroFormForMerge-DifferentExportValues-java-p1.png` / `dotnet/AcroFormForMerge-DifferentExportValues-dotnet-p1.png` |
| `AcroFormForMerge-DifferentFieldType.pdf` | forms | 0.107911 | 2.4512 | 0.00344128 | 0.00120486 | `java/AcroFormForMerge-DifferentFieldType-java-p1.png` / `dotnet/AcroFormForMerge-DifferentFieldType-dotnet-p1.png` |
| `AcroFormForMerge-DifferentOptions.pdf` | forms | 0.112658 | 2.51354 | 0.00357331 | 0.00126263 | `java/AcroFormForMerge-DifferentOptions-java-p1.png` / `dotnet/AcroFormForMerge-DifferentOptions-dotnet-p1.png` |
| `AcroFormForMerge-SameNameNode.pdf` | forms | 0.0549242 | 1.65176 | 0.00184855 | 0.000618934 | `java/AcroFormForMerge-SameNameNode-java-p1.png` / `dotnet/AcroFormForMerge-SameNameNode-dotnet-p1.png` |
| `AcroFormForMerge.pdf` | forms | 0.113154 | 2.51956 | 0.00357331 | 0.00128738 | `java/AcroFormForMerge-java-p1.png` / `dotnet/AcroFormForMerge-dotnet-p1.png` |
| `AcrobatMerge-DifferentExportValues-WasMaster.pdf` | backend antialiasing | 0.112559 | 2.50735 | 0.00357331 | 0.00126263 | `java/AcrobatMerge-DifferentExportValues-WasMaster-java-p1.png` / `dotnet/AcrobatMerge-DifferentExportValues-WasMaster-dotnet-p1.png` |
| `AcrobatMerge-DifferentExportValues.pdf` | backend antialiasing | 0.113154 | 2.51956 | 0.00357331 | 0.00128738 | `java/AcrobatMerge-DifferentExportValues-java-p1.png` / `dotnet/AcrobatMerge-DifferentExportValues-dotnet-p1.png` |
| `AcrobatMerge-DifferentFieldType-WasMaster.pdf` | forms | 0.107911 | 2.4512 | 0.00344128 | 0.00120486 | `java/AcrobatMerge-DifferentFieldType-WasMaster-java-p1.png` / `dotnet/AcrobatMerge-DifferentFieldType-WasMaster-dotnet-p1.png` |
| ... | Additional rows omitted from Markdown; see JSON report. | | | | | |

### `render-foreground-shape-equivalence-match`

Owner/root cause: Foreground masks overlap after dilation, but raster/color drift is larger than the generic visual threshold.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `4PP-Highlighting.pdf` | backend antialiasing | 0.498184 | 4.42496 | 0.0245923 | 0.00283885 | `java/4PP-Highlighting-java-p1.png` / `dotnet/4PP-Highlighting-dotnet-p1.png` |
| `AcroFormsBasicFields.pdf` | forms | 8.97611 | 29.3355 | 0.185994 | 0.108082 | `java/AcroFormsBasicFields-java-p1.png` / `dotnet/AcroFormsBasicFields-dotnet-p1.png` |
| `AcroFormsRotation.pdf` | forms | 2.22613 | 10.617 | 0.0762692 | 0.0276787 | `java/AcroFormsRotation-java-p1.png` / `dotnet/AcroFormsRotation-dotnet-p1.png` |
| `AngledExample.pdf` | backend antialiasing | 0.242647 | 2.62319 | 0.0126583 | 0.00172359 | `java/AngledExample-java-p1.png` / `dotnet/AngledExample-dotnet-p1.png` |
| `AnnotationTypes.pdf` | backend antialiasing | 0.485321 | 5.16782 | 0.0198637 | 0.00339176 | `java/AnnotationTypes-java-p1.png` / `dotnet/AnnotationTypes-dotnet-p1.png` |
| `OverlayTestBaseRot0.pdf` | geometry/clipping | 7.04244 | 19.0844 | 0.232373 | 0.0839688 | `java/OverlayTestBaseRot0-java-p1.png` / `dotnet/OverlayTestBaseRot0-dotnet-p1.png` |
| `Overlayed-with-rot0.pdf` | geometry/clipping | 7.81416 | 21.41 | 0.242251 | 0.0932033 | `java/Overlayed-with-rot0-java-p1.png` / `dotnet/Overlayed-with-rot0-dotnet-p1.png` |
| `Overlayed-with-rot180.pdf` | geometry/clipping | 7.81976 | 21.4171 | 0.242399 | 0.0932363 | `java/Overlayed-with-rot180-java-p1.png` / `dotnet/Overlayed-with-rot180-dotnet-p1.png` |
| ... | Additional rows omitted from Markdown; see JSON report. | | | | | |

### `render-image-mask-shape-equivalence-match`

Owner/root cause: Fixture-scoped image/mask rows preserve foreground shape while sampled-image raster details differ.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `JBIG2Image.pdf` | sampled images/masks | 8.58055 | 16.658 | 0.39151 | 0.0624294 | `java/JBIG2Image-java-p1.png` / `dotnet/JBIG2Image-dotnet-p1.png` |
| `PDFBOX-5840-410609.pdf` | sampled images/masks | 4.64631 | 16.3708 | 0.157927 | 0.0498614 | `java/PDFBOX-5840-410609-java-p1.png` / `dotnet/PDFBOX-5840-410609-dotnet-p1.png` |
| `data-000001.pdf` | sampled images/masks | 1.95102 | 13.4229 | 0.0427395 | 0.02347 | `java/data-000001-java-p1.png` / `dotnet/data-000001-dotnet-p1.png` |

### `render-pattern-transparency-raster-equivalence-match`

Owner/root cause: Fixture-scoped pattern/transparency rows remain visually bounded but not pixel-identical.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf` | PageDrawer patterns/transparency | 4.38203 | 13.1448 | 0.202084 | 0.0364117 | `java/PDFBox.GlobalResourceMergeTest.Doc01.decoded-java-p1.png` / `dotnet/PDFBox.GlobalResourceMergeTest.Doc01.decoded-dotnet-p1.png` |
| `PDFBox.GlobalResourceMergeTest.Doc01.pdf` | PageDrawer patterns/transparency | 4.38203 | 13.1448 | 0.202084 | 0.0364117 | `java/PDFBox.GlobalResourceMergeTest.Doc01-java-p1.png` / `dotnet/PDFBox.GlobalResourceMergeTest.Doc01-dotnet-p1.png` |
| `PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf` | PageDrawer patterns/transparency | 4.38203 | 13.1448 | 0.202084 | 0.0364117 | `java/PDFBox.GlobalResourceMergeTest.Doc02.decoded-java-p1.png` / `dotnet/PDFBox.GlobalResourceMergeTest.Doc02.decoded-dotnet-p1.png` |
| `PDFBox.GlobalResourceMergeTest.Doc02.pdf` | PageDrawer patterns/transparency | 4.38203 | 13.1448 | 0.202084 | 0.0364117 | `java/PDFBox.GlobalResourceMergeTest.Doc02-java-p1.png` / `dotnet/PDFBox.GlobalResourceMergeTest.Doc02-dotnet-p1.png` |
| `survey.pdf` | PageDrawer patterns/transparency | 1.71355 | 11.3535 | 0.0638211 | 0.0105489 | `java/survey-java-p1.png` / `dotnet/survey-dotnet-p1.png` |

### `render-form-widget-raster-equivalence-match`

Owner/root cause: Widget appearance rows preserve semantics while text placement/raster details differ.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `Acroform-PDFBOX-2333.pdf` | forms | 3.72526 | 15.9572 | 0.111515 | 0.0441672 | `java/Acroform-PDFBOX-2333-java-p1.png` / `dotnet/Acroform-PDFBOX-2333-dotnet-p1.png` |
| `MultilineFields.pdf` | forms | 2.80943 | 13.5436 | 0.0869892 | 0.0316069 | `java/MultilineFields-java-p1.png` / `dotnet/MultilineFields-dotnet-p1.png` |
| `PDFBOX3812-acrobat-multiline-auto.pdf` | forms | 0.984186 | 8.36544 | 0.0285535 | 0.0114709 | `java/PDFBOX3812-acrobat-multiline-auto-java-p1.png` / `dotnet/PDFBOX3812-acrobat-multiline-auto-dotnet-p1.png` |

### `render-glyph-layout-equivalence-match`

Owner/root cause: Glyph probe rows match identity with bounded glyph geometry, but rendered pixels still differ.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `AlignmentTests.pdf` | fonts/glyphs | 3.47581 | 15.9382 | 0.0952581 | 0.0441589 | `java/AlignmentTests-java-p1.png` / `dotnet/AlignmentTests-dotnet-p1.png` |
| `ControlCharacters.pdf` | fonts/glyphs | 3.61777 | 14.6935 | 0.107364 | 0.0458507 | `java/ControlCharacters-java-p1.png` / `dotnet/ControlCharacters-dotnet-p1.png` |
| `PDFBOX-3038-001033-p2.pdf` | fonts/glyphs | 2.23849 | 9.24207 | 0.0946392 | 0.0190549 | `java/PDFBOX-3038-001033-p2-java-p1.png` / `dotnet/PDFBOX-3038-001033-p2-dotnet-p1.png` |
| `PDFBOX-3062-002207-p1.pdf` | fonts/glyphs | 3.84713 | 16.1335 | 0.103692 | 0.0530551 | `java/PDFBOX-3062-002207-p1-java-p1.png` / `dotnet/PDFBOX-3062-002207-p1-dotnet-p1.png` |
| `PDFBOX-3656-SF1199AEG (Complete).pdf` | fonts/glyphs | 5.69982 | 15.8901 | 0.205924 | 0.0704182 | `java/PDFBOX-3656-SF1199AEG (Complete)-java-p1.png` / `dotnet/PDFBOX-3656-SF1199AEG (Complete)-dotnet-p1.png` |
| `PDFBOX-5784.pdf` | fonts/glyphs | 4.41855 | 18.1831 | 0.110229 | 0.0602213 | `java/PDFBOX-5784-java-p1.png` / `dotnet/PDFBOX-5784-dotnet-p1.png` |
| `arxiv-sample.pdf` | fonts/glyphs | 8.93562 | 33.7646 | 0.157936 | 0.0928319 | `java/arxiv-sample-java-p1.png` / `dotnet/arxiv-sample-dotnet-p1.png` |

### `render-sparse-equivalence-match`

Owner/root cause: Sparse non-near-blank pages amplify tiny raster differences.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `document.pdf` | backend antialiasing | 0.562364 | 5.08959 | 0.0183306 | 0.00754977 | `java/document-java-p1.png` / `dotnet/document-dotnet-p1.png` |

### `render-low-ink-equivalence-match`

Owner/root cause: Near-blank pages are sensitive to tiny antialiasing differences.

No current rows.

### `render-near-blank-threshold-equivalence-match`

Owner/root cause: One runtime may cross the near-blank metric boundary while the raster drift remains sparse and bounded.

No current rows.

### `render-low-mean-raster-drift-equivalence-match`

Owner/root cause: Low average channel error with bounded large/moderate pixel differences.

No current rows.

### `render-lossy-jpeg-decoder-equivalence-match`

Owner/root cause: JPEG decoding is lossy and backend-dependent while remaining visually bounded.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `jpeg_demo.pdf` | sampled images/masks | 0.774677 | 3.91313 | 0.0137354 | 0.00305231 | `java/jpeg_demo-java-p1.png` / `dotnet/jpeg_demo-dotnet-p1.png` |
| `jpegrgb.pdf` | sampled images/masks | 1.33447 | 5.58995 | 0.0648396 | 0.00356506 | `java/jpegrgb-java-p1.png` / `dotnet/jpegrgb-dotnet-p1.png` |

### `render-java-optional-jpx-reader-missing-match`

Owner/root cause: Java renders blank because its optional JPX reader is unavailable while .NET renders visible pixels.

| File | Source area | Mean | RMS | Moderate | Large | Artifacts |
|---|---|---:|---:|---:|---:|---|
| `JPXTestCMYK.pdf` | optional runtime provider | 8.99741 | 45.4149 | 0.062522 | 0.0594757 | `java/JPXTestCMYK-java-p1.png` / `dotnet/JPXTestCMYK-dotnet-p1.png` |
| `JPXTestGrey.pdf` | optional runtime provider | 3.55824 | 21.2928 | 0.0541767 | 0.0367003 | `java/JPXTestGrey-java-p1.png` / `dotnet/JPXTestGrey-dotnet-p1.png` |
| `JPXTestRGB.pdf` | optional runtime provider | 3.06101 | 20.5708 | 0.0549784 | 0.0372214 | `java/JPXTestRGB-java-p1.png` / `dotnet/JPXTestRGB-dotnet-p1.png` |

