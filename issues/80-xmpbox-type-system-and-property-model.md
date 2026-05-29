### Title
Port XmpBox slice 3: type system and property model parity

### Depends on
- #79 schema layer parity

### Background
The XMP type and property model is required for complete schema behavior and must be
closed before final regression/traceability closeout.

### Scope
- Port type/property classes under `org/apache/xmpbox/type/**`.
- Close normalization differences for value conversions and structured properties.
- Add tests covering primitive, structured, and array property behavior.

### Expected test scope
- Type conversion and validation tests.
- Property model roundtrip tests across representative metadata packets.

### Entry criteria
- Slice #79 merged and green.

### Exit criteria
- Type/property missing gaps are closed for touched paths.
- Remaining XmpBox work is narrowed to integration/regression closeout only.

### Risk register
- Value normalization mismatches can produce non-obvious parity drift.

### Definition of done
- `dotnet build` passes.
- Targeted XmpBox type/property tests pass.
- Conversion/normalization/traceability artifacts are updated.
