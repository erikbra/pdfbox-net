### Title
Complete PDModel/graphics shading core and base type parity

### Background
With font milestone closeout nearly complete, `org.apache.pdfbox.pdmodel.graphics` is the
next largest dependency-safe chunk. The highest-risk remaining area is the shading/function
core (`PDShading` hierarchy and related base wiring), which blocks full graphics fidelity.

### Depends on
- #51 Type0/CIDType0 and Unicode integration
- #52 Font regression + traceability closeout
- Existing color-space baseline from #22

### Scope
- Complete/align `PDShading` base behavior and remaining `PDShadingType1..7` implementations.
- Ensure shading dictionary parsing and common accessors align with upstream expectations.
- Wire function-driven shading dependencies to already-ported `pdmodel.common.function` classes.
- Keep advanced edge cases explicitly marked if deferred.

### Expected test scope
- Deterministic unit tests for each shading subtype dictionary parsing/construction path.
- Focused tests for function linkage and required-value validation.

### Entry criteria
- Font closeout (#51/#52) merged and green.

### Exit criteria
- `PDShading` hierarchy is compile-complete for current parity target.
- Core shading accessors/validation behavior is regression tested.

### Risk register
- Shading dictionaries have dense subtype-specific requirements and optional fields.
- Function evaluation parity can drift if dictionary defaults differ from upstream.

### Definition of done
- Build + targeted graphics tests pass.
- Conversion/normalization/traceability artifacts updated for touched mappings.
