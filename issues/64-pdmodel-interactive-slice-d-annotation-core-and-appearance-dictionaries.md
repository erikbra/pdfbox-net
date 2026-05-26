### Title
Port PDModel interactive slice D: annotation core, factories, and appearance dictionaries

### Depends on
- #63 interactive slice C

### Background
Annotation model types are one of the largest remaining interactive gaps. Core data-model and
factory coverage must land before appearance handlers and form generation.

### Scope
- Port annotation base/factory paths and remaining subtype dictionaries.
- Port appearance-related supporting dictionaries and entries:
  - appearance dictionary, entry, stream, characteristics
  - border style/effect dictionaries
  - external data dictionaries
- Cover the remaining annotation subtypes required for structural parity before handler logic.

### Expected test scope
- Annotation-factory dispatch tests.
- Dictionary/property tests for link, text, markup, popup, polygon/polyline, and widget cases.

### Entry criteria
- Slice C merged and green.

### Exit criteria
- Annotation core and supporting appearance dictionaries are ported.
- Factory-driven annotation construction is deterministic for covered subtype fixtures.

### Risk register
- Widget annotations cross over into incomplete AcroForm behavior.

### Definition of done
- `dotnet build` passes.
- Annotation-core tests pass.
- Traceability artifacts are updated.
