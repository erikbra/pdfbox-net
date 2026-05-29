# PDFBox .NET parity execution tracker

Last updated (UTC): 2026-05-29

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

## Baseline lock (from canonical reports)

Use these numbers as the starting baseline for every closeout decision until the next published rescan:

- `mapped_java_files_total`: **692**
- `upstream_java_files_total`: **1067**
- `missing_java_files_total`: **375**
- non-`in-sync` traceability rows: **31** (`partial` + `partially-in-sync`)
- Source of truth:
  - `reports/upstream-port-coverage-state.json`
  - `reports/pdfbox-main-gap-analysis.md`

## Mandatory execution loop for every slice

Each implementation slice is only complete when all checklist items below are true:

1. Scope is anchored to canonical scanner + gap report and explicit issue slice.
2. Code changes and tests for the slice are merged locally.
3. Build and tests are green on the branch.
4. Traceability/normalization/conversion rows are updated for touched upstream paths.
5. Canonical inventory scan is regenerated.
6. `reports/upstream-port-coverage-state.json` and `reports/pdfbox-main-gap-analysis.md` are republished with updated counters.
7. Slice is not marked done until the post-rescan counters and statuses are captured in this tracker.

## M2 implementation order (pdfbox core closeout)

Target: move `pdfbox` from **527/618 (85.3%)** to near-complete before broadening scope.

Execution order for highest-risk reduction:

1. **Parser/writer/filter foundations first**
   - `pdfparser` + `pdfwriter/compress` + required `filter` implementations
   - Primary issue anchor: `issues/60-filter-parser-writer-completeness.md`
2. **`contentstream/operator` gaps**
   - Close operator/processor/runtime integration gaps before broader model fan-out
3. **`pdmodel` core resource/cache/content-stream gaps**
   - Prioritize resource cache and document/content stream dependencies
4. **Remaining `pdmodel` graphics/font/image/shading classes**
   - Drive full main-module closure for the remaining missing paths

Issue sequence remains the prepared closeout run: `issues/53`-`77`, executed in dependency-safe order above.

## M3 quality debt burn-down policy

After each M2 slice, immediately burn down non-`in-sync` rows tied to that slice before moving on.

Priority hotspots:

- Parser/document pipeline: `Loader`, parser mappings, `PDDocument`, `PDDocumentCatalog`, `COSObject`
- Filter placeholders: `CCITT`, `DCT`, `JBIG2`, `JPX`, `Crypt`
- Deferred shading/toPaint parity path
- FontBox partials only when blocking pdfbox behavior (`CFFParser`, Type1/Type2 charstrings, `TTFParser`)

Rule: every touched traceability row must end `in-sync` in the same closeout cycle unless explicitly blocked with a documented dependency.

## M4 xmpbox completion slices

After pdfbox core stabilization, complete `xmpbox` from baseline **4/74 mapped, 70 missing** in this order:

1. Metadata entry points + XML parser/serializer path
2. Schema layer
3. Type system + property model
4. Integration tests + traceability closeout

Issue anchors:

- `issues/42-xmpbox-porting-plan.md`
- `issues/78-xmpbox-metadata-entry-points-and-xml-pipeline.md`
- `issues/79-xmpbox-schema-layer-parity.md`
- `issues/80-xmpbox-type-system-and-property-model.md`
- `issues/81-xmpbox-regression-traceability-and-closeout.md`

## M5 non-core strategy decision (explicit)

Decision for this tracker: **optimize for overall global parity increase after core lock**.

Execution order:

1. `tools` (lower UI/runtime complexity, faster percentage gain)
2. `examples`
3. `debugger`
4. `benchmark`

This order must remain explicit in parity updates so global coverage trend interpretation stays consistent.

Issue anchors:

- `issues/82-tools-module-parity-closeout.md`
- `issues/83-examples-module-parity-closeout.md`
- `issues/84-debugger-module-parity-closeout.md`
- `issues/85-benchmark-module-parity-closeout.md`

## M6 rescan/rebaseline and final parity lock

1. Complete M2 + M3 + M4 closeouts.
2. Regenerate and publish canonical reports:
   - `reports/upstream-port-coverage-state.json`
   - `reports/pdfbox-main-gap-analysis.md`
   - `reports/traceability-parity-report.json`
   - `reports/conversion-records.json`
   - `reports/normalization-records.json`
3. Confirm trend gates: missing count decreases, non-`in-sync` decreases, core subset coverage increases.
4. Fetch latest upstream head, rerun full inventory scan, reconcile drift.
5. Release parity lock only when all are true:
   - `mapped_java_files_total == upstream_java_files_total`
   - `missing_java_files_total == 0`
   - scoped traceability statuses are all `in-sync`
   - branch build/tests are green.

Issue anchor: `issues/86-final-parity-rescan-and-lock.md`.

### M6 execution snapshot (2026-05-29 UTC)

- Canonical rescan regenerated from upstream head `7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf`.
- Captured counters:
  - `mapped_java_files_total`: **974**
  - `upstream_java_files_total`: **1067**
  - `missing_java_files_total`: **93**
  - non-`in-sync` scoped traceability rows: **31** (`partial`: 11, `partially-in-sync`: 20)
- Build/tests status for this branch: **green** (`dotnet build PdfBoxNet.slnx`, `dotnet test PdfBoxNet.slnx --no-build`).
- Final parity lock decision: **NOT RELEASED** (gates remain unmet: `mapped != total`, `missing != 0`, scoped traceability includes non-`in-sync` rows).

## Execution order

1. `pdfbox` core closeout (`issues/53`-`77`)
2. `xmpbox` closeout (`issues/78`-`81`)
3. `tools` (`issues/82`)
4. `examples` (`issues/83`)
5. `debugger` (`issues/84`)
6. `benchmark` (`issues/85`)
7. final parity rescan and lock (`issues/86`)
