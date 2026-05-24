### Title
Port full PDF document loading pipeline (PDFParser / PDFDocumentParser)

### Background
The current `PDDocument.Load()` extracts only a raw dictionary string from the file bytes.
Real PDF loading requires:
1. Reading the `%%PDF-x.x` header
2. Locating and parsing the xref table (table-based or cross-reference stream)
3. Resolving indirect object references on-demand via xref
4. Decompressing object streams (PDF 1.5+ compressed xref)
5. Handling linearized PDFs
6. Building the complete in-memory `COSDocument` object graph
7. Wiring the result into `PDDocument` → `PDDocumentCatalog` → `PDPage` tree

Without this, the library cannot load actual PDFs from disk.

### Depends on
- COS layer (complete) — COSArray, COSDictionary, COSStream, COSObject
- Filter layer (FlateFilter, etc.) — for decompressing object and content streams
- XrefTrailerResolver (already ported)

### Scope
Port the following Java files:
- `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFParser.java` — top-level parser
  orchestrating xref reading, object resolution, decryption, and document construction
- `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFObjectStreamParser.java` — parses
  compressed object streams (ObjStm) used in cross-reference streams
- `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java` (Java version, full)
  — the existing C# COSParser is a simplified version; it needs to be extended to support
  cross-reference stream parsing and full document-level parsing

Update `PDDocument.Load()` to use the new parser rather than the simplified dictionary
extraction.

### Expected test scope
- Load a minimal uncompressed PDF (no filters) and verify page count and metadata.
- Load a real-world PDF with Flate-compressed content streams.
- Load a PDF with cross-reference streams (PDF 1.5+).
- Verify that `PDDocument.GetDocumentCatalog().GetPages()` returns the correct pages.
- Parser round-trip: load then save must produce a valid PDF.

### Entry criteria
- Existing baseline (`dotnet build`, `dotnet test`) is green (638 tests passing).
- COS and Filter layers complete (both are done).

### Exit criteria
- `PDDocument.Load(stream)` can load a real PDF file end-to-end.
- At least one fixture PDF is loaded successfully in tests.
- Provenance headers and conversion-records.json updated.

### Risk register
- Java cross-reference stream parsing logic is complex; linearized PDFs add another dimension.
- Encrypted PDF loading requires `StandardSecurityHandler.PrepareForDecryption` (issue #34).
- Cross-platform I/O and seeking semantics may require adaptation.

### Definition of done
- `dotnet build` passes.
- New PDF loading tests pass.
- Traceability and normalization records updated.
