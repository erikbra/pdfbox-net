### Title
Close PDModel document-pipeline semantic parity debt (`PDDocument*`, `PDPage*`, `PDRectangle`)

### Depends on
- #97 parser/contentstream semantic parity closeout

### Background
After parser stabilization, the next largest coupled area is PDModel document lifecycle/page traversal behavior. These rows are currently `partially-in-sync` and block parity lock.

### Scope
- Resolve non-`in-sync` rows tied to:
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocument.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentCatalog.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentInformation.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageTree.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java`
- Confirm document/page interactions remain aligned with upstream Java behavior.
- Update conversion/normalization/traceability records for touched rows.

### Expected test scope
- Targeted PDModel document/page lifecycle and parser-integration tests.

### Exit criteria
- All scoped rows in this slice are `in-sync`.
- PDModel document/page regression tests pass for touched behavior.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted PDModel tests pass.
- Canonical parity reports regenerated and checked in.
