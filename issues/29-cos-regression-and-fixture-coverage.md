### Title
Add fixture-driven COS regression coverage

### Depends on
- #28 COS visitors/serialization complete

### Scope
- Add focused fixture-based regression tests for COS behavior edge cases.
- Validate stability on malformed/minimal object structures and roundtrip scenarios.
- Keep scope limited to COS-layer behavior.

### Expected test scope
- Add deterministic fixture tests under `tests/PdfBox.Net.Tests` for COS-only workflows.
- Ensure existing COS tests remain green.

### Exit criteria
- COS regression suite covers key edge cases from conversion slices.
- Test execution is deterministic in CI.
- Reports are updated for any newly ported tests/mappings.
