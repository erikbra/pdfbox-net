### Title
Replace `PDModel/Font` stubs with full font-type implementations

### Depends on
- FontBox port (substantially complete — all CFF, TTF, Type1, AFM, CMap packages ported)
- #19 filter implementations (font program streams use Flate, DCT, etc.)
- Chunk 2/3 parser and pdmodel baseline

### Background
The current `src/PdfBox.Net/PDModel/Font/FontStubs.cs` provides abstract class skeletons
(PDFont, PDSimpleFont, PDVectorFont, PDType0Font, PDTrueTypeFont, PDCIDFont, PDCIDFontType2,
PDFontDescriptor) with mostly empty or throw-based implementations. No concrete font type
(PDType1Font, PDTrueTypeFont, PDType0Font, etc.) is ported.

Without real font implementations:
- Character code to Unicode mapping returns null for all fonts
- Glyph widths return 0 for all fonts
- Text extraction cannot produce correct text, spacing, or layout results
- PDF creation cannot embed or use any font type

The FontBox layer (CFF parser, TTF parser, Type1 parser, AFM parser, CMap parser) is now
substantially complete and provides the backend that these PDModel font classes depend on.

### Scope
Port the following from `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/`:

**Core font hierarchy** (replace stubs with real implementations):
- `PDFont.java` — abstract base: widths array, ToUnicode CMap, font matrix, font descriptor
- `PDFontLike.java` — interface
- `PDFontDescriptor.java` — real metrics: ascent, descent, capHeight, stem widths, bounding box
- `PDSimpleFont.java` — abstract base for simple (non-composite) fonts
- `PDVectorFont.java` — interface for vector outline fonts

**Concrete font types**:
- `PDType1Font.java` — Type 1 (PostScript) fonts, including the 14 standard PDF fonts
- `PDTrueTypeFont.java` — TrueType font loading (uses `TTFParser` from FontBox)
- `PDType0Font.java` — Composite/Unicode fonts (Type 0 with descendant CIDFont)
- `PDCIDFont.java` — abstract CID font base
- `PDCIDFontType0.java` — CID font backed by Type1/CFF outlines
- `PDCIDFontType2.java` — CID font backed by TrueType (uses `TrueTypeFont` from FontBox)
- `PDType3Font.java` — user-defined Type 3 fonts

**Font encoding**:
- `encoding/DictionaryEncoding.java` — custom encoding from a PDF dictionary
- `encoding/MacRomanEncoding.java` — Mac Roman encoding
- `encoding/WinAnsiEncoding.java` — Windows ANSI (Latin-1) encoding
- `encoding/SymbolEncoding.java` — Adobe Symbol encoding
- `encoding/ZapfDingbatsEncoding.java` — Zapf Dingbats encoding
- `encoding/Type1Encoding.java` — Type 1 built-in encoding from font program

**Font support classes**:
- `Standard14Fonts.java` — mapping for the 14 standard built-in PDF fonts
- `FontMappers.java` / `FontMapper.java` — interface for system font lookup
- `DefaultFontProvider.java` — default system/classpath font provider
- `FileSystemFontProvider.java` — font discovery from file system
- `PDFontFactory.java` — factory for constructing PDFont from COSDictionary
- `PDCIDSystemInfo.java` — CID system information (Registry, Ordering, Supplement)
- `PDPanose.java` — PANOSE font classification data

### Expected test scope
- Extend `tests/PdfBox.Net.Tests/FontStubsReplacementTest.cs` with behavioral assertions
- Add width-array roundtrip tests for Type1 and TrueType fonts
- Add ToUnicode CMap decoding tests
- Test that Standard14Fonts maps all 14 standard font names correctly
- Add font descriptor metrics test

### Entry criteria
- FontBox TTF, CFF, Type1, AFM, CMap packages are substantially complete (already done)
- COS layer is stable (already done)
- Filter baseline (#19) landed so font programs in streams can be decoded

### Exit criteria
- `PDType1Font`, `PDTrueTypeFont`, `PDType0Font`, `PDCIDFontType0`, `PDCIDFontType2` are real
  implementations (not stubs)
- `PDFontDescriptor` returns real metrics from the font program
- `Standard14Fonts` maps all 14 standard font names
- `FontStubs.cs` is removed or reduced to genuinely deferred items
- Width lookups and ToUnicode mapping work for a Type1 font fixture
- `reports/conversion-records.json` and traceability updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- Standard 14 font metrics (AFM data) require embedded resource files; check if fontbox AFM
  resources can be reused directly or need to be re-embedded in the .NET project
- `PDType3Font` glyph rendering depends on ContentStream processing of Type 3 CharProcs; this can
  remain as a stub in the initial port (extraction path does not need full Type3 rendering)
- `FileSystemFontProvider` behavior differs between OS/X, Linux, Windows; test on all three platforms
- `PDCIDFontType2` with vertical writing requires `VerticalHeaderTable` / `VerticalMetricsTable`
  from FontBox TTF; those are already ported so this should be unblocked

### PR slicing rule
- First PR: `PDFont.java` + `PDFontDescriptor.java` + `Standard14Fonts.java` + encoding classes
  (the widths/descriptor layer without full font program loading)
- Second PR: `PDType1Font.java` using Type1/CFF FontBox backend
- Third PR: `PDTrueTypeFont.java` and `PDCIDFontType2.java` using TTF FontBox backend
- Fourth PR: `PDType0Font.java` + `PDCIDFont.java` + `PDCIDFontType0.java`
- Fifth PR: font provider/mapper, PDFontFactory, PDPanose, PDCIDSystemInfo

### Definition of done
- `dotnet build` passes
- Font width and ToUnicode tests pass for Type1 and TrueType fixtures
- Standard14Fonts maps all 14 fonts
- FontStubs.cs reduced/removed
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~27 files, estimated 4–6 engineer-days
