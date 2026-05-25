### Title
Complete Type 1 + Standard 14 font parity path in PDModel/font

### Background
After core font foundation work, the first concrete parity slice should close Type 1 and
Standard 14 behavior. This is a high-impact path for text extraction and default-font usage.

### Depends on
- #48 PDModel/font core foundation

### Scope
- Complete parity behavior for `PDType1Font` and related Type 1 code paths.
- Harden `Standard14Fonts` mappings and lookup behavior against upstream expectations.
- Ensure encoding + glyph name lookup interactions are aligned for Type 1 workflows.
- Wire any remaining factory/descriptor integration needed specifically for Type 1 paths.

### Expected test scope
- Standard 14 mapping matrix tests (all built-in names and aliases used by upstream behavior).
- Type 1 width/encoding/Unicode mapping assertions on deterministic fixtures.

### Entry criteria
- #48 complete and merged.

### Exit criteria
- `PDType1Font` behavior is usable for extraction/layout baseline scenarios.
- Standard 14 mappings are regression tested and stable.
- No unresolved compile placeholders remain in touched Type 1 paths.

### Risk register
- Built-in font metric/encoding edge cases can diverge subtly from Java behavior.
- Alias handling for Standard 14 names can regress compatibility if incomplete.

### Definition of done
- Build + targeted font tests pass.
- Conversion/normalization/traceability reports updated for all introduced mappings.
