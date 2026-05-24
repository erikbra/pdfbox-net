# PDFBox Main Module Gap Analysis

Date: 2026-05-24 (updated)
Previous date: 2026-05-24 (first edition was written before the most recent PRs landed)
Reference upstream Java repository: Apache PDFBox trunk
Reference commit: ccd281cfecedcc0ad39709bece5e67b19a54e8db

## Summary

| Package | Java files (est.) | C# ported | Missing (est.) | % Done |
|---|---|---|---|---|
| `org.apache.pdfbox.cos` | ~26 | 25 | ~1 | ~96% ✅ |
| `org.apache.pdfbox.contentstream` (engine) | ~5 | 3 | ~2 | ~60% ⚠️ |
| `org.apache.pdfbox.contentstream.operator` | ~60 | 73† | ~2 | ~97% ✅ |
| `org.apache.pdfbox.filter` | ~18 | 17 | ~1 | ~94% ✅ |
| `org.apache.pdfbox.pdfparser` | ~8 | 9† | ~3 | ~70% ⚠️ |
| `org.apache.pdfbox.pdfwriter` | ~5 | 4 | ~1 | ~80% ⚠️ |
| `org.apache.pdfbox.pdmodel` (root) | ~10 | 9 | ~1 | ~90% ✅ |
| `org.apache.pdfbox.pdmodel.common` | ~15 | 3 | ~12 | ~20% ❌ |
| `org.apache.pdfbox.pdmodel.encryption` | ~12 | 11 | ~3 | ~75% ⚠️ |
| `org.apache.pdfbox.pdmodel.font` | ~30 | 18 | ~12 | ~60% ⚠️ |
| `org.apache.pdfbox.pdmodel.graphics` | ~40 | 24 | ~16 | ~60% ⚠️ |
| `org.apache.pdfbox.pdmodel.interactive` | ~30 | 2 | ~28 | ~7% ❌ |
| `org.apache.pdfbox.pdmodel.documentinterchange` | ~10 | 1 | ~9 | ~10% ⚠️ |
| `org.apache.pdfbox.rendering` | ~12 | 11 | ~4 | ~75% ⚠️ |
| `org.apache.pdfbox.text` | ~6 | 6 | 0 | ~100% ✅* |
| `org.apache.pdfbox.util` | ~15 | 10 | ~5 | ~67% ⚠️ |
| `org.apache.pdfbox.printing` | ~4 | 4 | 0 | ~100% ✅ |
| **TOTAL** | **~320** | **~230** | **~90** | **~70%** |

† The C# operator count (73) is higher than the Java ~60 because some Java files each handle
a single operator while the C# port includes the Operator/OperatorName/OperatorProcessor base
infrastructure separately. The PDFParser C# files also include xref-type files that don't have
direct Java counterparts.

\* text files are ported but some depend on incomplete pdmodel font and rendering layers

---

## Coverage refresh from traceability artifacts (2026-05-24)

This snapshot was recalculated from current report data in:
- `reports/conversion-records.json`
- `reports/normalization-records.json`
- `reports/traceability-parity-report.json`

### Conversion inventory

- Conversion rows: **286** (`276` unique source->target pairs)
- Port modes:
  - `mechanical`: **143**
  - `adapted`: **109**
  - `native-test`: **28**
  - `partial`: **4**
  - `adapted-minimal`: **2**
- Upstream test mappings converted: **25**

### Traceability status coverage

- Traceability rows: **289**
- `in-sync`: **216** (~74.7%)
- `partially-in-sync`: **18** (~6.2%)
- `partial`: **11** (~3.8%)
- blank status (needs classification/backfill): **44** (~15.2%)

### Immediate coverage-report follow-up

- The highest-leverage report cleanup is to classify the **44** blank-status
  traceability rows (mostly `contentstream/operator/*` mappings) as explicit
  `in-sync`/`partial`/`deferred` statuses.

## Fully or near-fully ported ✅

### `org.apache.pdfbox.cos` — ~96%

