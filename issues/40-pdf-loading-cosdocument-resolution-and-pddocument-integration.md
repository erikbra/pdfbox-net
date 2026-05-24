### Title
Integrate parser output into COSDocument resolution and PDDocument.Load

### Goal
Complete end-to-end document loading by wiring parser-resolved object graphs into `COSDocument` and `PDDocument`.

### Depends on
- #39 xref-stream/object-stream support
- Existing `PDDocument`, `PDDocumentCatalog`, and COS model types

### Scope
- Update `PDDocument.Load(...)` to use full parser flow instead of raw dictionary extraction.
- Ensure parser-produced `COSDocument` is connected to:
  - catalog resolution
  - page tree entry points
  - object dereference behavior required by existing PDModel code paths
- Keep decryption and advanced incremental save behavior out of scope.

### Expected test scope
- End-to-end load of deterministic fixture PDFs.
- Assertions on catalog existence, page tree traversal, and page count.

### Entry criteria
- #39 merged and green.

### Exit criteria
- `PDDocument.Load(stream)` can open real fixture PDFs end-to-end.
- Core document navigation APIs work on loaded documents.

### Risk register
- Object lifetime/ownership differences (`IDisposable`) vs Java semantics.
- Lazy object dereference behavior mismatches.

### Definition of done
- `dotnet build` passes.
- End-to-end document load tests pass.
- Conversion/traceability rows updated.
