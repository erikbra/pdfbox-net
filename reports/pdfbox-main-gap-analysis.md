# PDFBox Main Module Gap Analysis

Date: 2026-05-26
Reference upstream Java repository: Apache PDFBox trunk
Tracked parity baseline commit: `ccd281cfecedcc0ad39709bece5e67b19a54e8db`
Latest upstream head scanned: `a71c5679d69bc3fd3ab15e248b69441ee91dca6c`

## Scope and method

- Scanned **all current upstream Java files** under `pdfbox/src/main/java/org/apache/pdfbox/**/*.java`.
- The tracked parity baseline and the latest scanned upstream head both currently contain **618** files in this scope.
- Counted a Java file as **ported/mapped** when the repo contains either:
  - a `PDFBOX_SOURCE_PATH` provenance match in `src/**/*.cs`, or
  - a matching upstream→C# row in `reports/traceability-parity-report.json`.
- Excluded tests and non-main modules such as `fontbox`, `xmpbox`, `debugger`, `examples`, and `benchmark`.
- This is therefore a **direct source-coverage** report, not a claim of full functional parity.

## Summary

| Package family | Java files | Mapped C# ports | Missing | % Done |
|---|---:|---:|---:|---:|
| `org.apache.pdfbox` (root) | 1 | 1 | 0 | 100.0% |
| `org.apache.pdfbox.contentstream` | 3 | 2 | 1 | 66.7% |
| `org.apache.pdfbox.contentstream.operator` | 76 | 63 | 13 | 82.9% |
| `org.apache.pdfbox.cos` | 24 | 24 | 0 | 100.0% |
| `org.apache.pdfbox.filter` | 23 | 15 | 8 | 65.2% |
| `org.apache.pdfbox.multipdf` | 6 | 0 | 6 | 0.0% |
| `org.apache.pdfbox.pdfparser` (+ `xref`) | 18 | 11 | 7 | 61.1% |
| `org.apache.pdfbox.pdfwriter` (+ `compress`) | 7 | 4 | 3 | 57.1% |
| `org.apache.pdfbox.pdmodel` (root) | 25 | 8 | 17 | 32.0% |
| `org.apache.pdfbox.pdmodel.common` | 37 | 37 | 0 | 100.0% |
| `org.apache.pdfbox.pdmodel.documentinterchange` | 24 | 23 | 1 | 95.8% |
| `org.apache.pdfbox.pdmodel.encryption` | 19 | 11 | 8 | 57.9% |
| `org.apache.pdfbox.pdmodel.fdf` | 30 | 0 | 30 | 0.0% |
| `org.apache.pdfbox.pdmodel.fixup` | 8 | 0 | 8 | 0.0% |
| `org.apache.pdfbox.pdmodel.font` | 51 | 24 | 27 | 47.1% |
| `org.apache.pdfbox.pdmodel.graphics` | 90 | 39 | 51 | 43.3% |
| `org.apache.pdfbox.pdmodel.interactive` | 144 | 43 | 101 | 29.9% |
| `org.apache.pdfbox.printing` | 4 | 4 | 0 | 100.0% |
| `org.apache.pdfbox.rendering` | 10 | 10 | 0 | 100.0% |
| `org.apache.pdfbox.text` | 6 | 6 | 0 | 100.0% |
| `org.apache.pdfbox.util` (+ `filetypedetector`) | 12 | 12 | 0 | 100.0% |
| **TOTAL** | **618** | **337** | **281** | **54.5%** |

### What changed versus the previous report

- The report now uses an **exact full-file upstream scan** instead of package-size estimates.
- Counts now cover the **entire pdfbox main module**, including previously omitted families such as:
  - `multipdf`
  - `pdmodel.fdf`
  - `pdmodel.fixup`
  - `util.filetypedetector`
  - the deeper `pdmodel.interactive` and `pdmodel.graphics` subpackages
- As a result, the previous `~89%` figure was materially overstated for current upstream scope.
  The direct main-module coverage is currently **337 / 618 = 54.5%**.

---

## Areas with full or near-full direct source coverage

### Fully mapped (100%)

