# PDFBox .NET parity execution tracker

Last updated (UTC): 2026-05-30

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
- [x] M2: Close remaining `pdfbox` missing files (`contentstream/operator`, `filter`, `pdfparser`, `pdfwriter`, `pdmodel`)
- [ ] M3: Burn down `partial` / `partially-in-sync` quality debt for core modules
- [x] M4: Complete `xmpbox` parity slices
- [x] M5: Complete non-core modules (`tools`, `examples`, `debugger`, `benchmark`)
- [ ] M6: Final rescan at latest upstream head and release parity lock

## Baseline lock (from canonical reports)

Use these numbers as the starting baseline for every closeout decision until the next published rescan:

- `mapped_java_files_total`: **1067**
- `upstream_java_files_total`: **1067**
- `missing_java_files_total`: **0**
- non-`in-sync` traceability rows: **21** (`partial`: 11, `partially-in-sync`: 10)
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

## M2 implementation order (remaining mapping gaps)

Target: close all currently missing mapped paths in a dependency-safe order.

Execution order:

1. **pdfbox core foundations (22 files)**
   - `contentstream` (11)
   - `pdfparser` (7)
   - `pdfwriter/compress` (3)
   - `filter/Filter` (1)
2. **pdmodel core/root gaps (~18 files)**
   - resource-cache/content-stream/name-tree/documentinterchange root and other non-graphics root classes
3. **pdmodel feature clusters (51 files)**
   - `graphics/shading` (27)
   - `graphics/color` (10)
   - `graphics/image` (7)
   - `font` (6)
   - `documentinterchange` (1)
4. **xmpbox closeout (2 files)**
   - `DomHelper`
   - `PdfaExtensionHelper`

Issue sequence remains the prepared closeout run (`issues/53`-`81`) executed in this dependency order.

## M3 quality debt burn-down policy

After each M2 slice, immediately burn down non-`in-sync` rows tied to that slice before moving on.

Priority hotspots:

- `partial` rows: filter placeholders (`CCITT`, `DCT`, `JBIG2`, `JPX`, `Crypt`), `Loader`, `StandardSecurityHandler`, FontBox parser/charstring rows
- `partially-in-sync` rows: parser/document pipeline (`BaseParser`, `COSObject`, `PDDocument*`, `PDPage*`, `PDRectangle`), inline-image operator split, and shading `toPaint`/rendering paths

Rule: every touched traceability row must end `in-sync` in the same closeout cycle unless explicitly blocked with a documented dependency.

### Dedicated shading stabilization wave

Shading is the largest concentrated debt area and must be closed in one stabilization pass before advancing beyond the shading slice:

1. Complete missing shading files in the mapped gap list.
2. Resolve all shading traceability notes (`partial` / `partially-in-sync`) together.
3. Re-run parity reports and confirm shading rows are all `in-sync` before moving on.

## M4 xmpbox completion slices

After pdfbox core stabilization, close the remaining `xmpbox` delta (**72/74 mapped, 2 missing**) in one focused pass:

1. Port `DomHelper`.
2. Port `PdfaExtensionHelper`.
3. Complete tests + traceability/normalization/conversion rows + canonical rescan for the xmpbox slice.

Issue anchors:

- `issues/42-xmpbox-porting-plan.md`
- `issues/78-xmpbox-metadata-entry-points-and-xml-pipeline.md`
- `issues/90-xmpbox-two-file-closeout.md`

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

1. Complete M2 + M3 + M4 + M5 closeouts.
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

Issue anchor: `issues/91-final-parity-rescan-and-lock-execution.md`.

### M6 execution snapshot (2026-05-29 UTC)

- Canonical rescan regenerated from upstream head `7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf`.
- Captured counters:
  - `mapped_java_files_total`: **974**
  - `upstream_java_files_total`: **1067**
  - `missing_java_files_total`: **93**
  - non-`in-sync` scoped traceability rows: **31** (`partial`: 11, `partially-in-sync`: 20)
- Build/tests status for this branch: **green** (`dotnet build PdfBoxNet.slnx`, `dotnet test PdfBoxNet.slnx --no-build`).
- Final parity lock decision: **NOT REACHED** (gates remain unmet: `mapped != total`, `missing != 0`, scoped traceability includes non-`in-sync` rows).

### M6 execution snapshot (2026-05-30 UTC) — issue #91

- Canonical rescan regenerated from latest upstream head `eeb5d611e0cea8beac3d7025a4dbccbef51d5caf`.
- Tracked parity baseline commit: `a71c5679d69bc3fd3ab15e248b69441ee91dca6c`.
- Captured counters:
  - `mapped_java_files_total`: **1067**
  - `upstream_java_files_total`: **1067**
  - `missing_java_files_total`: **0**
  - non-`in-sync` scoped traceability rows: **21** (`partial`: 11, `partially-in-sync`: 10)
