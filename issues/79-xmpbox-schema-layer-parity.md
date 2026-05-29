### Title
Port XmpBox slice 2: schema layer parity

### Depends on
- #78 metadata entry points and XML pipeline

### Background
After parser/serializer foundations, schema classes can be ported in a dependency-safe
way to enable typed metadata operations.

### Scope
- Port remaining schema classes under `org/apache/xmpbox/schema/**`.
- Align schema registration and namespace behavior with the slice 1 pipeline.
- Add schema-focused tests for creation, lookup, and roundtrip fidelity.

### Expected test scope
- Schema construction/registration tests.
- Fixture tests that assert schema-level roundtrip behavior.

### Entry criteria
- Slice #78 merged and green.

### Exit criteria
- Targeted schema coverage is in place and passing.
- Schema mappings are no longer tracked as missing for touched paths.

### Risk register
- Namespace/prefix collisions can introduce subtle serialization regressions.

### Definition of done
- `dotnet build` passes.
- Targeted XmpBox schema tests pass.
- Reports are updated for all touched source paths.