- `org.apache.pdfbox` (root package) — **1 / 1**
- `org.apache.pdfbox.cos` — **24 / 24**
- `org.apache.pdfbox.printing` — **4 / 4**
- `org.apache.pdfbox.rendering` — **10 / 10**
- `org.apache.pdfbox.text` — **6 / 6**
- `org.apache.pdfbox.util` (+ `filetypedetector`) — **12 / 12**
- `org.apache.pdfbox.pdfparser.xref` — **6 / 6**
- `org.apache.pdfbox.pdfwriter` (root package only) — **3 / 3**
- `org.apache.pdfbox.pdmodel.common` — **37 / 37**
- `org.apache.pdfbox.pdmodel.common.filespecification` — **4 / 4**
- `org.apache.pdfbox.pdmodel.common.function` — **6 / 6**
- `org.apache.pdfbox.pdmodel.common.function.type4` — **11 / 11**
- `org.apache.pdfbox.pdmodel.documentinterchange.logicalstructure` — **12 / 12**
- `org.apache.pdfbox.pdmodel.documentinterchange.markedcontent` — **2 / 2**
- `org.apache.pdfbox.pdmodel.documentinterchange.prepress` — **1 / 1**
- `org.apache.pdfbox.contentstream.operator.text` — **16 / 16**

### High coverage but not yet closed

- `org.apache.pdfbox.contentstream.operator` overall — **63 / 76**
  - Strongest subareas: text **16 / 16**, color **12 / 13**, marked-content **5 / 6**
  - Remaining gaps are concentrated in graphics and state operators
- `org.apache.pdfbox.pdmodel.documentinterchange` overall — **23 / 24**
  - Remaining tagged-PDF file: `PDStandardAttributeObject`

---

## Largest remaining gaps

### `org.apache.pdfbox.pdmodel.interactive` — 43 / 144 mapped (101 missing)

This is now the single largest unported family in the main module.

Major zero-coverage subareas:
- `interactive.annotation.handlers` — **0 / 20**
- `interactive.digitalsignature` — **0 / 12**
- `interactive.digitalsignature.visible` — **0 / 6**
- `interactive.measurement` — **0 / 4**
- `interactive.pagenavigation` — **0 / 7**
- `interactive.viewerpreferences` — **0 / 1**
- `interactive` root package — **0 / 4**

Still-thin subareas:
- `interactive.action` — **9 / 25**
- `interactive.annotation` — **18 / 32**
- `interactive.form` — **4 / 21**

### `org.apache.pdfbox.pdmodel.graphics` — 39 / 90 mapped (51 missing)

Main remaining gaps:
- `graphics.shading` — **10 / 37**
- `graphics.color` — **13 / 23**
- `graphics.image` — **2 / 9**
- `graphics.blend` — **0 / 2**
- `graphics` root package — **2 / 4**
- `graphics.state` — **4 / 6**
- `graphics.form` — **2 / 3**

This means the recent shading/core-type work improved the package, but the full graphics stack is still far from source-complete because most shading helper/rendering classes and several image/color types remain missing.

### `org.apache.pdfbox.pdmodel.font` — 24 / 51 mapped (27 missing)

Breakdown:
- `font` root package — **16 / 39**
- `font.encoding` — **8 / 12**

Still-missing examples include `CIDFontMapping`, `CMapManager`, `FontCache`, `FontMapperImpl`, `FontProvider`, `PDCIDFontType0`, `PDType3Font`, `ToUnicodeWriter`, and multiple encoding classes.

### `org.apache.pdfbox.pdmodel.fdf` — 0 / 30 mapped

No direct FDF package coverage is currently present.

### `org.apache.pdfbox.pdmodel.fixup` — 0 / 8 mapped

Both the root fixup package and `fixup.processor` remain unported.

---

## Other notable package gaps

### `org.apache.pdfbox.pdfparser` (+ `xref`) — 11 / 18 mapped

Mapped coverage is solid for xref-specific classes, but the root parser package still misses key files:
- `BaseParser`
- `BruteForceParser`
- `EndstreamFilterStream`
- `FDFParser`
- `PDFXRefStream`
- `PDFXrefStreamParser`
- `XrefParser`

