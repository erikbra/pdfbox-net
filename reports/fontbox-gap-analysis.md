# FontBox Port Gap Analysis

Date: 2026-05-23  
Reference commit (Java upstream): `6b9b255eb471b384bac3d2d55c4e47f24fac6dac` (trunk)  
Reference commit (C# ported): current branch

## Summary

| Package | Java files | C# ported | Missing | % Done |
|---|---|---|---|---|
| `org.apache.fontbox` (root) | 2 | 2 | 0 | 100% ✅ |
| `org.apache.fontbox.afm` | 8 | 0 | 8 | 0% ❌ |
| `org.apache.fontbox.cff` | 26 | 11 | 15 | 42% ⚠️ |
| `org.apache.fontbox.cmap` | 5 | 0 | 5 | 0% ❌ |
| `org.apache.fontbox.encoding` | 4 | 4 | 0 | 100% ✅ |
| `org.apache.fontbox.pfb` | 1 | 1 | 0 | 100% ✅ |
| `org.apache.fontbox.ttf` (root) | 44 | 12 | 32 | 27% ⚠️ |
| `org.apache.fontbox.ttf.gsub` | 13 | 0 | 13 | 0% ❌ |
| `org.apache.fontbox.ttf.model` | 5 | 0 | 5 | 0% ❌ |
| `org.apache.fontbox.ttf.table.common` | 12 | 0 | 12 | 0% ❌ |
| `org.apache.fontbox.ttf.table.gsub` | 9 | 0 | 9 | 0% ❌ |
| `org.apache.fontbox.type1` | 6 | 6 | 0 | 100% ✅ |
| `org.apache.fontbox.util` | 1 | 1 | 0 | 100% ✅ |
| `org.apache.fontbox.util.autodetect` | 7 | 7 | 0 | 100% ✅ |
| **TOTAL** | **143** | **44** | **99** | **31%** |

> Note: `src/PdfBox.Net.FontBox/Util/Geometry/GeneralPath.cs` is a C# utility that has no 1:1 Java counterpart in fontbox proper; it is excluded from the count above.

---

## Packages fully ported ✅

### `org.apache.fontbox` (root)
All 2 files ported:
- `EncodedFont.cs` ← `EncodedFont.java`
- `FontBoxFont.cs` ← `FontBoxFont.java`

### `org.apache.fontbox.encoding`
All 4 files ported:
- `Encoding.cs` ← `Encoding.java`
- `BuiltInEncoding.cs` ← `BuiltInEncoding.java`
- `MacRomanEncoding.cs` ← `MacRomanEncoding.java`
- `StandardEncoding.cs` ← `StandardEncoding.java`

### `org.apache.fontbox.pfb`
All 1 file ported:
- `PfbParser.cs` ← `PfbParser.java`

### `org.apache.fontbox.type1`
All 6 files ported:
- `DamagedFontException.cs` ← `DamagedFontException.java`
- `Token.cs` ← `Token.java`
- `Type1CharStringReader.cs` ← `Type1CharStringReader.java`
- `Type1Font.cs` ← `Type1Font.java`
- `Type1Lexer.cs` ← `Type1Lexer.java`
- `Type1Parser.cs` ← `Type1Parser.java`

### `org.apache.fontbox.util`
All 1 file ported:
- `BoundingBox.cs` ← `BoundingBox.java`

### `org.apache.fontbox.util.autodetect`
All 7 files ported:
- `FontDirFinder.cs` ← `FontDirFinder.java`
- `FontFileFinder.cs` ← `FontFileFinder.java`
- `MacFontDirFinder.cs` ← `MacFontDirFinder.java`
- `NativeFontDirFinder.cs` ← `NativeFontDirFinder.java`
- `OS400FontDirFinder.cs` ← `OS400FontDirFinder.java`
- `UnixFontDirFinder.cs` ← `UnixFontDirFinder.java`
- `WindowsFontDirFinder.cs` ← `WindowsFontDirFinder.java`

---

## Packages partially ported ⚠️

### `org.apache.fontbox.cff` — 11/26 ported

**Ported (11):**
- `CFFCIDFont.cs` ← `CFFCIDFont.java`
- `CFFCharset.cs` ← `CFFCharset.java`
- `CFFCharsetType1.cs` ← `CFFCharsetType1.java`
- `CFFEncoding.cs` ← `CFFEncoding.java`
- `CFFFont.cs` ← `CFFFont.java`
- `CFFParser.cs` ← `CFFParser.java`
- `CFFStandardEncoding.cs` ← `CFFStandardEncoding.java`
- `CFFStandardString.cs` ← `CFFStandardString.java`
- `CFFType1Font.cs` ← `CFFType1Font.java`
- `Type1CharString.cs` ← `Type1CharString.java`
- `Type2CharString.cs` ← `Type2CharString.java`

**Missing (15):**
- `CFFCharsetCID.java` — CID-keyed font charset
- `CFFExpertCharset.java` — Expert charset
- `CFFExpertEncoding.java` — Expert encoding
- `CFFExpertSubsetCharset.java` — Expert subset charset
- `CFFISOAdobeCharset.java` — ISO Adobe charset
- `CFFOperator.java` — Operator enum/type for charstring opcodes
- `CIDKeyedType2CharString.java` — CID-keyed Type 2 charstring
- `CharStringCommand.java` — Parsed charstring command
- `DataInput.java` — Low-level byte-stream interface for CFF parser
- `DataInputByteArray.java` — Byte-array backed DataInput
- `DataInputRandomAccessRead.java` — RandomAccessRead-backed DataInput
- `EmbeddedCharset.java` — Embedded CFF charset wrapper
- `FDSelect.java` — CFF FD-Select structure
- `Type1CharStringParser.java` — Type 1 charstring parser
- `Type2CharStringParser.java` — Type 2 charstring parser

**Impact:** The current `CFFParser.cs` is a focused adapted port; it lacks the full charstring interpreter infrastructure (`Type1CharStringParser`, `Type2CharStringParser`, `CFFOperator`, `CharStringCommand`) and the full charset/encoding coverage needed for CID-keyed fonts (`CFFCharsetCID`, `FDSelect`, `CFFExpertCharset`, `CFFExpertSubsetCharset`, `CFFISOAdobeCharset`).

### `org.apache.fontbox.ttf` root — 12/44 ported

**Ported (12):**
- `FontHeaders.cs` ← `FontHeaders.java`
- `HeaderTable.cs` ← `HeaderTable.java`
- `MaximumProfileTable.cs` ← `MaximumProfileTable.java`
- `MemoryTTFDataStream.cs` ← adapted from `RandomAccessReadDataStream.java`/`MemoryTTFDataStream.java`
- `NameRecord.cs` ← `NameRecord.java`
- `NamingTable.cs` ← `NamingTable.java`
- `OTFParser.cs` ← `OTFParser.java`
- `OpenTypeFont.cs` ← `OpenTypeFont.java`
- `TTFDataStream.cs` ← `TTFDataStream.java`
- `TTFParser.cs` ← `TTFParser.java`
- `TTFTable.cs` ← `TTFTable.java`
- `TrueTypeFont.cs` ← `TrueTypeFont.java`

**Missing from TTF root (32):**
- `CFFTable.java` — Inline CFF table in an OTF font
- `CmapLookup.java` — Cmap lookup interface
- `CmapSubtable.java` — Cmap platform/encoding subtable
- `CmapTable.java` — `cmap` TTF table (character-to-glyph mapping)
- `DigitalSignatureTable.java` — `DSIG` TTF table
- `GlyfCompositeComp.java` — Composite glyph component
- `GlyfCompositeDescript.java` — Composite glyph description
- `GlyfDescript.java` — Abstract glyph description
- `GlyfSimpleDescript.java` — Simple glyph description
- `GlyphData.java` — Glyph outline data
- `GlyphDescription.java` — Glyph description interface
- `GlyphRenderer.java` — Shape-based glyph renderer
- `GlyphSubstitutionTable.java` — `GSUB` TTF table
- `GlyphTable.java` — `glyf` TTF table
- `HorizontalHeaderTable.java` — `hhea` TTF table
- `HorizontalMetricsTable.java` — `hmtx` TTF table
- `IndexToLocationTable.java` — `loca` TTF table
- `KerningSubtable.java` — Kerning pair subtable
- `KerningTable.java` — `kern` TTF table
- `OS2WindowsMetricsTable.java` — `OS/2` TTF table
- `OTLTable.java` — OpenType layout table base
- `OpenTypeScript.java` — OpenType script registry enum
- `PostScriptTable.java` — `post` TTF table
- `RandomAccessReadDataStream.java` — RandomAccessRead-backed data stream
- `RandomAccessReadUnbufferedDataStream.java` — Unbuffered variant
- `SubstitutingCmapLookup.java` — GSUB-aware cmap lookup
- `TTCDataStream.java` — TrueType Collection data stream
- `TTFSubsetter.java` — Font subsetter (embeddable font creation)
- `TrueTypeCollection.java` — `.ttc` multi-font container parser
- `VerticalHeaderTable.java` — `vhea` TTF table
- `VerticalMetricsTable.java` — `vmtx` TTF table
- `VerticalOriginTable.java` — `VORG` TTF table
- `WGL4Names.java` — Windows Glyph List 4 (WGL4) name table

**Missing TTF subdirs (all 3 missing entirely):**

#### `org.apache.fontbox.ttf.gsub` — 0/13 ported
All 13 GSUB worker files:
- `CompoundCharacterTokenizer.java`
- `DefaultGsubWorker.java`
- `GlyphArraySplitter.java`
- `GlyphArraySplitterRegexImpl.java`
- `GlyphSubstitutionDataExtractor.java`
- `GsubWorker.java`
- `GsubWorkerFactory.java`
- `GsubWorkerForBengali.java`
- `GsubWorkerForDevanagari.java`
- `GsubWorkerForDflt.java`
- `GsubWorkerForGujarati.java`
- `GsubWorkerForLatin.java`
- `GsubWorkerForTamil.java`

#### `org.apache.fontbox.ttf.model` — 0/5 ported
- `GsubData.java`
- `Language.java`
- `MapBackedGsubData.java`
- `MapBackedScriptFeature.java`
- `ScriptFeature.java`

#### `org.apache.fontbox.ttf.table.common` — 0/12 ported
- `CoverageTable.java`
- `CoverageTableFormat1.java`
- `CoverageTableFormat2.java`
- `FeatureListTable.java`
- `FeatureRecord.java`
- `FeatureTable.java`
- `LangSysTable.java`
- `LookupListTable.java`
- `LookupSubTable.java`
- `LookupTable.java`
- `RangeRecord.java`
- `ScriptTable.java`

#### `org.apache.fontbox.ttf.table.gsub` — 0/9 ported
- `AlternateSetTable.java`
- `LigatureSetTable.java`
- `LigatureTable.java`
- `LookupTypeAlternateSubstitutionFormat1.java`
- `LookupTypeLigatureSubstitutionSubstFormat1.java`
- `LookupTypeMultipleSubstitutionFormat1.java`
- `LookupTypeSingleSubstFormat1.java`
- `LookupTypeSingleSubstFormat2.java`
- `SequenceTable.java`

---

## Packages not yet started ❌

### `org.apache.fontbox.afm` — 0/8 ported
Adobe Font Metrics (AFM) parser and data model. Used by Type1 font metrics loading.
- `AFMParser.java` — AFM file format parser
- `CharMetric.java` — Per-character metrics (width, bbox, ligatures, kern pairs)
- `Composite.java` — Composite character entry
- `CompositePart.java` — Part of a composite character
- `FontMetrics.java` — Top-level AFM font metrics model
- `KernPair.java` — Kerning pair entry
- `Ligature.java` — Ligature substitution entry
- `TrackKern.java` — Track kerning entry

### `org.apache.fontbox.cmap` — 0/5 ported
CMap (Character Map) parser for Type0/CIDFont composite fonts. Required for CJK and non-Latin PDF text extraction.
- `CIDRange.java` — CID mapping range
- `CMap.java` — CMap data model
- `CMapParser.java` — CMap file/stream parser
- `CMapStrings.java` — CMap-backed string utilities
- `CodespaceRange.java` — Codespace range entry

---

## Key gaps by priority

### Priority 1 — CFF charstring infrastructure (blocks Type1/CFF rendering)
Missing: `CFFOperator`, `CharStringCommand`, `Type1CharStringParser`, `Type2CharStringParser`,
`DataInput`, `DataInputByteArray`, `DataInputRandomAccessRead`

These are required for interpreting glyph outlines in Type1 and CFF fonts. Without them, path
construction for glyph rendering and width computation is incomplete.

### Priority 2 — CFF charset/encoding coverage (blocks CID fonts)
Missing: `CFFCharsetCID`, `FDSelect`, `CFFExpertCharset`, `CFFExpertSubsetCharset`,
`CFFISOAdobeCharset`, `EmbeddedCharset`, `CIDKeyedType2CharString`, `CFFExpertEncoding`

Required for CID-keyed font support (used for CJK and large Unicode fonts).

### Priority 3 — TTF core table stack (blocks real TrueType glyph access)
Missing: `CmapTable`/`CmapSubtable`/`CmapLookup`, `GlyphTable`/`GlyphData`/`GlyfDescript*`,
`HorizontalHeaderTable`, `HorizontalMetricsTable`, `IndexToLocationTable`,
`OS2WindowsMetricsTable`, `PostScriptTable`, `KerningTable`/`KerningSubtable`

The current TTF port can parse the font header and naming tables. Core glyph access requires
the cmap (character-to-glyph) and glyf (outline data) tables.

### Priority 4 — CMap package (blocks composite/CJK font support)
All 5 files in `org.apache.fontbox.cmap`. Required for PDF CMap stream parsing and CJK text
extraction. These are referenced from `PDModel.Font` when processing Type0/CIDFont streams.

### Priority 5 — AFM package (blocks Type1 font metrics)
All 8 files in `org.apache.fontbox.afm`. Adobe Font Metrics (.afm) files provide character
widths and kerning for Type1 fonts embedded in PDFs.

### Priority 6 — TTF GSUB + OpenType layout tables (blocks complex script text)
The `ttf/gsub`, `ttf/model`, `ttf/table/common`, and `ttf/table/gsub` packages (39 files total).
Required for rendering scripts needing glyph substitution (Bengali, Devanagari, Gujarati,
Tamil, Latin ligatures, etc.).

### Priority 7 — TTF extended tables (blocks full font support)
Remaining TTF root files: `TrueTypeCollection`, `TTFSubsetter`, `VerticalHeaderTable`,
`VerticalMetricsTable`, `VerticalOriginTable`, `WGL4Names`, `TTCDataStream`,
`DigitalSignatureTable`, `OTLTable`, `GlyphSubstitutionTable`, `OpenTypeScript`.

---

## Planned work breakdown

The following issues cover the remaining work (dependency order):

| Issue | Scope | Files | Priority |
|---|---|---|---|
| #8 (update) | Complete `fontbox/cff/**` charstring + charset gaps | 15 | 1, 2 |
| #9a | Core `fontbox/ttf/**` cmap + glyf table stack | ~14 | 3 |
| #12 | Port `fontbox/afm/**` | 8 | 5 |
| #13 | Port `fontbox/cmap/**` | 5 | 4 |
| #9b | Remaining `fontbox/ttf/**` core tables | ~18 | 7 |
| #9c | `fontbox/ttf/table/common` + `fontbox/ttf/table/gsub` | 21 | 6 |
| #9d | `fontbox/ttf/model` + `fontbox/ttf/gsub` workers | 18 | 6 |
| #10 | Replace `PDModel.Font` stubs with FontBox-backed types | — | after above |
| #11 | Dedicated `PdfBox.Net.FontBox` project split | — | last |

See individual issue files under `issues/` for detailed scope and exit criteria.
