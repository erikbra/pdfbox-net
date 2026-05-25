### Title
Close PDModel/font milestone with regression coverage and report completion

### Background
After core and concrete font-type slices land, the milestone closeout should lock in deterministic
fixture coverage and complete traceability/report hygiene for `pdmodel.font`.

### Depends on
- Issues #48–#51

### Scope
- Expand fixture-backed regression coverage across the completed font stack.
- Resolve remaining blank/partial traceability statuses in touched `pdmodel.font` mappings.
- Update coverage and gap-analysis reports to reflect milestone completion state.

### Expected test scope
- Focused regression suite for Standard 14, Type1, TrueType, and composite Type0/CID paths.
- Smoke validation through text extraction scenarios that depend on completed font behavior.

### Entry criteria
- #48–#51 merged and green.

### Exit criteria
- `pdmodel.font` is complete for the current parity target with explicit status classification.
- Coverage/gap reports are refreshed and auditable.

### Risk register
- Fixture selection may miss corner cases unless representative font PDFs are included.
- Late-stage report updates can reveal hidden mapping gaps.

### Definition of done
- Build + targeted tests pass.
- `reports/conversion-records.json`, `reports/normalization-records.json`,
  `reports/traceability-parity-report.json`, and `reports/pdfbox-main-gap-analysis.md` updated.
