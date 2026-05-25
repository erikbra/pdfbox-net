# PDFBox Main Module Gap Analysis

Date: 2026-05-25 (updated)
Previous date: 2026-05-25 (coverage refresh)
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
| `org.apache.pdfbox.pdmodel.graphics` | ~40 | 33 | ~8 | ~82% ⚠️ |
| `org.apache.pdfbox.pdmodel.interactive` | ~35 | 22 | ~13 | ~63% ⚠️ |
| `org.apache.pdfbox.pdmodel.documentinterchange` | ~10 | 22 | 0 | ~100% ✅ |
| `org.apache.pdfbox.rendering` | ~12 | 11 | ~4 | ~75% ⚠️ |
| `org.apache.pdfbox.text` | ~6 | 6 | 0 | ~100% ✅* |
| `org.apache.pdfbox.util` | ~9 | 6 | ~3 | ~67% ⚠️ |
| `org.apache.pdfbox.printing` | ~4 | 4 | 0 | ~100% ✅ |
| **TOTAL** | **~334** | **~297** | **~37** | **~89%** |

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

- Conversion rows: **340** (`328` unique source->target pairs)
- Port modes:
  - `mechanical`: **151**
  - `adapted`: **147**
  - `native-test`: **38**
  - `partial`: **4**
  - `adapted-minimal`: **2**
- Upstream test mappings converted: **25**

### Traceability status coverage

- Traceability rows: **349**
- `in-sync`: **267** (~76.5%)
- `partially-in-sync`: **28** (~8.0%)
- `partial`: **10** (~2.9%)
- blank status (needs classification/backfill): **44** (~12.6%)

### Immediate coverage-report follow-up

- The highest-leverage report cleanup remains classifying the **44** blank-status
  traceability rows (mostly `contentstream/operator/*` mappings) as explicit
  `in-sync`/`partial`/`deferred` statuses.
- `PDFObjectStreamParser` is still missing an explicit traceability parity row/status entry
  even though parser milestone work is complete.

## Completed since previous edition ✅

