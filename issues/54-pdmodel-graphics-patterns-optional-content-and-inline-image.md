### Title
Complete PDModel/graphics patterns, optional-content, and inline-image types

### Background
After shading core is in place, remaining type-level gaps in patterns, optional-content, and
inline-image handling must be closed to make graphics resources structurally complete.

### Depends on
- #53 PDModel/graphics shading and core types

### Scope
- Complete `PDTilingPattern` and `PDShadingPattern` parity path for current milestone scope.
- Replace remaining optional-content placeholders with real `PDOptionalContent*` behavior.
- Complete `PDInlineImage` data-model and dictionary handling used by content-stream execution.
- Align resource dictionary integrations for these types.

### Expected test scope
- Pattern dictionary/property roundtrip tests.
- Optional-content dictionary and layer lookup tests.
- Inline-image dictionary + byte payload parsing tests.

### Entry criteria
- #53 complete and green.

### Exit criteria
- Pattern/optional-content/inline-image types are no longer stub-backed in touched paths.
- Tests protect dictionary parsing and serialization behavior.

### Risk register
- Pattern resources and optional-content can have cross-dictionary references.
- Inline-image parsing is sensitive to stream/operator boundary handling.

### Definition of done
- Build + targeted graphics tests pass.
- Report artifacts updated for all touched classes/tests.
