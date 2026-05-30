### Title
Rerun final parity lock after semantic traceability closeout

### Depends on
- #99 filter/security + FontBox parser semantic parity closeout

### Background
After issues #97-#99, all previously non-`in-sync` scoped rows should be closed. This issue performs the final canonical rerun and lock decision publication.

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
