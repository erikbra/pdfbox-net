### Title
Port PDModel interactive slice F: annotation appearance handlers and appearance generation

### Depends on
- #65 interactive slice E
- #57 graphics closeout

### Background
Once annotation core and AcroForm structure are in place, appearance generation can be ported
without relying on large placeholder regions.

### Scope
- Port annotation appearance handler infrastructure and subtype handlers.
- Port form/annotation appearance generation helpers needed by widget and markup paths.
- Align generated appearance streams with the completed graphics/content-stream stack.

### Expected test scope
- Fixture-backed appearance-generation tests for representative annotation and widget cases.
- Regression tests ensuring generated appearance streams are structurally valid.

### Entry criteria
- Slice E merged and green.

### Exit criteria
- Appearance handlers are no longer zero-coverage.
- Covered annotation/widget types can generate or consume appearance streams deterministically.

### Risk register
- Appearance generation crosses graphics, forms, and content-stream execution layers.

### Definition of done
- `dotnet build` passes.
- Appearance-generation tests pass.
- Traceability artifacts are updated.
