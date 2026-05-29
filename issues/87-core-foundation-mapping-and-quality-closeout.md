### Title
Close core foundation mapping + paired quality debt (`contentstream`/`pdfparser`/`pdfwriter`/`filter`)

### Depends on
- Current canonical baseline in `reports/parity-execution-tracker.md`
- Existing parser/load pipeline slices #37-#41

### Background
This is the first active issue toward 100% conversion. It closes the remaining core foundation mapping gaps and burns down paired `partial` rows in the same pass.

### Scope
- Port remaining missing files for:
  - `contentstream`
  - `pdfparser`
  - `pdfwriter/compress`
  - `filter/Filter`
- Burn down paired `partial` rows for placeholder filters and loader/security paths to `in-sync`.
- Update traceability/conversion/normalization rows for touched paths.

### Expected test scope
- Targeted parser/filter/writer/contentstream tests.

### Exit criteria
- No remaining missing files in the scoped core foundation set.
- Touched traceability rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
