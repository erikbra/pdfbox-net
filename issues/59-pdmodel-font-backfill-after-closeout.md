### Title
Backfill remaining PDModel/font files after issues #51-#52

### Depends on
- #51 Type0/CIDType0 and Unicode integration
- #52 Font regression coverage and traceability closeout

### Background
The current font milestone closes the main functional path, but the refreshed gap analysis still
shows unmapped supporting types in `pdmodel.font` that block the 51/51 target.

### Scope
- Port the remaining font support and backfill types still missing from the scan, including:
  - mapper/provider/cache classes
  - embedder/subsetter helpers
  - `PDType1CFont`, `PDMMType1Font`, `PDType3Font`, `PDType3CharProc`
  - `ToUnicodeWriter`, `UniUtil`
  - remaining encoding classes and related support types
- Preserve the completed font regression baseline from #52 while expanding mappings to full
  package coverage.

### Expected test scope
- Fixture-backed tests for Type1C/Type3/composite-font support where behavior is newly exposed.
- Regression coverage for embedder/subsetter helpers that affect save paths.

### Entry criteria
- #51 and #52 merged and green.

### Exit criteria
- `pdmodel.font` reaches 51 / 51 mapped for the current upstream baseline.
- Font support types no longer depend on placeholder/stub behavior in touched paths.
- Reports classify every touched font mapping explicitly.

### Risk register
- Type3 and embedder paths can fan out into content-stream and writer behavior.
- System-font mapper/provider behavior may differ across OS platforms.

### PR slicing rule
- Split by support cluster (provider/cache, Type1C/Type3, embedders/subsetting), but keep each
  PR tied to a runnable regression slice.

### Definition of done
- `dotnet build` passes.
- Font-focused targeted tests pass.
- Traceability and gap-analysis artifacts reflect full `pdmodel.font` coverage.
