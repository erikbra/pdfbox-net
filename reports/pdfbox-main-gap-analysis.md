# PDFBox Main Module Gap Analysis

Date: 2026-05-24
Reference upstream Java repository: Apache PDFBox trunk
Reference commit: ccd281cfecedcc0ad39709bece5e67b19a54e8db

## Summary

| Package | Java files (est.) | C# ported | Missing (est.) | % Done |
|---|---|---|---|---|
| `org.apache.pdfbox.cos` | ~30 | 24 | ~6 | ~80% ⚠️ |
| `org.apache.pdfbox.contentstream` (engine) | ~5 | 3 | ~2 | ~60% ⚠️ |
| `org.apache.pdfbox.contentstream.operator` | ~60 | 19 | ~41 | ~32% ❌ |
| `org.apache.pdfbox.filter` | ~18 | 3 | ~15 | ~17% ❌ |
| `org.apache.pdfbox.pdfparser` | ~8 | 9 | ~2 | ~80% ⚠️ |
| `org.apache.pdfbox.pdfwriter` | ~5 | 3 | ~2 | ~60% ⚠️ |
| `org.apache.pdfbox.pdmodel` (root) | ~10 | 7 | ~3 | ~70% ⚠️ |
| `org.apache.pdfbox.pdmodel.common` | ~15 | 1 | ~14 | ~7% ❌ |
| `org.apache.pdfbox.pdmodel.encryption` | ~12 | 0 | ~12 | 0% ❌ |
| `org.apache.pdfbox.pdmodel.font` | ~30 | 3 | ~27 | ~10% ❌ |
| `org.apache.pdfbox.pdmodel.graphics` | ~40 | 5 | ~35 | ~13% ❌ |
| `org.apache.pdfbox.pdmodel.interactive` | ~30 | 2 | ~28 | ~7% ❌ |
| `org.apache.pdfbox.pdmodel.documentinterchange` | ~10 | 1 | ~9 | ~10% ❌ |
| `org.apache.pdfbox.rendering` | ~12 | 11 | ~4 | ~75% ⚠️ |
| `org.apache.pdfbox.text` | ~6 | 6 | 0 | ~100% ✅* |
| `org.apache.pdfbox.util` | ~15 | 3 | ~12 | ~20% ❌ |
| `org.apache.pdfbox.printing` | ~4 | 0 | ~4 | 0% ❌ |
| **TOTAL** | **~320** | **~100** | **~220** | **~31%** |

\* text files are ported but many depend on stubs — not yet functionally complete

---

## Fully ported (no gaps)

### `org.apache.pdfbox.text`
All 6 files are mechanically ported (but functionally depend on completing execution core,
pdmodel font, and filter layers):
- `PDFTextStripper.cs`
- `PDFTextStripperByArea.cs`
- `PDFMarkedContentExtractor.cs`
- `LegacyPDFStreamEngine.cs`
- `TextPosition.cs`
- `TextPositionComparator.cs`

---

## Substantially complete (minor gaps remain)

### `org.apache.pdfbox.cos` — ~80% ported
**Ported (24):** All primitives (COSBase, COSArray, COSDictionary, COSStream, COSName,
COSString, COSBoolean, COSFloat, COSInteger, COSNull, COSNumber, COSObject, COSObjectKey,
COSObjectable, COSOutputStream, COSInputStream, COSDocumentState, COSUpdateInfo, COSUpdateState,
COSIncrement, PDFDocEncoding, ICOSVisitor, ICOSParser, UnmodifiableCOSDictionary).

**Key remaining items:**
- `COSDocument.java` — the top-level COS document container (in-memory PDF object graph)
- `COSDocument` integrates with the parser; currently no C# equivalent

### `org.apache.pdfbox.pdfparser` — ~80% ported
**Ported:** COSParser, PDFStreamParser, XrefTrailerResolver, Xref types.

**Remaining:**
- `PDFDocumentParser.java` — top-level parse orchestration (load a full PDF document)
- `PDFObjectStreamParser.java` / `FDFParser.java`

### `org.apache.pdfbox.pdfwriter` — ~60% ported
**Ported:** COSWriter, COSStandardOutputStream, ContentStreamWriter.

**Remaining:**
- `COSWriterXRefEntry.java` — xref entry for incremental writes
- Full incremental update support

---

## Packages with major gaps

### `org.apache.pdfbox.filter` — ~17% ported (CRITICAL BLOCKER)
Only the abstract base and decode infrastructure are ported. No concrete filter is implemented.
Every compressed PDF stream (including all real-world PDFs with Flate-compressed content)
cannot be decoded until these are ported.

