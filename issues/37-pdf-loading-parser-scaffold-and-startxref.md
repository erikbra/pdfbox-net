### Title
Port PDF loading parser scaffold and startxref/header flow

### Goal
Establish the first executable slice of full PDF loading by porting parser entry scaffolding and reliable header + `startxref` discovery.

### Depends on
- COS foundation milestone (#26-#30)
- Existing parser support classes already ported under `pdfparser`

### Scope
- Port parser entry orchestration from:
  - `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFParser.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFDocumentParser.java`
- Implement:
  - PDF header/version validation (`%PDF-`)
  - `startxref` lookup from file tail
  - bootstrap parser state needed for xref/trailer read in next slice
- Keep xref/object graph resolution out of scope for this issue.

### Expected test scope
- Fixture tests for valid/invalid PDF header handling.
- `startxref` location tests on deterministic sample PDFs.

### Entry criteria
- `dotnet build` and `dotnet test` baseline are green.

### Exit criteria
- Parser can open a stream and locate header + `startxref` deterministically.
- Baseline tests remain green with new targeted parser tests.

### Risk register
- Stream seek behavior differences between Java and .NET when scanning file tails.
- Off-by-one handling around EOL and whitespace near `startxref`.

### Definition of done
- `dotnet build` passes.
- New parser scaffold tests pass.
- Traceability/conversion records updated for touched classes.
