### Title
Close out documentinterchange milestone with regression fixtures and report completion

### Background
The final milestone step is to lock in the full `documentinterchange` conversion
with deterministic regression fixtures and complete reporting hygiene.

### Depends on
- Issues #43–#46

### Scope
- Add/expand tagged PDF fixture coverage for the full documentinterchange read path.
- Classify and update remaining documentinterchange traceability statuses.
- Ensure conversion/normalization/traceability artifacts are complete and auditable.

### Expected test scope
- Regression suite covering structure tree, references, attributes, and parent-tree lookups.
- Roundtrip smoke checks where write-path behavior is implemented.

### Entry criteria
- Issues #43–#46 merged.

### Exit criteria
- Documentinterchange package is functionally complete for current parity target.
- Traceability rows in this package have explicit statuses (no blanks).
- Gap analysis is updated with revised package completion percentage.

### Risk register
- Fixture scarcity for edge-case tagged PDFs.
- Late closeout may reveal previously hidden partial implementations.

### Definition of done
- Build + test suite pass.
- `reports/conversion-records.json`, `reports/normalization-records.json`,
  and `reports/traceability-parity-report.json` updated.
- `reports/pdfbox-main-gap-analysis.md` updated with milestone completion status.
