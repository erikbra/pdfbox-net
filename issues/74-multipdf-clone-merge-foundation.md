### Title
Port multipdf clone and merge foundation

### Depends on
- #60 filter/parser/writer completeness
- Stable document load/save pipeline

### Background
`multipdf` is zero-coverage, but its dependency-safe starting point is clone/merge infrastructure.

### Scope
- Port:
  - `PDFCloneUtility`
  - `PDFMergerUtility`
- Align cloned object handling with completed COS/parser/writer ownership rules.

### Expected test scope
- Merge and clone regression tests with small deterministic fixtures.

### Entry criteria
- Parser/writer completeness merged and green.

### Exit criteria
- Clone and merge foundation classes are mapped and fixture tested.

### Risk register
- Object cloning can expose indirect object ownership and xref edge cases.

### Definition of done
- `dotnet build` passes.
- Multipdf clone/merge tests pass.
- Traceability artifacts are updated.
