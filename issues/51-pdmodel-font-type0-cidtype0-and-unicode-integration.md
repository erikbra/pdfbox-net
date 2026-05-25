### Title
Complete Type0/CIDType0 integration and Unicode mapping parity

### Background
Composite font support (`PDType0Font` + CID descendants) is required to move text extraction,
rendering, and interactive paths beyond basic Latin-only coverage.

### Depends on
- #48 PDModel/font core foundation
- #50 TrueType + CIDType2 parity

### Scope
- Complete `PDType0Font` integration with descendant CID font handling.
- Complete/align `PDCIDFont` and `PDCIDFontType0` behavior for current parity target.
- Harden ToUnicode/CMap-driven mapping path for composite fonts.
- Ensure `PDFontFactory` routing covers composite dictionaries deterministically.

### Expected test scope
- Composite-font fixture tests validating descendant resolution and Unicode mapping.
- Regression checks for common Type0 CMap paths used by existing text tests.

### Entry criteria
- #48 and #50 complete.

### Exit criteria
- Type0/CIDType0 paths are functional for baseline composite-font extraction scenarios.
- No unresolved routing gaps remain in factory construction for covered composite dictionaries.

### Risk register
- CMap/descendant resolution has high edge-case density across PDFs.
- Composite fallback behavior may expose subtle differences from Java stream/COS semantics.

### Definition of done
- Build + targeted tests pass.
- Conversion/traceability records updated with explicit adaptation notes where needed.
