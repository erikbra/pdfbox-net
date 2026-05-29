### Title
Execute final parity rescan and release lock gates

### Depends on
- #77 pdfbox core closeout
- #81 XmpBox closeout
- #82-#85 non-core module closeouts

### Background
M6 requires a final upstream-head reconciliation and canonical rescan before parity can
be declared complete.

### Scope
- Regenerate canonical inventory and gap reports from latest upstream head.
- Refresh traceability/conversion/normalization reports for final counters.
- Verify all parity lock gates before release declaration.

### Expected test scope
- Full solution build and test run.

### Entry criteria
- M2-M5 milestones are complete and green.

### Exit criteria
- `mapped_java_files_total == upstream_java_files_total`
- `missing_java_files_total == 0`
- Scoped traceability statuses are all `in-sync`
- Final branch build/tests are green.

### Definition of done
- Canonical reports are republished and checked in.
- Final parity lock decision is documented with captured counters.