**Ported (25):** All primitives: `COSArray`, `COSBase`, `COSBoolean`, `COSDictionary`,
`COSDocument`, `COSDocumentState`, `COSFloat`, `COSIncrement`, `COSInputStream`, `COSInteger`,
`COSName`, `COSNull`, `COSNumber`, `COSObject`, `COSObjectKey`, `COSObjectable`,
`COSOutputStream`, `COSStream`, `COSString`, `COSUpdateInfo`, `COSUpdateState`,
`ICOSParser`, `ICOSVisitor`, `PDFDocEncoding`, `UnmodifiableCOSDictionary`.

**Potentially missing (1–2):** Minor utilities (`COSHexString`, `COSObjectReference`) that
may exist in some Java versions. Functionally the COS layer is complete.

### `org.apache.pdfbox.contentstream.operator` — ~97%

**Ported (73 C# files covering all major operator categories):**
- **Color (12):** sc, scn, cs, k, g, rg, SC, SCN, CS, K, G, RG
- **State (12):** cm, Q, q, gs, Tm, i, J, d, j, M, w, ri
- **Path construction (8):** re, h, c, y, v, l, m, n
- **Path painting (10):** S, s, f, F, f\*, B, B\*, W, W\*
- **Marked content (5):** BDC, BMC, EMC, DP, MP
- **Text (16):** BT, ET, Td, TD, T\*, Tc, Tf, Tz, TL, Tr, Ts, Tw, Tj, TJ, ', "
- **Inline image (3):** BI, ID, EI
- **Type3 font (2):** d0, d1
- **Compatibility (2):** BX, EX
- **Other (1):** Do

**Missing (2):**
- `graphics/CloseAndFillNonZeroAndStrokePath.java` — operator `b` (close path + fill non-zero + stroke)
- `graphics/CloseAndFillEvenOddAndStrokePath.java` — operator `b*` (close path + fill even-odd + stroke)
  - Both operator names are defined in `OperatorName.cs` but no processor class or registration exists yet.

### `org.apache.pdfbox.filter` — ~94%

**Ported (17):** `Filter.cs` (abstract base), `DecodeOptions.cs`, `DecodeResult.cs`,
`FlateFilter.cs` (deflate/zlib), `ASCIIHexFilter.cs`, `ASCII85Filter.cs`, `DCTFilter.cs`,
`LZWFilter.cs`, `RunLengthDecodeFilter.cs`, `CCITTFaxDecodeFilter.cs`, `JBIG2Filter.cs`,
`JPXFilter.cs`, `CryptFilter.cs`, `FilterFactory.cs`, `FilterMaker.cs`, `IdentityFilter.cs`,
`Predictor.cs`.

**Possibly missing (1):** `FlateFilter` inner helpers / PNG predictor completeness.
All filter types needed for real-world PDFs are present.

### `org.apache.pdfbox.text` — ~100%

All 6 files mechanically ported. Functional completeness depends on completing the font
and rendering layers:
- `PDFTextStripper.cs`, `PDFTextStripperByArea.cs`, `PDFMarkedContentExtractor.cs`
- `LegacyPDFStreamEngine.cs`, `TextPosition.cs`, `TextPositionComparator.cs`

### `org.apache.pdfbox.printing` — 100%

All 4 files ported:
- `Orientation.cs`, `PDFPageable.cs`, `PDFPrintable.cs`, `Scaling.cs`
(Functional rendering depends on the rendering layer receiving real .NET graphics support.)

---

## Substantially complete — minor gaps remain ⚠️

### `org.apache.pdfbox.pdfparser` — ~90%

**Ported (11 C# files):** `COSParser.cs`, `PDFDocumentParser.cs`, `PDFParser.cs`,
`PDFObjectStreamParser.cs`, `PDFStreamParser.cs`, `XrefTrailerResolver.cs`, plus 6 xref-type files.

**Remaining:**
- `FDFParser.java` — FDF (Form Data Format) file parser.

### `org.apache.pdfbox.pdfwriter` — ~80%

**Ported (4):** `COSWriter.cs`, `COSStandardOutputStream.cs`, `ContentStreamWriter.cs`,
`Compress/CompressParameters.cs`.

**Remaining:**
- `COSWriterXRefEntry.java` — xref entry for incremental save support.

### `org.apache.pdfbox.pdmodel` (root) — ~90%

**Ported (9):** `PDDocument.cs`, `PDDocumentCatalog.cs`, `PDDocumentInformation.cs`,
`PDPage.cs`, `PDPageTree.cs`, `PageLayout.cs`, `PageMode.cs`.
Also: `RenderingSupportStubs.cs` (stubs for patterns, optional content, etc.) and
`TextStubs.cs` (stubs for PDOutlineItem, PDThreadBead).

**Remaining:** Full xref/cross-reference aware document load path is not yet implemented
(see pdfparser gaps above). `RenderingSupportStubs.cs` and `TextStubs.cs` contain items
that will be promoted to real implementations in future issues.

### `org.apache.pdfbox.pdmodel.encryption` — ~75%

**Ported (11):** `AESKeyLength.cs`, `AccessPermission.cs`, `DecryptionMaterial.cs`,
`MessageDigests.cs`, `PDCryptFilterDictionary.cs`, `PDEncryption.cs`, `ProtectionPolicy.cs`,
`SecurityHandler.cs`, `StandardDecryptionMaterial.cs`, `StandardProtectionPolicy.cs`,
`StandardSecurityHandler.cs`.

**Remaining / partial:**
- `StandardSecurityHandler.PrepareForDecryption` — throws `NotSupportedException`.
  The full RC4/AES decryption flow is not yet implemented.
- `PublicKeySecurityHandler.java` — public-key (certificate-based) decryption — missing.
- `PublicKeyDecryptionMaterial.java` — missing.

### `org.apache.pdfbox.pdmodel.font` — ~60%

**Ported (18):** Real `PDFont` abstract base, `PDVectorFont`, `PDSimpleFont`, `PDType1Font`,
`PDTrueTypeFont`, `PDType0Font`, `PDCIDFont` hierarchy; `PDFontDescriptor`, `PDDictionaryFont`,
`PDCIDSystemInfo`, `PDFontFactory`, `Standard14Fonts`, `PDPanose`, `DefaultFontProvider`,
`FontMapper`, `FontMappers`, `FileSystemFontProvider`; encoding classes
(`DictionaryEncoding`, `Encoding`, `GlyphList`, `MacRomanEncoding`, `SymbolEncoding`,
`Type1Encoding`, `WinAnsiEncoding`, `ZapfDingbatsEncoding`).

**Still missing or stub-level:**
- `PDType3Font` — exists as an abstract stub in `RenderingSupportStubs.cs`; no real
  glyph-width or rendering implementation.
- `PDCIDFontType0.java` — CID font backed by CFF/Type1 outlines; partial or missing.
- Full `FontDescriptorFactory`/font-resolution chain needed for embedded font loading.
- `FontMapper` implementations may be partial.

### `org.apache.pdfbox.pdmodel.graphics` — ~60%

**Ported (24):**
- Color spaces: `PDColorSpace`, `PDColor`, `PDColorSpaceFactory`, `PDDeviceRGB`,
  `PDDeviceCMYK`, `PDDeviceGray`, `PDCalRGB`, `PDCalGray`, `PDLab`, `PDICCBased`,
  `PDIndexed`, `PDSeparation`, `PDDeviceN`, `PDPattern`
- Forms/Images: `PDFormXObject`, `PDTransparencyGroup`, `PDImageXObject`, `PDXObject`
- State: `PDGraphicsState`, `PDTextState`, `PDExtendedGraphicsState`, `PDLineDashPattern`,
  `PDSoftMask`
- Other: `BlendMode`

**Still missing (~16):**
- Shading: `PDShading.java`, `PDShadingType1–7.java` (~8 files) — needed for shading fills
- Optional-content real implementations: `PDOptionalContentProperties`, `PDOptionalContentGroup`
  (stub-only in `RenderingSupportStubs.cs`)
- Pattern real implementations: `PDTilingPattern`, `PDShadingPattern` (stub-only)
- `PDInlineImage.java` — inline image data model
- `PDFunction` subtypes (Type 0, 2, 3, 4) — `PDFunction` base stub exists but no
  concrete implementations for sampled/exponential/stitching/PostScript functions.

### `org.apache.pdfbox.rendering` — ~75%

**Ported (11):** `PDFRenderer.cs`, `PageDrawer.cs`, `PageDrawerParameters.cs`,
`GlyphCache.cs`, `GroupGraphics.cs`, `ImageType.cs`, `RenderDestination.cs`,
`SoftMask.cs`, `TilingPaint.cs`, `TilingPaintFactory.cs`.
Also `AwtStubs.cs` (Java AWT placeholder types for .NET).

**Remaining:**
- `AwtStubs.cs` provides Java AWT placeholder types that allow the rendering code to
  compile. The rendering layer will not produce real rasterized output until these
  stubs are replaced with actual .NET graphics API calls (System.Drawing, SkiaSharp,
  or similar).
- Some `PageDrawer` methods are no-ops or throw due to missing shading/function support.

### `org.apache.pdfbox.util` — ~67%

**Ported (10):** `Vector.cs`, `Matrix.cs`, `AffineTransform.cs`, `DateConverter.cs`,
`Hex.cs`, `NumberFormatUtil.cs`, `GregorianCalendar.cs`, `ParsePosition.cs`,
`SimpleTimeZone.cs`, `StringUtil.cs`.

**Remaining (~5):**
- `SmallMap.java` — small-map optimization for low-entry-count dictionaries
- `IteratorChain.java` — chained iterator over multiple collections
- `CharUtils.java` — character classification utilities (whitespace, line-terminators)
- Potentially 1–2 more minor utilities.

---

## Packages with major gaps ❌

### `org.apache.pdfbox.pdmodel.common` — ~20%

**Ported (3):** `PDRectangle.cs`, `PDStream.cs`, `PDMetadata.cs`.

**Missing (~12 classes):**
- `PDNameTreeNode.java` — name-keyed tree node (for embedded files, destinations)
- `PDNumberTreeNode.java` — number-keyed tree node (for page labels, etc.)
- `PDDestination.java` — navigation destination (GoTo actions, links)
- `PDFileSpecification.java` — file attachment reference
- `PDTextStream.java` — text-type stream wrapper
- `PDPageLabels.java` — page label ranges
- `PDRange.java` — numeric range descriptor
- `PDFunction` concrete types (Type 0 Sampled, Type 2 Exponential, Type 3 Stitching,
  Type 4 PostScript) — base stub exists in `RenderingSupportStubs.cs` but no subtypes

### `org.apache.pdfbox.pdmodel.interactive` — ~7% ❌

**Ported (stubs only):** `PDOutlineItem.cs` (stub), `PDThreadBead.cs` (stub)
— both in `TextStubs.cs`.

**Missing (~28 full implementations):**
- Document outline: `PDDocumentOutline.java`, `PDOutlineNode.java`, `PDOutlineItem.java` (real)
- Actions (~8): `PDActionGoTo.java`, `PDActionLaunch.java`, `PDActionURI.java`,
  `PDActionJavaScript.java`, `PDActionNamed.java`, `PDActionRemoteGoTo.java`, etc.
- Annotations (~15+): `PDAnnotation.java` base + all subtypes (Link, Text, FreeText, Line,
  Square, Circle, Polygon, PolyLine, Highlight, Underline, Squiggly, StrikeOut, Stamp, Caret,
  Ink, Popup, FileAttachment, Sound, Movie, Widget, Screen, PrinterMark, etc.)
- `PDPageTransition.java`
- Viewer preferences, threading, embedded files
- Forms (AcroForm): `PDAcroForm.java`, `PDField.java`, etc.

### `org.apache.pdfbox.pdmodel.documentinterchange` — ~10% ⚠️

**Ported (1):** `PDMarkedContent.cs`.
**Missing (~9):** Tagged PDF structure elements, marked content attributes,
accessibility properties, artifact types, etc.

---

## Key remaining gaps by priority

### Priority 1 — Full PDF document loading (CRITICAL PATH, next large piece)
**Scope:** `org.apache.pdfbox.pdfparser` — full `PDFParser`/`PDFDocumentParser`
orchestration, `PDFObjectStreamParser`, and `PDDocument.Load()` integration.

The current `PDDocument.Load()` extracts only a raw dictionary from the file. Real PDF loading
requires reading the xref table (or cross-reference stream), resolving object references
on-demand, decompressing object streams, and wiring everything into `COSDocument` + `PDDocument`.
Without this, the library cannot load actual PDFs from disk.

**Execution plan (issue series):**
- `issues/37-pdf-loading-parser-scaffold-and-startxref.md`
- `issues/38-pdf-loading-xref-table-and-trailer-resolution.md`
- `issues/39-pdf-loading-xref-stream-and-object-stream-parser.md`
- `issues/40-pdf-loading-cosdocument-resolution-and-pddocument-integration.md`
- `issues/41-pdf-loading-regression-fixtures-roundtrip-and-report-closeout.md`

### Priority 2 — PDModel interactive layer (~28 files) ❌
**Scope:** `org.apache.pdfbox.pdmodel.interactive`

Annotations, actions, bookmarks, outlines, viewer preferences, and forms are entirely absent
(only two empty stubs). These are present in almost every real-world PDF.

**See:** `issues/32-pdmodel-interactive-port.md` (new)

### Priority 3 — Rendering with real .NET graphics
**Scope:** Replace `AwtStubs.cs` with platform-appropriate .NET rendering

The rendering layer compiles and the logic is ported, but `AwtStubs.cs` means no real pixels
are produced. Adopting System.Drawing.Common, SkiaSharp, or Microsoft.Maui.Graphics would
unlock actual PDF-to-image conversion.

**See:** `issues/33-rendering-net-graphics.md` (new)

### Priority 4 — StandardSecurityHandler decryption
**Scope:** `PrepareForDecryption` RC4/AES flow in `StandardSecurityHandler`

Required for loading any password-protected PDF. The data model and structure types are
present; only the cryptographic decryption flow is missing.

**See:** `issues/34-encryption-decryption.md` (new)

### Priority 5 — PDModel/Common completeness (~12 files)
**Scope:** `org.apache.pdfbox.pdmodel.common`

PDNameTreeNode, PDNumberTreeNode, PDDestination, PDFileSpecification, PDPageLabels, PDRange,
PDFunction subtypes. Required for document outlines, embedded files, destinations, and page
label support.

**See:** `issues/35-pdmodel-common-completeness.md` (new)

### Priority 6 — Missing operator processors (2 files)
**Scope:** `b` and `b*` graphics operators

`CloseAndFillNonZeroAndStrokePath` and `CloseAndFillEvenOddAndStrokePath` are defined in
`OperatorName.cs` but have no processor class or registration. Small scope; should be a
quick win bundled into the next operators PR.

**See:** `issues/36-close-fill-operators.md` (new)

---

## Dependency order

```
Full PDF loading series (#37-#41)
    └── PDModel interactive (#32) — annotations/actions can be read from real docs
            └── PDModel common completeness (#35) — tree nodes needed by interactive
Rendering .NET graphics (#33) — mostly independent of above
Encryption decryption (#34) — independent of above
Close/Fill operators (#36) — independent quick win
```

## Total remaining work estimate

| Priority | Issue | Files | Effort |
|---|---|---|---|
| 1 | #37–#41 Full PDF document loading series | ~5 core files + tests/reporting | 3–5 days |
| 2 | #32 PDModel interactive port | ~28 | 4–6 days |
| 3 | #33 Rendering .NET graphics | ~5 (adapt) | 3–5 days |
| 4 | #34 Encryption decryption | ~3 | 1–2 days |
| 5 | #35 PDModel/Common completeness | ~12 | 2–3 days |
| 6 | #36 Close/Fill operators | 2 | 0.5 days |
| | **Total** | **~55** | **~13–20 engineer-days** |