- **Issue #53 (`pdmodel/graphics/shading` core and base types) is complete.**
  - Ported the full `PDShading` abstract base class with all dictionary accessors
    (`Background`, `BBox`, `AntiAlias`, `ColorSpace` via `CS`/`ColorSpace` dual key)
    and the complete `PDShading.Create()` factory routing all 7 shading subtypes.
  - Ported `PDShadingType1` (function-based): `Domain`, `Matrix` accessors.
  - Ported `PDShadingType2` (axial): `Coords`, `Domain`, `Extend` accessors.
  - Ported `PDShadingType3` (radial): extends `PDShadingType2`.
  - Ported `PDTriangleBasedShadingType` (abstract base for types 4–7):
    `BitsPerComponent`, `BitsPerCoordinate`, `NumberOfColorComponents`, `Decode`
    accessors, and `Interpolate` helper. Stream rendering deferred.
  - Ported `PDShadingType4` (free-form Gouraud): `BitsPerFlag` accessor.
  - Ported `PDShadingType5` (lattice Gouraud): `VerticesPerRow` accessor.
  - Ported `PDMeshBasedShadingType` (abstract base for types 6–7): hierarchy wiring.
  - Ported `PDShadingType6` (Coons patch mesh) and `PDShadingType7` (tensor-product
    patch mesh). Patch generation/rendering deferred.
  - Wired function dependencies: `SetFunction(PDFunction)` / `SetFunction(COSArray)` /
    `GetFunction()` / `EvalFunction(float)` / `EvalFunction(float[])` with per-spec
    output clamping to `[0,1]`.
  - Added `PDResources.GetShading(COSName)` and `GetShadingNames()`.
  - Added 12 new `COSName` constants: `SHADING_TYPE`, `BACKGROUND`, `BBOX`,
    `ANTI_ALIAS`, `CS`, `SHADING`, `COORDS`, `EXTEND`, `BITS_PER_COORDINATE`,
    `BITS_PER_FLAG`, `VERTICES_PER_ROW`, `MATRIX`.
  - Added `PDShadingTest` (43 tests): factory routing, all dictionary accessor
    round-trips, function wiring, `EvalFunction` clamping, `PDResources` shading
    lookup, and `COSName` constant value assertions.
  - Deferred: `toPaint()` rendering hooks, `collectTriangles`/`collectPatches`
    stream-based rendering methods (targeted for issue #56).
  - Updated conversion/normalization/traceability artifacts.
- **Issue #47 (`pdmodel/documentinterchange` regression + traceability closeout) is complete.**
  - Added deterministic tagged PDF fixture coverage (`Fixtures/TaggedPdf/minimal-tagged.pdf`) with
    end-to-end catalog → `StructTreeRoot` → `ParentTree` traversal assertions.
  - Completed remaining documentinterchange class ports for current parity target:
    `PDMarkInfo`, `PDParentTreeValue`, `PDPropertyList`, `PDBoxStyle`,
    `PDArtifactMarkedContent`, `PDPrintFieldAttributeObject`,
    `PDExportFormatAttributeObject`, `PDFourColours`, and `StandardStructureTypes`.
  - Extended `PDAttributeObject.Create()` owner dispatch for `PrintField` and export-format owners.
  - Added `PDDocumentCatalog.GetMarkInfo()/SetMarkInfo()` integration accessors.
  - Updated conversion/normalization/traceability artifacts and set all documentinterchange
    traceability rows to explicit `in-sync` statuses.
- **Issue #46 (`pdmodel/documentinterchange` parent-tree and integration) is complete.**
  - `PDParentTreeNumberTreeNode`: typed number-tree node for the parent tree; handles both
    single structure-element dictionary values and page-level arrays of structure elements.
  - `PDStructureTreeRoot.GetParentTree()` / `SetParentTree()` / `GetParentTreeEntries(int)`:
    fully wired parent-tree accessors enabling end-to-end page-key → structure-element resolution.
  - `PDLayoutAttributeObject`, `PDListAttributeObject`, `PDTableAttributeObject`: typed
    tagged-PDF attribute object subtypes; `PDAttributeObject.Create()` factory dispatches to them.
  - `PDParentTreeIntegrationTest` (29 tests): covers parent-tree round-trip, single-element and
    array-value lookups, missing-key guard, multi-page stability, end-to-end catalog traversal,
    all three new attribute subtypes, and factory dispatch.
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
- Full `FontDescriptorFactory`/font-substitution chain remains deferred, but descriptor fallback,
  CID width defaults, and deterministic provider lookup are now wired for the current parity slice.

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

**Direct upstream util files ported (6/9):**
- `Vector.cs`, `Matrix.cs`, `DateConverter.cs`, `Hex.cs`, `NumberFormatUtil.cs`, `StringUtil.cs`

**Additional supporting utility adapters (non-upstream helpers):**
- `AffineTransform.cs`, `GregorianCalendar.cs`, `ParsePosition.cs`, `SimpleTimeZone.cs`

**Remaining direct upstream util files (~3):**
- `IterativeMergeSort.java`
- `Version.java`
- `XMLUtil.java`

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

### `org.apache.pdfbox.pdmodel.documentinterchange` — ~100% ✅

**Ported (22):** `PDMarkedContent.cs`, `PDStructureNode.cs`, `PDStructureTreeRoot.cs`,
`PDStructureElement.cs`, `Revisions.cs`, `PDMarkedContentReference.cs`, `PDObjectReference.cs`,
`PDAttributeObject.cs`, `PDDefaultAttributeObject.cs`, `PDUserAttributeObject.cs`, `PDUserProperty.cs`,
`PDParentTreeNumberTreeNode.cs`, `PDLayoutAttributeObject.cs`, `PDListAttributeObject.cs`,
`PDTableAttributeObject.cs`, `PDMarkInfo.cs`, `PDParentTreeValue.cs`, `PDPropertyList.cs`,
`PDBoxStyle.cs`, `PDArtifactMarkedContent.cs`, `PDPrintFieldAttributeObject.cs`,
`PDExportFormatAttributeObject.cs`, `PDFourColours.cs`, `StandardStructureTypes.cs`.
**Completed in issue #46:**
- `PDParentTreeNumberTreeNode`: typed number-tree node for the `ParentTree` entry; handles both
  single structure-element dictionary values and page-level arrays of structure elements.
- `PDStructureTreeRoot.GetParentTree()` / `SetParentTree()` / `GetParentTreeEntries(int)`: fully
  wired parent-tree accessors with end-to-end page-key → structure-element resolution.
- `PDLayoutAttributeObject`, `PDListAttributeObject`, `PDTableAttributeObject`: typed tagged-PDF
  attribute object subtypes with owner dispatch in `PDAttributeObject.Create()`.
**Issue #47 closeout:**
- Deterministic tagged fixture and regression tests now cover structure tree, parent-tree lookups,
  attribute-owner dispatch, artifact subtype dispatch, and closeout helper types.
- Traceability rows in this package are fully classified with explicit `in-sync` statuses.

---

## Key remaining gaps by priority

### Priority 1 — Close remaining PDModel/font milestone work ⚠️
**Scope:** `org.apache.pdfbox.pdmodel.font`

The font milestone is in late-stage execution (issues #48-#50 are complete; #51-#52 remain):
- Complete Type0/CIDType0 + composite Unicode mapping parity.
- Finish regression/traceability closeout for the full font package.

**See:**
- `issues/51-pdmodel-font-type0-cidtype0-and-unicode-integration.md`
- `issues/52-pdmodel-font-regression-coverage-and-traceability-closeout.md`

### Priority 2 — Next large chunk: PDModel graphics completion (new issue series) ⚠️
**Scope:** `org.apache.pdfbox.pdmodel.graphics` + graphics-dependent execution paths

After the font closeout, `pdmodel.graphics` is the highest-leverage chunk to convert fully:
- Multiple high-impact graphics classes remain partially implemented or stub-backed
  (`PDShading*`, `PDInlineImage`, optional-content and pattern integrations).
- Rendering and content-stream fidelity still depend on this package being functionally complete.
- This chunk can be executed in dependency-safe slices with fixture-backed regression coverage.

Planned execution issues:
- `issues/53-pdmodel-graphics-shading-and-core-types.md`
- `issues/54-pdmodel-graphics-patterns-optional-content-and-inline-image.md`
- `issues/55-pdmodel-graphics-state-and-xobject-integration.md`
- `issues/56-graphics-contentstream-and-rendering-integration.md`
- `issues/57-pdmodel-graphics-regression-coverage-and-traceability-closeout.md`

### Priority 3 — PDModel interactive completion and parity hardening ⚠️
**Scope:** `org.apache.pdfbox.pdmodel.interactive`

Interactive parity remains high value, but should follow the graphics milestone so
annotation appearance, optional content, and form visuals have stable graphics behavior.

**See:** `issues/32-pdmodel-interactive-port.md`

### Priority 4 — Rendering backend modernization
**Scope:** Replace `AwtStubs.cs` with platform-appropriate .NET rendering

The rendering layer compiles and the logic is ported, but `AwtStubs.cs` means no real pixels
are produced. Backend replacement should follow graphics completion to avoid chasing stub-only paths.

**See:** `issues/33-rendering-net-graphics.md`

### Priority 5 — StandardSecurityHandler decryption
**Scope:** `PrepareForDecryption` RC4/AES flow in `StandardSecurityHandler`

Required for loading any password-protected PDF. The data model and structure types are
present; only the cryptographic decryption flow is missing.

**See:** `issues/34-encryption-decryption.md`

### Priority 6 — Missing operator processors (2 files)
**Scope:** `b` and `b*` graphics operators

`CloseAndFillNonZeroAndStrokePath` and `CloseAndFillEvenOddAndStrokePath` are defined in
`OperatorName.cs` but have no processor class or registration. Small scope; should be a
quick win bundled into the next operators PR.

**See:** `issues/36-close-fill-operators.md`

---

## Dependency order

```
Finish PDModel font closeout (#51–#52)
Complete PDModel graphics chunk (#53–#57)
PDModel interactive hardening (#32 remaining scope)
Rendering .NET graphics backend replacement (#33)
Encryption decryption (#34)
Close/Fill operators (#36) — independent quick win
```

## Total remaining work estimate

| Priority | Issue | Files | Effort |
|---|---|---|---|
| 1 | #51–#52 PDModel font closeout | ~6–10 remaining | 2–3 days |
| 2 | #53–#57 PDModel graphics completion | ~14–22 remaining | 5–8 days |
| 3 | #32 PDModel interactive completion | ~10–16 remaining | 3–5 days |
| 4 | #33 Rendering .NET graphics backend | ~5 (adapt) | 3–5 days |
| 5 | #34 Encryption decryption | ~3 | 1–2 days |
| 6 | #36 Close/Fill operators | 2 | 0.5 days |
| | **Total** | **~40–58** | **~14.5–23.5 engineer-days** |
