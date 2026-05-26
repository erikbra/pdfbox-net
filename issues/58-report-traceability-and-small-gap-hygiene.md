### Title
Close traceability hygiene and near-complete small gaps for the 618-file baseline

### Depends on
- Current gap analysis refresh in `reports/pdfbox-main-gap-analysis.md`
- Existing operator, common, document-interchange, util, and parser baselines

### Background
The fastest path toward credible 100% parity is to fix report accuracy before opening more
high-volume porting work. The current baseline still carries blank traceability rows, stale
upstream-path mappings, and several small nearly-complete package gaps.

### Scope
- Classify the blank-status rows concentrated in `contentstream.operator.color`,
  `contentstream.operator.graphics`, and `contentstream.operator.state`.
- Normalize or remove stale upstream-path mappings called out by the gap report.
- Close the remaining small package gaps for:
  - `pdmodel.common` (`COSDictionaryMap`, `PDImmutableRectangle`, `PDObjectStream`)
  - `pdmodel.documentinterchange` tagged attribute objects
  - `util` (`IterativeMergeSort`, `Version`, `XMLUtil`)
  - `util.filetypedetector` (`ByteTrie`, `FileType`, `FileTypeDetector`)
  - `Loader`
- Refresh coverage and gap reports after the hygiene pass lands.

### Expected test scope
- Targeted tests for any newly ported small-gap classes.
- Validation that report-generation artifacts remain internally consistent.

### Entry criteria
- Current build/test baseline is green.

### Exit criteria
- No blank traceability rows remain in the touched operator packages.
- Stale path-drift mappings are removed or normalized.
- The near-complete small package gaps are closed or explicitly deferred with rationale.
- Gap-analysis reporting reflects the true denominator and updated mapping totals.

### Risk register
- Some stale mappings may reflect upstream renames rather than pure deletions.
- `Loader` can fan out into parser/writer and security behaviors if scope expands.

### PR slicing rule
- First PR: traceability classification + stale mapping cleanup.
- Second PR: near-complete small package ports + report refresh.

### Definition of done
- `dotnet build` passes.
- Relevant targeted tests pass.
- `reports/conversion-records.json`, `reports/normalization-records.json`,
  `reports/traceability-parity-report.json`, and `reports/pdfbox-main-gap-analysis.md` are updated.
