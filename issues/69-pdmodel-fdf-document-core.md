### Title
Port PDModel/FDF document core

### Depends on
- #60 filter/parser/writer completeness
- #68 interactive closeout for shared annotation/form concepts

### Background
`pdmodel.fdf` is currently zero-coverage. The document/core model should land first so later field,
page, template, and annotation mirror types build on a stable base.

### Scope
- Port the FDF document core types:
  - `FDFDocument`
  - `FDFCatalog`
  - `FDFDictionary`
  - `FDFJavaScript`
  - `FDFNamedPageReference`
- Establish parser/load/save integration needed for FDF model construction.

### Expected test scope
- FDF document dictionary/property tests.
- Fixture-backed load/save smoke tests for basic FDF documents.

### Entry criteria
- Parser/writer completeness merged and green.

### Exit criteria
- FDF core document model is mapped and regression tested.

### Risk register
- FDF parsing can expose parser assumptions not covered by standard PDF fixtures.

### Definition of done
- `dotnet build` passes.
- Targeted FDF core tests pass.
- Traceability artifacts are updated.
