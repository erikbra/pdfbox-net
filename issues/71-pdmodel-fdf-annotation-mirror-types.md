### Title
Port PDModel/FDF annotation mirror types

### Depends on
- #70 FDF field, page, and template model
- #68 interactive closeout

### Background
The largest remaining FDF volume is the annotation mirror hierarchy, which should be handled after
interactive annotation parity is already stable.

### Scope
- Port the FDF annotation base and remaining subtype mirror classes.
- Reuse completed interactive annotation concepts where safe, but keep FDF-specific dictionary
  behavior explicit.

### Expected test scope
- FDF annotation-factory and subtype property tests.
- Regression tests for representative highlight, text, link, square/circle, line, and markup cases.

### Entry criteria
- #70 merged and green.

### Exit criteria
- FDF annotation mirror types are mapped for the current parity target.

### Risk register
- Annotation subtype breadth can create wide test-fixture requirements.

### Definition of done
- `dotnet build` passes.
- Targeted FDF annotation tests pass.
- Traceability artifacts are updated.
