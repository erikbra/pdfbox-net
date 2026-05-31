### Title
Close PDDocument lifecycle + catalog/info semantic parity debt

### Depends on
- #100 final traceability lock rerun

### Background
PR #191 / issue #100 confirmed that the parity lock is still blocked by 6 `partially-in-sync` PDModel rows. This slice tackles the document-lifecycle and document-metadata half of that remaining work.

### Scope
- Resolve non-`in-sync` rows tied to:
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocument.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentCatalog.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentInformation.java`
- Reconcile any remaining semantic drift in document lifecycle, catalog access, and document information behavior.
- Update conversion/normalization/traceability records for touched rows.

### Expected test scope
- Targeted PDDocument load/save, catalog, and document-information regression tests.

### Exit criteria
- All scoped rows in this slice are `in-sync`.
- No regressions remain in touched PDDocument/catalog/info behavior.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted PDModel tests pass.
- Canonical parity reports regenerated and checked in.