**Ported (3):** `Filter.cs` (abstract base), `DecodeOptions.cs`, `DecodeResult.cs`

**Missing (~15):**
- `FlateFilter.java` — deflate/zlib, used by ~95% of modern PDFs
- `ASCIIHexFilter.java` — hex-encoded streams
- `ASCII85Filter.java` — base-85 encoded streams
- `DCTFilter.java` — JPEG image compression
- `LZWFilter.java` — LZW (legacy, used in older PDFs)
- `RunLengthDecodeFilter.java` — simple run-length encoding
- `CCITTFaxDecodeFilter.java` — Group 3/4 fax-compressed images
- `JBIG2Filter.java` — JBIG2 bi-level image compression
- `JPXFilter.java` — JPEG 2000 image compression
- `CryptFilter.java` — encryption-layer filter
- `FilterFactory.java` — filter registry and factory
- `Predictor.java` — PNG predictor post-processing for Flate/LZW
- `FlateFilter` related inner helpers

### `org.apache.pdfbox.contentstream.operator` — ~32% ported (BLOCKER)
Only text, basic state, marked content, and DrawObject operators are ported. All graphics
path construction, painting, color, inline image, shading, and compatibility operators are absent.

**Ported (19):** All text operators (Tj, TJ, ', ", Tf, Tc, Tw, Tz, TL, Tr, Ts, Td, TD, Tm, T*,
BT, ET), state operators (q, Q, cm, Tm, gs), marked content (BMC, BDC, EMC, MP, DP), DrawObject (Do).

**Missing by category (~41 operator files):**

Color operators (~12):
- `color/SetNonStrokingColor.java` — sc
- `color/SetNonStrokingColorN.java` — scn
- `color/SetNonStrokingColorSpace.java` — cs
- `color/SetNonStrokingDeviceCMYKColor.java` — k
- `color/SetNonStrokingDeviceGrayColor.java` — g
- `color/SetNonStrokingDeviceRGBColor.java` — rg
- `color/SetStrokingColor.java` — SC
- `color/SetStrokingColorN.java` — SCN
- `color/SetStrokingColorSpace.java` — CS
- `color/SetStrokingDeviceCMYKColor.java` — K
- `color/SetStrokingDeviceGrayColor.java` — G
- `color/SetStrokingDeviceRGBColor.java` — RG

Path construction operators (~8):
- `graphics/LineTo.java` — l
- `graphics/MoveTo.java` — m
- `graphics/CurveTo.java` — c
- `graphics/CurveToReplicateFinalPoint.java` — y
- `graphics/CurveToReplicateInitialPoint.java` — v
- `graphics/AppendRectangleToPath.java` — re
- `graphics/ClosePath.java` — h
- `graphics/EndPath.java` — n (no-op path terminator)

Path painting operators (~9):
- `graphics/StrokePath.java` — S
- `graphics/CloseAndStrokePath.java` — s
- `graphics/FillNonZeroRule.java` — f / F
- `graphics/FillEvenOddRule.java` — f*
- `graphics/FillNonZeroAndStrokePath.java` — B
- `graphics/FillEvenOddAndStrokePath.java` — B*
- `graphics/ClipNonZeroRule.java` — W
- `graphics/ClipEvenOddRule.java` — W*
- `graphics/CloseAndFillNonZeroAndStrokePath.java` / `graphics/CloseAndFillEvenOddAndStrokePath.java`

State operators (remaining, ~5):
- `state/SetLineWidth.java` — w
- `state/SetLineCap.java` — J
- `state/SetLineJoin.java` — j
- `state/SetMiterLimit.java` — M
- `state/SetLineDashPattern.java` — d
- `state/SetFlatness.java` — i
- `state/SetRenderingIntent.java` — ri

Inline image (~3):
- `graphics/BeginInlineImage.java` — BI
- `graphics/BeginInlineImageData.java` — ID
- `graphics/EndInlineImage.java` — EI

Other (~4):
- `graphics/ShadingFill.java` — sh
- `type3/SetType3GlyphWidth.java` — d0
- `type3/SetType3GlyphWidthAndBoundingBox.java` — d1
- `compatibility/BeginCompatibilitySection.java` / `EndCompatibilitySection.java` — BX/EX

### `org.apache.pdfbox.pdmodel.font` — ~10% ported
**Ported:** `GlyphList.cs`, `FontStubs.cs` (stubs for PDFont hierarchy), `PDDictionaryFont.cs`

