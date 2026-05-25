# PDFBox Main Module Gap Analysis

Date: 2026-05-25 (updated)
Previous date: 2026-05-25
Reference upstream Java repository: Apache PDFBox trunk
Reference commit: ccd281cfecedcc0ad39709bece5e67b19a54e8db

## Summary

| Package | Java files (est.) | C# ported | Missing (est.) | % Done |
|---|---|---|---|---|
| `org.apache.pdfbox.cos` | ~26 | 25 | ~1 | ~96% ✅ |
| `org.apache.pdfbox.contentstream` (engine) | ~5 | 3 | ~2 | ~60% ⚠️ |
| `org.apache.pdfbox.contentstream.operator` | ~60 | 73† | ~2 | ~97% ✅ |
| `org.apache.pdfbox.filter` | ~18 | 17 | ~1 | ~94% ✅ |
| `org.apache.pdfbox.pdfparser` | ~8 | 11† | ~1 | ~90% ✅ |
| `org.apache.pdfbox.pdfwriter` | ~5 | 4 | ~1 | ~80% ⚠️ |
| `org.apache.pdfbox.pdmodel` (root) | ~10 | 9 | ~1 | ~90% ✅ |
| `org.apache.pdfbox.pdmodel.common` | ~35 | 34 | ~1 | ~97% ✅ |
| `org.apache.pdfbox.pdmodel.encryption` | ~12 | 11 | ~3 | ~75% ⚠️ |
| `org.apache.pdfbox.pdmodel.font` | ~30 | 18 | ~12 | ~60% ⚠️ |
| `org.apache.pdfbox.pdmodel.graphics` | ~40 | 24 | ~16 | ~60% ⚠️ |
| `org.apache.pdfbox.pdmodel.interactive` | ~35 | 22 | ~13 | ~63% ⚠️ |
| `org.apache.pdfbox.pdmodel.documentinterchange` | ~10 | 1 | ~9 | ~10% ⚠️ |
| `org.apache.pdfbox.rendering` | ~12 | 11 | ~4 | ~75% ⚠️ |
| `org.apache.pdfbox.text` | ~6 | 6 | 0 | ~100% ✅* |
| `org.apache.pdfbox.util` | ~15 | 10 | ~5 | ~67% ⚠️ |
| `org.apache.pdfbox.printing` | ~4 | 4 | 0 | ~100% ✅ |
| **TOTAL** | **~340** | **~283** | **~57** | **~83%** |

† The C# operator count (73) is higher than the Java ~60 because some Java files each handle
a single operator while the C# port includes the Operator/OperatorName/OperatorProcessor base
infrastructure separately. The PDFParser C# files also include xref-type files that don't have
direct Java counterparts.

\* text files are ported but some depend on incomplete pdmodel font and rendering layers

---

## Coverage refresh from traceability artifacts (2026-05-25)

This snapshot was recalculated from current report data in:
- `reports/conversion-records.json`
- `reports/normalization-records.json`
- `reports/traceability-parity-report.json`

### Conversion inventory

- Conversion rows: **291** (`281` unique source->target pairs)
- Port modes:
  - `mechanical`: **143**
  - `adapted`: **110**
  - `native-test`: **32**
  - `partial`: **4**
  - `adapted-minimal`: **2**
- Upstream test mappings converted: **25**

### Traceability status coverage

- Traceability rows: **296**
- `in-sync`: **223** (~75.3%)
- `partially-in-sync`: **18** (~6.2%)
- `partial`: **11** (~3.8%)
- blank status (needs classification/backfill): **44** (~14.9%)

### Immediate coverage-report follow-up

- The highest-leverage report cleanup remains classifying the **44** blank-status
  traceability rows (mostly `contentstream/operator/*` mappings) as explicit
  `in-sync`/`partial`/`deferred` statuses.
- `PDFObjectStreamParser` now has a conversion record row but still needs an explicit
  traceability parity row/status entry.

## Completed since previous edition ✅

- **Issue #35 (`pdmodel/common`) is functionally complete for this slice.**
  The package is no longer a major gap: it now includes tree nodes, file specification
  types, page labels/ranges, and the full `PDFunction` Type 0/2/3/4 stack with Type 4
  parser/operators.
