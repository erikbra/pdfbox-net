### Title
Complete TrueType + CIDFontType2 parity in PDModel/font

### Background
TrueType-backed fonts are required for broad real-world PDF compatibility. This slice
focuses on `PDTrueTypeFont` and `PDCIDFontType2` paths using the already ported FontBox TTF stack.

### Depends on
- #48 PDModel/font core foundation
- #49 Type 1 + Standard 14 parity

### Scope
- Complete parity behavior for `PDTrueTypeFont` and `PDCIDFontType2`.
- Ensure CID/type2 width and Unicode mapping behavior aligns with upstream expectations.
- Close any remaining integration gaps between PDModel font wrappers and FontBox TrueType data.
- Keep unsupported vertical/advanced edge-cases explicitly tracked if deferred.

### Expected test scope
- Deterministic TrueType fixture tests for code-to-Unicode and width extraction.
- CID/type2 regression checks for descendant-font dictionary handling.

### Entry criteria
- #48 and #49 complete and green.

### Exit criteria
- TrueType and CIDType2 paths are stable for baseline extraction/use cases.
- Tests cover key mapping + width behaviors and prevent regressions.

### Risk register
- CID/ToUnicode interactions can vary across embedded font variants.
- Width fallback logic can drift if descriptor/table precedence is not aligned.

### Definition of done
- Build + targeted tests pass.
- Report artifacts updated for all touched classes/tests.
