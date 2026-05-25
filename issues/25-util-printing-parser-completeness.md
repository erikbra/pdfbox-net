### Title
Port remaining `pdfbox/util` classes, `printing` module, and complete `pdfparser`/`pdfwriter`

### Depends on
- #19 filter implementations (Flate needed for real PDF loading)
- Chunk 2/3 parser/writer baseline
- COSDocument (document object graph) — see notes below

### Background
Status refresh (2026-05-25): much of this issue has already been completed in later slices.

**`pdfbox/util`**: partially complete. Core classes used by parser/writer paths are ported
(`DateConverter`, `Hex`, `Matrix`, `NumberFormatUtil`, `StringUtil`, `Vector`) and covered by
tests. Remaining direct upstream `org.apache.pdfbox.util` files to consider are now:
`IterativeMergeSort.java`, `Version.java`, and `XMLUtil.java`.

**`pdfbox/printing`**: complete for current parity target. All four upstream files are ported:
`Orientation`, `PDFPrintable`, `PDFPageable`, and `Scaling`, with `PrintingTest` coverage.

**`pdfbox/pdfparser`**: `PDFDocumentParser` and `PDFObjectStreamParser` are already ported and
wired into the document load pipeline with fixture-backed tests.

**`pdfbox/pdfwriter`**: `CompressParameters` is ported; `COSWriterXRefEntry` remains relevant
for incremental-save parity.

### Scope (remaining relevance)

**Still relevant:**
- `pdfbox/util` parity backfill for:
  - `IterativeMergeSort.java`
  - `Version.java`
  - `XMLUtil.java`
- `pdfbox/pdfwriter` parity backfill for:
  - `COSWriterXRefEntry.java`

**No longer relevant (already done in-repo):**
- `pdfbox/printing` class ports (`Orientation`, `PDFPrintable`, `PDFPageable`, `Scaling`)
- `pdfbox/pdfparser` ports (`PDFDocumentParser`, `PDFObjectStreamParser`) for current target
- `pdfbox/util` baseline classes (`DateConverter`, `Hex`, `NumberFormatUtil`, `StringUtil`)

### Expected test scope
- Already present:
  - `tests/PdfBox.Net.Tests/DateConverterTest.cs`
  - `tests/PdfBox.Net.Tests/HexTest.cs`
  - `tests/PdfBox.Net.Tests/PrintingTest.cs`
  - parser/object-stream coverage (`PDFDocumentParserTest`, `PDFParserXrefStreamObjectStreamTest`,
    `PDDocumentLoadSaveRoundtripTest`)
- Remaining test additions should be tied only to newly ported `IterativeMergeSort`,
  `Version`, `XMLUtil`, and `COSWriterXRefEntry`.

### Entry criteria
- #19 filter implementations landed (PDFDocumentParser needs FlateFilter for xref streams)
- Chunk 2/3 parser/writer baseline stable
- `dotnet build` and `dotnet test` green

### Exit criteria
- `IterativeMergeSort`, `Version`, and `XMLUtil` parity decision implemented (ported or
  explicitly deferred with rationale)
- `COSWriterXRefEntry` parity decision implemented (ported or explicitly deferred with rationale)
- Any newly ported files include provenance/doc comments consistent with existing style
- `reports/conversion-records.json` and traceability updated for any new mappings
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
- First PR: `util` package remaining parity (`IterativeMergeSort`, `Version`, `XMLUtil`)
- Second PR: `pdfwriter` remaining parity (`COSWriterXRefEntry`)

### Definition of done
- `dotnet build` passes
- `dotnet test` passes
- Remaining relevant classes in this issue are either ported or explicitly deferred
- Provenance headers and doc comments are maintained on any new ports
- Conversion/normalization/traceability records updated accordingly
