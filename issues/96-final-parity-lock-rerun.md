### Title
Rerun final parity lock gates and release 100% conversion decision

### Depends on
- #95 traceability debt burndown final closeout

### Background
With mapping and traceability debt expected to be fully closed, this issue re-runs canonical parity validation and records the lock decision.

### Scope
- Regenerate canonical parity reports from latest upstream head:
  - `reports/upstream-port-coverage-state.json`
  - `reports/pdfbox-main-gap-analysis.md`
  - `reports/traceability-parity-report.json`
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
- Verify final lock gates:
  - `mapped_java_files_total == upstream_java_files_total`
  - `missing_java_files_total == 0`
  - No scoped traceability rows with `partial`/`partially-in-sync`
  - Branch build/tests green
- Record lock decision in `reports/parity-execution-tracker.md`.

### Expected test scope
- Full solution build and test run.

### Exit criteria
- All parity lock gates are satisfied and documented.

### Definition of done
- Canonical reports are republished and checked in.
- Final parity lock decision is recorded with captured counters.
