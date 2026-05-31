### Title
Close PDPage tree + rectangle semantic parity debt

### Depends on
- #101 PDDocument lifecycle + catalog/info semantic parity closeout

### Background
After the document-lifecycle slice, the remaining parity lock blockers should be concentrated in page traversal and geometry behavior. This issue closes the last page/tree/rectangle rows before the final lock rerun.

### Scope
- Resolve non-`in-sync` rows tied to:
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageTree.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java`
- Reconcile semantic drift in page traversal, inherited attributes, and rectangle/box behavior.
- Update conversion/normalization/traceability records for touched rows.

### Expected test scope
- Targeted PDPage, PDPageTree, and PDRectangle regression tests.

### Exit criteria
- All scoped rows in this slice are `in-sync`.
- No regressions remain in touched page/tree/geometry behavior.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted PDModel tests pass.
- Canonical parity reports regenerated and checked in.
