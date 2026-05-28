# PDFBox .NET parity execution tracker

Last updated (UTC): 2026-05-28

## 100% parity target (canonical)

100% parity is reached only when all of the following are true:

1. Upstream scoped Java inventory is fully mapped (`mapped_java_files_total == upstream_java_files_total`).
2. Missing count is zero (`missing_java_files_total == 0`).
3. Traceability rows for scoped upstream `source_path` values are `in-sync` only (no `partial` / `partially-in-sync`).
4. Build and test validation are green for the parity branch.

Canonical scanner/report pair:

- Scanner state: `reports/upstream-port-coverage-state.json`
- Human report: `reports/pdfbox-main-gap-analysis.md`

## Milestone implementation status

- [x] M1: Lock parity target definition and canonical inventory/report pair
  - [x] Canonical mapping method formalized (provenance union traceability)
  - [x] Automated workflow scan uses canonical generator
  - [x] Coverage state, aggregate coverage JSON, and gap analysis are generated together
- [ ] M2: Close remaining `pdfbox` missing files (`contentstream/operator`, `filter`, `pdfparser`, `pdfwriter`, `pdmodel`)
- [ ] M3: Burn down `partial` / `partially-in-sync` quality debt for core modules
- [ ] M4: Complete `xmpbox` parity slices
- [ ] M5: Complete non-core modules (`tools`, `examples`, `debugger`, `benchmark`)
- [ ] M6: Final rescan at latest upstream head and release parity lock

## Execution order

1. `pdfbox` core closeout (`issues/53`-`77`)
2. `xmpbox` closeout
3. `tools`
4. `examples`
5. `debugger`
6. `benchmark`
7. final parity rescan and lock
