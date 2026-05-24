### Title
Port remaining `pdfbox/util` classes, `printing` module, and complete `pdfparser`/`pdfwriter`

### Depends on
- #19 filter implementations (Flate needed for real PDF loading)
- Chunk 2/3 parser/writer baseline
- COSDocument (document object graph) — see notes below

### Background
Several supporting modules are partially or not yet ported:

**`pdfbox/util`**: Only `Vector`, `Matrix`, and `AffineTransform` are ported (~20% complete).
The remaining utility classes cover date handling, hex encoding, number formatting, and other
helpers required throughout the codebase.

**`pdfbox/printing`**: Not started at all. The printing module bridges PDFBox to the OS print
subsystem — required for any application that needs to print PDF pages.

**`pdfbox/pdfparser`**: ~80% complete; missing `PDFDocumentParser` (top-level PDF loader that
produces a live `PDDocument`) and `COSDocument` (the in-memory PDF object graph container).
Without these, the full document open/save pipeline depends on indirect wiring.

**`pdfbox/pdfwriter`**: ~60% complete; missing `COSWriterXRefEntry` for incremental writes and
a robust xref stream writer.

### Scope

**`pdfbox/util`** (~12 classes):
- `DateConverter.java` — PDF date string ↔ `DateTime` parsing/formatting
- `Hex.java` — hex encode/decode utilities
- `NumberFormatUtil.java` — number formatting for PDF serialization
- `SmallMap.java` — small-map optimized container
- `IteratorChain.java` — iterable chain utility
- `CharUtils.java` — character classification utilities
- `NetworkUtil.java` — URL/network utilities (if applicable to .NET context)
- `StringUtil.java` — string helpers
- Any remaining small utilities referenced from other ported classes

**`pdfbox/pdfparser` completion** (~3 classes):
- `COSDocument.java` — the in-memory PDF object graph (object table, header version, trailer);
  bridges the output of COSParser to the PDDocument constructor
- `PDFDocumentParser.java` — orchestrates COSParser to produce a fully-loaded COSDocument and
  PDDocument (the primary entry point for `PDDocument.load(...)`)
- `PDFObjectStreamParser.java` — decodes ObjStm (object stream) entries

**`pdfbox/pdfwriter` completion** (~2 classes):
- `COSWriterXRefEntry.java` — xref entry for incremental update writes
- `CompressParameters.java` — compression parameters for object-stream writes

**`pdfbox/printing`** (~4 classes):
- `PDFPrintable.java` — `Printable` (Java)/`PrintDocument` (.NET) adapter for a single PDF page
- `PDFPageable.java` — `Pageable` (Java)/print-document adapter for all pages
- `PrintOrientation.java` — orientation enum (auto, landscape, portrait)
- `Scaling.java` — scaling mode enum (actual size, fit, stretch, shrink to fit)

For .NET printing adaptation:
- Use `System.Drawing.Printing.PrintDocument` + `PrintPage` event as the nearest .NET equivalent
- Map `PDFPrintable` → a `PrintDocument` page renderer
- Map `PDFPageable` → an `IEnumerator<PageSettings>` or equivalent multi-page controller

### Expected test scope
- Add `tests/PdfBox.Net.Tests/DateConverterTest.cs` with PDF date string parsing edge cases
- Add `tests/PdfBox.Net.Tests/HexUtilTest.cs` for encode/decode roundtrips
- Extend `tests/PdfBox.Net.Tests/ParserWriterRoundtripTest.cs` to cover object stream loading
  via PDFDocumentParser
- Add `tests/PdfBox.Net.Tests/PrintingTest.cs` (compile + basic instantiation only; skip render
  assertions on headless CI)

### Entry criteria
- #19 filter implementations landed (PDFDocumentParser needs FlateFilter for xref streams)
- Chunk 2/3 parser/writer baseline stable
- `dotnet build` and `dotnet test` green

### Exit criteria
- `DateConverter` correctly parses and formats all standard PDF date formats
- `Hex` encode/decode roundtrip tests pass
- `COSDocument` holds a functional in-memory object table
- `PDFDocumentParser` successfully loads a minimal unencrypted fixture PDF
- `COSWriterXRefEntry` supports incremental xref table writes
- `PDFPrintable` and `PDFPageable` compile and can be instantiated
- `reports/conversion-records.json` and traceability updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- `PDFDocumentParser` is large and complex; split into sequential sub-PRs rather than a single
  big PR
- .NET `System.Drawing.Printing` is legacy on non-Windows; consider abstracting the print output
  behind an interface and providing the Windows implementation only for now
- `NumberFormatUtil` formatting semantics must match Java's `float`/`double` PDF output exactly
  to avoid COSWriter output divergence; add precise roundtrip tests
- `COSDocument` owns the cross-reference table and object dereference — needs careful thread
  safety analysis; mark any mutable shared state clearly

### PR slicing rule
- First PR: `util` package — `DateConverter` + `Hex` + `NumberFormatUtil` + `CharUtils` +
  remaining small utilities
- Second PR: `pdfparser` — `COSDocument` (object table, header, trailer)
- Third PR: `pdfparser` — `PDFDocumentParser` (load orchestration)
- Fourth PR: `pdfparser` — `PDFObjectStreamParser`
- Fifth PR: `pdfwriter` — `COSWriterXRefEntry` + `CompressParameters`
- Sixth PR: `printing` — all 4 printing classes

### Definition of done
- `dotnet build` passes
- PDF date string parse/format tests pass
- `PDFDocumentParser` loads a minimal fixture PDF to a live PDDocument
- `PDFPrintable` and `PDFPageable` compile
- `COSDocument` object table accessible
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~21 files, estimated 3–4 engineer-days
