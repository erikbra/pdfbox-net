### Title
Port multipdf page extraction and splitting

### Depends on
- #74 multipdf clone and merge foundation

### Background
Once clone/merge infrastructure is in place, page extraction and splitting can build on the same
document-copy behavior.

### Scope
- Port:
  - `PageExtractor`
  - `Splitter`
- Keep behavior aligned with completed document and page-tree semantics.

### Expected test scope
- Page extraction and split-result tests using deterministic fixture PDFs.

### Entry criteria
- #74 merged and green.

### Exit criteria
- Extraction and splitting work for baseline fixture scenarios.

### Risk register
- Split behavior can expose inherited-resource and bookmark/page-label edge cases.

### Definition of done
- `dotnet build` passes.
- Multipdf extraction/splitting tests pass.
- Traceability artifacts are updated.