**Missing (~27 classes):**
- `PDType1Font.java` — Type 1 (PostScript) fonts, including standard 14
- `PDTrueTypeFont.java` — TrueType font loading/glyph mapping
- `PDType0Font.java` — Composite (CJK/Unicode) fonts using CMaps
- `PDCIDFontType0.java` — CID font with Type1/CFF outlines
- `PDCIDFontType2.java` — CID font with TrueType outlines
- `PDType3Font.java` — Type 3 (user-defined) fonts
- `PDFont.java` — real abstract base with ToUnicode/widths
- `PDFontLike.java` — interface for font-like objects
- `PDFontDescriptor.java` — real font descriptor with metrics
- `Encoding/DictionaryEncoding.java` — custom character encoding
- `Encoding/MacRomanEncoding.java`, `WinAnsiEncoding.java`, `SymbolEncoding.java`, etc.
- `encoding/Type1Encoding.java`, `GlyphList.java` refinements
- `Standard14Fonts.java` — mapping for the 14 built-in PDF fonts
- `FontMappers.java` / `FontMapper.java` / `DefaultFontProvider.java`
- `PDPanose.java`, `PDCIDSystemInfo.java`, `PDFontFactory.java`

### `org.apache.pdfbox.pdmodel.graphics` — ~13% ported
**Ported:** `PDXObject.cs`, `PDGraphicsState.cs`, `PDTextState.cs`
+ stub types: `PDColorSpace`, `PDColor`, `PDAbstractPattern`, `PDTilingPattern`, `PDShadingPattern`,
  `PDFormXObject`, `PDTransparencyGroup`, `PDSoftMask`, `PDLineDashPattern`, `PDOptionalContent*`,
  `PDImage`, `BlendMode`, `PDFunction*`

**Missing (~35 real implementations):**
- Color spaces: `PDDeviceRGB.java`, `PDDeviceCMYK.java`, `PDDeviceGray.java`, `PDCalRGB.java`,
  `PDCalGray.java`, `PDLab.java`, `PDICCBased.java`, `PDIndexed.java`, `PDSeparation.java`,
  `PDDeviceN.java`, `PDPattern.java`
- Images: `PDImageXObject.java`, `PDInlineImage.java`, `PDImage.java` (real)
- Forms: `PDFormXObject.java` (real), `PDTransparencyGroup.java` (real)
- Shading: `PDShading.java`, `PDShadingType1.java` through `PDShadingType7.java`
- Extended graphics state: `PDExtendedGraphicsState.java`
- Soft mask: `PDSoftMask.java` (real)
- Optional content: `PDOptionalContentProperties.java` (real), `PDOptionalContentGroup.java` (real)

### `org.apache.pdfbox.pdmodel.common` — ~7% ported
**Ported:** `PDRectangle.cs`

**Missing (~14 classes):**
- `PDStream.java` — abstract wrapper around COSStream for pdmodel
- `PDMetadata.java` — XMP metadata access
- `PDNameTreeNode.java`, `PDNumberTreeNode.java` — tree structures
- `PDDestination.java` — navigation targets
- `PDFileSpecification.java` — file attachment references
- `PDTextStream.java` — text-type stream
- `PDPageLabels.java` — page label ranges
- `PDRange.java` — numeric range
- `function/PDFunction*.java` (~6 function types)

### `org.apache.pdfbox.pdmodel.encryption` — 0% ported
Required for loading password-protected PDFs.

**Missing (~12 classes):**
- `StandardDecryptionMaterial.java` / `PublicKeyDecryptionMaterial.java`
- `AccessPermission.java`
- `PDEncryption.java`
- `SecurityHandler.java` / `StandardSecurityHandler.java` / `PublicKeySecurityHandler.java`
- `CryptFilterDictionary.java`
- `PDCryptFilterDictionary.java`
- `MessageDigests.java` / `RC4Cipher.java`
- `AESKeyLength.java`

### `org.apache.pdfbox.pdmodel.interactive` — ~7% ported
**Ported (stubs):** `PDOutlineItem.cs`, `PDThreadBead.cs`

**Missing (~28 full implementations):**
- `PDDocumentOutline.java` / `PDOutlineNode.java` / `PDOutlineItem.java` (real)
- `PDActionGoTo.java`, `PDActionLaunch.java`, `PDActionURI.java`, and other action types
- `PDAnnotation` hierarchy (~15+ annotation types)
- `PDDocumentCatalog` navigation helpers
- `PDPageTransition.java`
- Viewer preferences, threading, embedded files, etc.