### `org.apache.pdfbox.filter` — 15 / 23 mapped

Still missing:
- `ASCII85InputStream`
- `ASCII85OutputStream`
- `CCITTFaxDecoderStream`
- `CCITTFaxEncoderStream`
- `Filter`
- `FlateFilterDecoderStream`
- `MissingImageReaderException`
- `TIFFExtension`

### `org.apache.pdfbox.pdmodel.encryption` — 11 / 19 mapped

Still missing:
- `InvalidPasswordException`
- `PublicKeyDecryptionMaterial`
- `PublicKeyProtectionPolicy`
- `PublicKeyRecipient`
- `PublicKeySecurityHandler`
- `SaslPrep`
- `SecurityHandlerFactory`
- `SecurityProvider`

### `org.apache.pdfbox.multipdf` — 0 / 6 mapped

Still entirely unported:
- `LayerUtility`
- `Overlay`
- `PDFCloneUtility`
- `PDFMergerUtility`
- `PageExtractor`
- `Splitter`

### `org.apache.pdfbox.util.filetypedetector` — 3 / 3 mapped

Mapped:
- `ByteTrie`
- `FileType`
- `FileTypeDetector`

### Root/main-package coverage

- `org.apache.pdfbox.Loader` now has first-pass PDF entry-point coverage (`partial` parity; FDF/XFDF and public-key overloads remain deferred).

---

## Traceability quality inside the mapped set

Among the **238** current traceability rows that already point into the pdfbox main module:

- `in-sync`: **206**
- `partially-in-sync`: **25**
- `partial`: **7**
- blank / unclassified: **0**

### Highest-value traceability cleanup

The previously unclassified operator rows in `contentstream.operator.color`,
`contentstream.operator.graphics`, and `contentstream.operator.state` are now explicitly classified,
so the touched operator families no longer carry blank-status traceability rows.

### Known partial / partially-in-sync hotspots

- `COSObject`
- `PDDocument`, `PDDocumentCatalog`, `PDDocumentInformation`, `PDPage`, `PDPageTree`
- `PDRectangle`
- `StandardSecurityHandler`
- `PDAcroForm`, `PDField`, `PDUnknownField`, `PDCheckBox`, `PDTextField`
- `PDShading` and the current `PDShadingType*` hierarchy
- Several image/filter implementations (`CCITTFaxFilter`, `CryptFilter`, `DCTFilter`, `JBIG2Filter`, `JPXFilter`)

So even within the 327 mapped upstream files, a meaningful subset is still only partial parity rather than complete parity.

---

## Legacy mapping cleanup

The previously called-out stale path-drift examples have been normalized or removed from the auditable
records. This cleanup covered:
- renamed state operators (`SetLineCapStyle`, `SetLineJoinStyle`, `SetLineMiterLimit`)
- inline-image mapping normalization onto the current upstream `BeginInlineImage`
- parser/font/tagged-PDF path drift (`PDFParser`, `FontMapperImpl`, and tagged-PDF attribute objects)
- removal of obsolete compatibility-section alias rows from the traceability records

---

## Recommended priority order from the refreshed inventory

1. Close traceability hygiene for already-mapped operator files (cheap accuracy win).
2. Finish the remaining `pdmodel.font` milestone work.
3. Continue `pdmodel.graphics`, especially shading helpers, image types, and remaining color abstractions.
4. Tackle `pdmodel.interactive` in dependency-ordered slices, starting with forms/action coverage before appearance handlers and signatures.
5. Add first-pass coverage for untouched families: `multipdf`, `pdmodel.fdf`, and `pdmodel.fixup`.

## Bottom line

The pdfbox main module is **much broader than the older estimate-based report reflected**. The port is strong in
`cos`, `pdmodel.common`, `contentstream.operator`, `rendering`, `text`, `printing`, and utility layers (including
`filetypedetector`), but the full upstream main-module inventory is still only **54.5% directly mapped today
(337 / 618 files)**.

The biggest remaining source-volume is concentrated in:
- `pdmodel.interactive`
- `pdmodel.graphics`
- `pdmodel.font`
- `pdmodel.fdf`
- `pdmodel.fixup`
- `multipdf`
