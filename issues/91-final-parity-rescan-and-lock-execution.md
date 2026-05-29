### Title
Execute final parity rescan from latest upstream head and release 100% lock

### Depends on
- #90 XmpBox two-file closeout
- #82-#85 non-core module closeouts

### Background
This is the final lock issue. It performs upstream-head reconciliation, full rescan, and gate validation for 100% conversion.

### Scope
- Fetch latest upstream head and reconcile drift.
- Regenerate canonical parity reports.
- Verify and record final lock gates:
  - `mapped_java_files_total == upstream_java_files_total`
  - `missing_java_files_total == 0`
  - Scoped traceability statuses are all `in-sync`
  - Branch build/tests are green

### Expected test scope
- Full solution build and test run.

### Exit criteria
- All lock gates are true and documented.

### Definition of done
- Canonical reports are republished and checked in.
- Final parity lock decision is recorded with captured counters.