### `org.apache.pdfbox.util` — ~20% ported
**Ported:** `Vector.cs`, `Matrix.cs`, `AffineTransform.cs`

**Missing (~12 classes):**
- `DateConverter.java` — PDF date string ↔ Java/C# date parsing
- `Hex.java` — hex encoding/decoding utilities
- `NumberFormatUtil.java` — number formatting for PDF output
- `SmallMap.java` — small-map optimization
- `IteratorChain.java`
- `CharUtils.java`
- Various other small utilities

### `org.apache.pdfbox.printing` — 0% ported
Required for printing PDF pages via OS print APIs.

**Missing (~4 classes):**
- `PDFPrintable.java` — Printable adapter for a single PDF page
- `PDFPageable.java` — Pageable adapter for a PDF document
- `PrintOrientation.java` — orientation enum
- `Scaling.java` — scaling mode enum

---

## Key gaps by priority

### Priority 1 — Filter implementations (CRITICAL PATH)
**Scope:** `org.apache.pdfbox.filter` (~15 files)
Without `FlateFilter` (zlib/deflate), real-world PDF streams cannot be decoded. All compressed
content in PDF — text streams, image data, font programs, cross-reference streams — uses Flate.
This is a hard blocker for any end-to-end PDF processing.
**Size estimate:** Medium (~15 files, ~2–3 engineer-days)
**See:** `issues/19-filter-implementations.md`

### Priority 2 — ContentStream graphics/color/path operators (~41 operator files)
**Scope:** Missing color, path construction, path painting, state, and inline image operators
Required for page rendering and full content stream processing. Without these, the page
renderer can only process text content.
**Size estimate:** Medium (~41 files, ~2–4 engineer-days)
**See:** `issues/20-contentstream-graphics-operators.md`

### Priority 3 — PDModel font full port (~27 files)
**Scope:** Replace `FontStubs.cs` with real font implementations
Required for: actual character code to Unicode mapping, glyph width lookups, Type1/TrueType
loading, and font subsetting (for PDF creation).
**Size estimate:** Large (~27 files, ~4–6 engineer-days)
**See:** `issues/21-pdmodel-font-full-port.md`

### Priority 4 — PDModel color spaces (~20 files)
**Scope:** Replace color-space stubs in `RenderingSupportStubs.cs` with real implementations
Required for: rendering, image extraction, and any operation involving color.
**Size estimate:** Medium (~20 files, ~3–4 engineer-days)
**See:** `issues/22-pdmodel-color-spaces.md`

### Priority 5 — PDModel common + graphics support (~25 files)
**Scope:** PDStream, PDMetadata, real form/image/shading XObjects, extended graphics state
Required for: complex PDF content processing, XObject rendering, transparency, optional content.
**Size estimate:** Medium-Large (~25 files, ~3–5 engineer-days)
**See:** `issues/23-pdmodel-graphics-common.md`

### Priority 6 — PDModel interactive + encryption (~40 files)
**Scope:** Annotations, actions, outlines, encryption, document navigation
Required for: loading protected PDFs, UI-layer features (annotations, bookmarks, forms).
**Size estimate:** Large (~40 files, ~5–7 engineer-days)
**See:** `issues/24-pdmodel-interactive-encryption.md`

### Priority 7 — Util completeness + DateConverter + printing (~20 files)
**Scope:** Remaining pdfbox.util classes, printing module
Required for: correct date handling, hex utilities, printing support.
**Size estimate:** Small-Medium (~20 files, ~2–3 engineer-days)
**See:** `issues/25-util-printing-completeness.md`

---

## Dependency order

```
Filter implementations (#19)
    └── ContentStream graphics operators (#20)
            └── PDModel font full port (#21)
                    └── PDModel color spaces (#22)
                            └── PDModel graphics/common (#23)
                                    └── PDModel interactive/encryption (#24)

Util completeness (#25) — mostly independent, can run in parallel
```

## Total remaining work estimate

| Priority | Issue | Files | Effort |
|---|---|---|---|
| 1 | #19 Filter implementations | ~15 | 2–3 days |
| 2 | #20 ContentStream graphics operators | ~41 | 2–4 days |
| 3 | #21 PDModel font full port | ~27 | 4–6 days |
| 4 | #22 PDModel color spaces | ~20 | 3–4 days |
| 5 | #23 PDModel graphics/common | ~25 | 3–5 days |
| 6 | #24 PDModel interactive/encryption | ~40 | 5–7 days |
| 7 | #25 Util + printing | ~20 | 2–3 days |
| | **Total** | **~188** | **~21–32 engineer-days** |