- Build/tests status for this branch: **green** (`dotnet build PdfBoxNet.slnx` — 0 errors; `dotnet test PdfBoxNet.slnx --no-build` — 1049 passed, 0 failed).
- Progress since prior snapshot: missing −93 (93 → 0); non-`in-sync` rows −10 (31 → 21).
- Lock gate evaluation:

  | Gate | Required | Actual | Met? |
  |---|---|---|---|
  | `mapped_java_files_total == upstream_java_files_total` | 1067 | 1067 | ✅ |
  | `missing_java_files_total == 0` | 0 | 0 | ✅ |
  | All scoped traceability rows `in-sync` | 0 non-`in-sync` | 21 | ❌ |
  | Branch build/tests green | all pass | 1049/1049 | ✅ |

- Final parity lock decision: **NOT REACHED** (gate still unmet: 21 non-`in-sync` traceability rows remain).
- Remaining work: resolve 21 non-`in-sync` traceability rows before the lock can be released.

### M6 execution snapshot (2026-05-30 UTC) — post-refresh

- Canonical reports regenerated at `2026-05-30T20:58:25.328Z` from upstream head `eeb5d611e0cea8beac3d7025a4dbccbef51d5caf`.
- Captured counters (from `reports/upstream-port-coverage-state.json`):
  - `mapped_java_files_total`: **1067**
  - `upstream_java_files_total`: **1067**
  - `missing_java_files_total`: **0**
  - non-`in-sync` scoped traceability rows: **21** (`partial`: 11, `partially-in-sync`: 10)
- Lock gate evaluation remains unchanged:

  | Gate | Required | Actual | Met? |
  |---|---|---|---|
  | `mapped_java_files_total == upstream_java_files_total` | 1067 | 1067 | ✅ |
  | `missing_java_files_total == 0` | 0 | 0 | ✅ |
  | All scoped traceability rows `in-sync` | 0 non-`in-sync` | 21 | ❌ |
  | Branch build/tests green | all pass | pending this slice | ⏳ |

- Next large parity-completion wave: **semantic/traceability closeout of parser + PDModel document pipeline + filter/security + FontBox parser rows**.

### M6 execution snapshot (2026-05-30 UTC) — issue #96

- Canonical rescan regenerated from latest upstream head `eeb5d611e0cea8beac3d7025a4dbccbef51d5caf`.
- Tracked parity baseline commit: `a71c5679d69bc3fd3ab15e248b69441ee91dca6c`.
- Captured counters:
  - `mapped_java_files_total`: **1067**
  - `upstream_java_files_total`: **1067**
  - `missing_java_files_total`: **0**
  - non-`in-sync` scoped traceability rows: **21** (`partial`: 11, `partially-in-sync`: 10)
- Build/tests status for this branch: **green** (`dotnet build PdfBoxNet.slnx` — 0 errors; `dotnet test PdfBoxNet.slnx --no-build` — 1059 passed, 0 failed).
- Progress since prior snapshot: mapped +32 (1035 → 1067); missing −32 (32 → 0); non-`in-sync` rows unchanged (21 → 21).
- Lock gate evaluation:

  | Gate | Required | Actual | Met? |
  |---|---|---|---|
  | `mapped_java_files_total == upstream_java_files_total` | 1067 | 1067 | ✅ |
  | `missing_java_files_total == 0` | 0 | 0 | ✅ |
  | All scoped traceability rows `in-sync` | 0 non-`in-sync` | 21 | ❌ |
  | Branch build/tests green | all pass | 1059/1059 | ✅ |

- Final parity lock decision: **NOT REACHED** (mapping/missing gates are now satisfied, but 21 scoped `partial`/`partially-in-sync` traceability rows still block lock release).

## Execution order

1. parser/contentstream semantic closeout (`issues/97`)
2. PDModel document pipeline semantic closeout (`issues/98`)
3. filter/security + FontBox parser semantic closeout (`issues/99`)
4. final parity lock rerun (`issues/100`)

## Concrete next issues toward 100% conversion

Execute these in order and apply the mandatory closeout loop after each issue:

1. `issues/97-parser-inline-image-traceability-closeout.md`
   - Resolve parser/contentstream focused non-`in-sync` rows (`BeginInlineImage`, `BaseParser`, `COSObject`, `Loader`).
   - Keep split inline-image operator behavior mechanically aligned to upstream Java semantics.
2. `issues/98-pdmodel-document-pipeline-traceability-closeout.md`
   - Resolve PDModel document-pipeline non-`in-sync` rows (`PDDocument`, `PDDocumentCatalog`, `PDDocumentInformation`, `PDPage`, `PDPageTree`, `PDRectangle`).
   - Close remaining parity notes for document load/save/page traversal behavior.
3. `issues/99-filter-security-fontbox-traceability-closeout.md`
   - Resolve remaining filter/security/font-parser non-`in-sync` rows (`CCITTFax`, `Crypt`, `DCT`, `JBIG2`, `JPX`, `StandardSecurityHandler`, `CFFParser`, `Type1/Type2CharString`, `TTFParser`).
   - Ensure adaptation notes are reduced to `in-sync` justifications or eliminated by implementation updates.
4. `issues/100-final-traceability-lock-rerun.md`
   - Regenerate canonical reports, verify zero scoped non-`in-sync` rows, and rerun full lock gates.
   - Record final release decision in the tracker with full counters.