- **Issues #37 and #38 (parser scaffold + classic xref parsing) are complete and merged.**
  Header/version parsing, `startxref` discovery, classic xref table parsing, trailer
  resolution, `/Prev` loop guards, and malformed xref validation are in place with tests.
- **`PDDocument.Load()` is now wired to `PDFParser`** with deterministic fixture coverage
  for classic xref, flate-content, and xref-stream paths (`FullPdfDocumentLoadingTest`).
- **Issue #41 (parser milestone closeout) is complete.**
  - `PDDocumentLoadSaveRoundtripTest` added: fixture-driven load → save → reload smoke
    checks for all four parser paths (classic xref, flate-content, xref-stream,
    object-stream). All 4 fixture types roundtrip successfully with page count and
    metadata (title, author) preserved.
  - `PDDocument.Save()` improved: now writes proper indirect object bodies (`N 0 obj …
    endobj`) with a correct xref table, resolving the circular-reference stack overflow
    that occurred when saving loaded PDFs with back-references in the page tree.
  - `COSDictionary.WriteValuePDF` fixed: COSObjects with keys are now serialized as
    `N M R` indirect references instead of being inlined recursively.
  - Reporting artifacts (`conversion-records.json`, `normalization-records.json`,
    `traceability-parity-report.json`) updated with entries for
    `FullPdfDocumentLoadingTest.cs`, `PDFParserXrefStreamObjectStreamTest.cs`, and
    `PDDocumentLoadSaveRoundtripTest.cs`.

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

### `org.apache.pdfbox.pdfparser` — ~93% ✅

