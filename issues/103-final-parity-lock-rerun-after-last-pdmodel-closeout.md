### Title
Rerun final parity lock after last PDModel semantic closeout

### Depends on
- #102 PDPage tree + rectangle semantic parity closeout

### Background
PR #191 / issue #100 already performed a final lock rerun and proved that mapping, missing-file, and build/test gates are green. Once issues #101-#102 close the last 6 PDModel `partially-in-sync` rows, this issue performs the new canonical rerun and records the final lock decision.

### Scope
- Regenerate canonical parity reports:
  - `reports/upstream-port-coverage-state.json`
  - `reports/pdfbox-main-gap-analysis.md`
  - `reports/traceability-parity-report.json`
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
- Re-evaluate lock gates:
  - `mapped_java_files_total == upstream_java_files_total`
  - `missing_java_files_total == 0`
  - scoped non-`in-sync` traceability rows == `0`
  - branch build/tests green
- Record the final lock decision in `reports/parity-execution-tracker.md`.

### Expected test scope
- Full solution build and test run.

### Exit criteria
- All lock gates are satisfied and documented, or any remaining blocker is explicitly recorded with exact counters.

### Definition of done
- Canonical reports are republished and checked in.
- Final lock decision is recorded with captured counters and gate table.
