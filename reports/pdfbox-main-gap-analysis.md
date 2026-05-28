# PDFBox Upstream Java Gap Analysis (All Modules)

Datetime (UTC): 2026-05-28T20:52:52.739Z
Reference upstream Java repository: Apache PDFBox trunk
Tracked parity baseline commit: `a71c5679d69bc3fd3ab15e248b69441ee91dca6c`
Latest upstream head scanned: `6196c451156dcce18d6c69c4deaa0935854d9a1a`

## Scope and method

- Scanned **all current upstream Java files** under `**/src/main/java/**/*.java` across the entire upstream repository tree.
- Total upstream files in scope: **1067**.
- Counted a Java file as **ported/mapped** when this repo contains either:
  - a `PDFBOX_SOURCE_PATH` provenance match in `src/**/*.cs`, or
  - a matching upstream→C# row in `reports/traceability-parity-report.json`.
- This is a direct source-coverage inventory; it does not imply full behavioral parity.

## Summary

| Upstream module | Java files | Mapped C# ports | Missing | % Done |
|---|---:|---:|---:|---:|
| `pdfbox` | 618 | 527 | 91 | 85.3% |
| `fontbox` | 143 | 143 | 0 | 100.0% |
| `xmpbox` | 74 | 4 | 70 | 5.4% |
| `io` | 18 | 18 | 0 | 100.0% |
| `tools` | 26 | 0 | 26 | 0.0% |
| `examples` | 94 | 0 | 94 | 0.0% |
| `debugger` | 91 | 0 | 91 | 0.0% |
| `benchmark` | 3 | 0 | 3 | 0.0% |
| **TOTAL** | **1067** | **692** | **375** | **64.9%** |

Library-core subset (`pdfbox` + `fontbox` + `xmpbox` + `io`) coverage: **692 / 853 = 81.1%**.

---

## Package-family breakdown (largest code-bearing modules)

### `pdfbox`

| Package family | Java files | Mapped | Missing | % Done |
|---|---:|---:|---:|---:|
| `pdmodel` | 428 | 359 | 69 | 83.9% |
| `contentstream` | 79 | 68 | 11 | 86.1% |
| `cos` | 24 | 24 | 0 | 100.0% |
| `filter` | 23 | 22 | 1 | 95.7% |
| `pdfparser` | 18 | 11 | 7 | 61.1% |
| `util` | 12 | 12 | 0 | 100.0% |
| `rendering` | 10 | 10 | 0 | 100.0% |
| `pdfwriter` | 7 | 4 | 3 | 57.1% |
| `multipdf` | 6 | 6 | 0 | 100.0% |
| `text` | 6 | 6 | 0 | 100.0% |
| `printing` | 4 | 4 | 0 | 100.0% |
| `(root)` | 1 | 1 | 0 | 100.0% |

### `fontbox`

| Package family | Java files | Mapped | Missing | % Done |
|---|---:|---:|---:|---:|
| `ttf` | 83 | 83 | 0 | 100.0% |
| `cff` | 26 | 26 | 0 | 100.0% |
| `afm` | 8 | 8 | 0 | 100.0% |
| `util` | 8 | 8 | 0 | 100.0% |
| `type1` | 6 | 6 | 0 | 100.0% |
| `cmap` | 5 | 5 | 0 | 100.0% |
| `encoding` | 4 | 4 | 0 | 100.0% |
| `(root)` | 2 | 2 | 0 | 100.0% |
| `pfb` | 1 | 1 | 0 | 100.0% |

### `xmpbox`

| Package family | Java files | Mapped | Missing | % Done |
|---|---:|---:|---:|---:|
| `type` | 50 | 0 | 50 | 0.0% |
| `schema` | 15 | 1 | 14 | 6.7% |
| `xml` | 6 | 2 | 4 | 33.3% |
| `(root)` | 3 | 1 | 2 | 33.3% |

---

## Largest remaining gaps

### `examples` — 0 / 94 mapped (94 missing)

- `examples/src/main/java/org/apache/pdfbox/examples/ant/PDFToTextTask.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/AddBorderToField.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateCheckBox.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateMultiWidgetsForm.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreatePushButton.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateRadioButtons.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateSimpleForm.java`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateSimpleFormWithEmbeddedFont.java`
- ... (86 more missing files in this module)

### `debugger` — 0 / 91 mapped (91 missing)

- `debugger/src/main/java/org/apache/pdfbox/debugger/PDFDebugger.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSArrayBased.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSDeviceN.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSIndexed.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSSeparation.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/ColorBarCellRenderer.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/DeviceNColorant.java`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/DeviceNTableModel.java`
- ... (83 more missing files in this module)

### `pdfbox` — 527 / 618 mapped (91 missing)

- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDContentStream.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/MissingOperandException.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetColor.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CloseFillEvenOddAndStrokePath.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CloseFillNonZeroAndStrokePath.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/DrawObject.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/GraphicsOperatorProcessor.java`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/LegacyFillNonZeroRule.java`
- ... (83 more missing files in this module)

### `xmpbox` — 4 / 74 mapped (70 missing)

- `xmpbox/src/main/java/org/apache/xmpbox/DateConverter.java`
- `xmpbox/src/main/java/org/apache/xmpbox/XMPMetadata.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/AdobePDFSchema.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/DublinCoreSchema.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/ExifSchema.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/PDFAExtensionSchema.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/PDFAIdentificationSchema.java`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/PhotoshopSchema.java`
- ... (62 more missing files in this module)

### `tools` — 0 / 26 mapped (26 missing)

- `tools/src/main/java/org/apache/pdfbox/tools/DecompressObjectstreams.java`
- `tools/src/main/java/org/apache/pdfbox/tools/Decrypt.java`
- `tools/src/main/java/org/apache/pdfbox/tools/Encrypt.java`
- `tools/src/main/java/org/apache/pdfbox/tools/ExportFDF.java`
- `tools/src/main/java/org/apache/pdfbox/tools/ExportXFDF.java`
- `tools/src/main/java/org/apache/pdfbox/tools/ExtractImages.java`
- `tools/src/main/java/org/apache/pdfbox/tools/ExtractText.java`
- `tools/src/main/java/org/apache/pdfbox/tools/ExtractXMP.java`
- ... (18 more missing files in this module)

### `benchmark` — 0 / 3 mapped (3 missing)

- `benchmark/src/main/java/org/apache/pdfbox/benchmark/LoadAndSave.java`
- `benchmark/src/main/java/org/apache/pdfbox/benchmark/Rendering.java`
- `benchmark/src/main/java/org/apache/pdfbox/benchmark/TextExtraction.java`

---

## Traceability status for mapped upstream source rows

Among **458** `traceability-parity-report.json` rows with upstream `source_path` in this all-module scope:

- `in-sync`: **427**
- `partially-in-sync`: **20**
- `partial`: **11**

## Bottom line

- Full upstream Java scope currently maps **692 / 1067 (64.9%)** files.
- Main library coverage is strongest in `fontbox` and `io` (fully mapped by source-path criteria), with `pdfbox` at high but incomplete coverage and `xmpbox` largely unported.
- Non-library upstream modules (`examples`, `debugger`, `tools`, `benchmark`) remain mostly or entirely unported and account for a large share of the remaining file-volume gap.
