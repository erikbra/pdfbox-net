### Title
Close out PDF loading milestone with fixtures, roundtrip regression, and report updates

### Goal
Stabilize and verify the full PDF loading milestone with regression fixtures and complete traceability/coverage reporting.

### Depends on
- #40 parser integration into `PDDocument.Load`

### Scope
- Add/expand fixture-based tests for:
  - classic xref table PDFs
  - xref stream PDFs
  - object stream PDFs
- Add load -> save (basic) roundtrip smoke checks for parser-integrated paths.
- Update reporting artifacts:
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
  - `reports/traceability-parity-report.json`
  - `reports/pdfbox-main-gap-analysis.md` coverage section for parser completion

### Expected test scope
- Deterministic fixture coverage for all parser pathways delivered in #37-#40.
- Regression assertions to prevent parser backslides.

### Entry criteria
- #40 merged and green.

### Exit criteria
- Parser milestone has repeatable fixture-based confidence.
- Coverage/traceability reports reflect completed parser slice.

### Risk register
- Fixture drift or nondeterministic data sources.
- False confidence if only happy-path fixtures are used.

### Definition of done
- `dotnet build` and relevant `dotnet test` scopes pass.
- Reporting artifacts are current and auditable for the parser milestone.