**Ported (11 C# files):** `COSParser.cs`, `PDFDocumentParser.cs`, `PDFParser.cs`,
`PDFObjectStreamParser.cs`, `PDFStreamParser.cs`, `XrefTrailerResolver.cs`, plus 6 xref-type files.

**Completed in latest slices (#37/#38/#39/#41):**
- `%PDF-` header/version parsing and deterministic `startxref` lookup.
- Classic xref-table + trailer parsing with `/Prev` recursion guard behavior.
- Malformed xref subsection/entry validation with dedicated regression tests.
- Xref-stream and object-stream (type-1 and type-2) parsing with full fixture coverage.
- `PDDocument.Save()` improved to write proper indirect object bodies + xref table,
  enabling load → save → reload roundtrips for all parser-integrated fixture paths.
- `COSDictionary.WriteValuePDF` fixed to write indirect references as `N M R` instead of
  recursively inlining, preventing circular-reference stack overflows.

**Remaining:**
- `FDFParser.java` — FDF (Form Data Format) parser is still missing.
- `COSParser` parity beyond the currently tracked low-level subset.

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

**Completed recently:**
- `PDDocument.Load(...)` now routes through `PDFParser` and can load deterministic fixture PDFs
  through xref-table/xref-stream paths.

**Remaining:** `RenderingSupportStubs.cs` and `TextStubs.cs` still contain deferred placeholder
implementations that should be promoted to real types in future slices.

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

## Packages with major shifts and remaining gaps

### `org.apache.pdfbox.pdmodel.common` — ~95% ✅

**Now ported (34+ files):**
- Tree nodes: `PDNameTreeNode`, `PDNumberTreeNode`
- File specifications: `PDFileSpecification`, `PDSimpleFileSpecification`,
  `PDComplexFileSpecification`, `PDEmbeddedFile`
- Labels/ranges/wrappers: `PDPageLabels`, `PDPageLabelRange`, `PDRange`,
  `PDDictionaryWrapper`, `PDTypedDictionaryWrapper`, `PDDestinationOrAction`
- Function stack: `PDFunction` + Type 0/2/3/4 + Type4 parser/operator infrastructure
- Existing core types remain: `PDRectangle`, `PDStream`, `PDMetadata`, `COSArrayList`

**Remaining (minor):**
- `PDRectangle` still has deferred transform/path parity methods.
- Report hygiene: one `partially-in-sync` status remains for `PDRectangle`.

### `org.apache.pdfbox.pdmodel.interactive` — ~65% ⚠️

**Now ported (major expansion):**
- Outline/navigation: `PDDocumentOutline`, `PDOutlineNode`, `PDOutlineItem`, destination types.
- Actions: `PDAction`, `PDActionFactory`, `PDActionGoTo`, `PDActionLaunch`,
  `PDActionURI`, `PDActionJavaScript`, `PDActionNamed`, `PDActionRemoteGoTo`.
- Annotation base + multiple concrete subtypes (Link, Text, FreeText, Line, Square, Circle,
  Highlight/Underline/Squiggly/StrikeOut, Stamp, FileAttachment, Widget, etc.).
- Forms baseline: `PDAcroForm`, `PDField`, `PDTextField`, `PDCheckBox`, `PDUnknownField`.

**Remaining:**
- AcroForm appearance/value pipeline is still partial.
- Interactive coverage remains uneven (several classes still `partially-in-sync`).

### `org.apache.pdfbox.pdmodel.documentinterchange` — ~35% ⚠️

**Ported (5):** `PDMarkedContent.cs`, `PDStructureNode.cs`, `PDStructureTreeRoot.cs`,
`PDStructureElement.cs`, `Revisions.cs`.
**Missing (~6):** Tagged PDF reference wrappers, attribute objects, class/role-map
parity, and remaining accessibility/tagged-PDF support types.

---

## Key remaining gaps by priority

### Priority 1 — Full `pdmodel.documentinterchange` conversion (tagged PDF/logical structure) ⚠️
**Scope:** `org.apache.pdfbox.pdmodel.documentinterchange`

This remains the most under-converted major package in the `pdfbox` module
(~10% mapped in this report). It is also a coherent package boundary that can be
driven to full parity in a focused milestone instead of partial cross-package work.

Planned execution issues:
- `issues/43-documentinterchange-structure-tree-core.md`
- `issues/44-documentinterchange-marked-content-and-object-references.md`
- `issues/45-documentinterchange-attributes-and-role-map.md`
- `issues/46-documentinterchange-parent-tree-and-integration.md`
- `issues/47-documentinterchange-regression-traceability-closeout.md`

### Priority 2 — PDModel interactive completion and parity hardening ⚠️
**Scope:** `org.apache.pdfbox.pdmodel.interactive`

The parser/load milestone (#37–#41) is now complete. All fixture types (classic xref,
flate-content, xref-stream, object-stream) load and roundtrip correctly.

Interactive parity hardening remains high value, but is now positioned after finishing
the `documentinterchange` package to fully close one major untouched area first.

**See:** `issues/32-pdmodel-interactive-port.md`

### Priority 3 — Rendering with real .NET graphics
**Scope:** Replace `AwtStubs.cs` with platform-appropriate .NET rendering

The rendering layer compiles and the logic is ported, but `AwtStubs.cs` means no real pixels
are produced. Adopting System.Drawing.Common, SkiaSharp, or Microsoft.Maui.Graphics would
unlock actual PDF-to-image conversion.

**See:** `issues/33-rendering-net-graphics.md`

### Priority 4 — StandardSecurityHandler decryption
**Scope:** `PrepareForDecryption` RC4/AES flow in `StandardSecurityHandler`

Required for loading any password-protected PDF. The data model and structure types are
present; only the cryptographic decryption flow is missing.

**See:** `issues/34-encryption-decryption.md`

### Priority 5 — Missing operator processors (2 files)
**Scope:** `b` and `b*` graphics operators

`CloseAndFillNonZeroAndStrokePath` and `CloseAndFillEvenOddAndStrokePath` are defined in
`OperatorName.cs` but have no processor class or registration. Small scope; should be a
quick win bundled into the next operators PR.

**See:** `issues/36-close-fill-operators.md`

---

## Dependency order

```
PDModel documentinterchange full conversion (#43–#47)
PDModel interactive hardening (#32 remaining scope)
Rendering .NET graphics (#33) — mostly independent of above
Encryption decryption (#34) — independent of above
Close/Fill operators (#36) — independent quick win
```

## Total remaining work estimate

| Priority | Issue | Files | Effort |
|---|---|---|---|
| 1 | #43–#47 PDModel documentinterchange full conversion | ~12–18 | 4–6 days |
| 2 | #32 PDModel interactive completion | ~10–16 remaining | 3–5 days |
| 3 | #33 Rendering .NET graphics | ~5 (adapt) | 3–5 days |
| 4 | #34 Encryption decryption | ~3 | 1–2 days |
| 5 | #36 Close/Fill operators | 2 | 0.5 days |
| | **Total** | **~32–44** | **~11.5–18.5 engineer-days** |
