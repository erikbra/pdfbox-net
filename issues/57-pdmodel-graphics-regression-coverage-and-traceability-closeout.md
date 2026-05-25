### Title
Close PDModel/graphics milestone with regression coverage and report completion

### Background
After graphics implementation slices land, a closeout issue should lock deterministic coverage,
finalize traceability statuses, and refresh gap-analysis reporting.

### Depends on
- #53-#56 graphics implementation slices

### Scope
- Expand fixture-backed regression coverage for completed graphics model behaviors.
- Resolve remaining blank/partial traceability statuses for touched graphics mappings.
- Refresh coverage/gap-analysis reports to reflect graphics milestone completion state.

### Expected test scope
- Focused graphics regression suite plus smoke coverage through contentstream/rendering paths.
- Validation that graphics-related regressions do not break previously completed parser/font flows.

### Entry criteria
- #53-#56 merged and green.

### Exit criteria
- `pdmodel.graphics` milestone complete for current parity target with explicit status classification.
- Coverage and gap reports refreshed and auditable.

### Risk register
- Missing representative fixtures can hide remaining graphics edge cases.
- Late-stage report cleanup may reveal mapping gaps requiring follow-up fixes.

### Definition of done
- Build + targeted tests pass.
- `reports/conversion-records.json`, `reports/normalization-records.json`,
  `reports/traceability-parity-report.json`, and `reports/pdfbox-main-gap-analysis.md` updated.
