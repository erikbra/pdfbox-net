### Title
Create the next-stage conversion plan with larger dependency-ordered chunks.

### Goal
Produce a concrete, dependency-aware roadmap for the next PDFBox -> .NET conversion stages using larger work packages than current micro-slices, while preserving quality and traceability.

### Current Context
- `io` is substantially ported and covered with tests.
- `pdfbox` has seed coverage in `util` and low-dependency `cos` primitives.
- Parser/writer and broader document/model layers remain.
- Conversion process must continue to use Skill flow A -> F -> E, with Skill G mappings, provenance headers, and updates to:
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
  - `reports/traceability-parity-report.json`

### Planning Constraints
- Use dependency-safe sequencing (no top-layer ports before required foundations).
- Increase chunk size compared to file-by-file slices, but keep PRs reviewable.
- Keep mechanical parity intent explicit; record intentional adaptations.

---

## Chunk 1 - Complete COS foundation set (containers + primitives + streams)

### Scope boundaries
- Complete foundational `pdfbox/cos` entities needed by parser/writer entry points.
- Include container/value/stream-adjacent COS types and visitor completeness for this layer.
- Exclude full parser orchestration and high-level `pdmodel` behaviors.

### Target upstream areas/classes
- `pdfbox/src/main/java/org/apache/pdfbox/cos/*` (remaining foundational and container types).

### Expected test scope
- Extend/add COS-focused tests in `tests/PdfBox.Net.Tests/*COS*`.
- Add serialization/roundtrip coverage for core COS containers and primitive interactions.

### Entry criteria
- Existing baseline (`dotnet build`, `dotnet test`) is green.
- Existing COS primitive seed files remain in-sync and passing.

### Exit criteria
- COS foundation compiles cleanly with coherent visitor coverage.
- COS behavior tests pass for primitives + containers + stream-adjacent types.
- Required provenance and report artifacts are updated.

### Risk register
- Java collection/ordering semantics mismatch in dictionary/array-like COS containers.
- Numeric/lexical edge-case drift (`COSFloat`/`COSNumber` parsing behavior).
- Stream ownership/lifetime differences across Java vs .NET I/O abstractions.

### PR slicing rule
- Allow sub-PRs by COS subclusters (containers, names/strings, stream-related), but all must land under one milestone with no unresolved cluster gaps.

### Definition of done
- `dotnet build` passes.
- Relevant COS test subset passes (and full suite if touched broadly).
- `reports/*.json` updated for conversion/normalization/traceability rows.
- Skill G mapping notes recorded for any non-trivial substitutions.

### Issue drafts prepared for the COS execution path
- `issues/26-cos-containers-and-primitives.md`
- `issues/27-cos-stream-types-and-lifecycles.md`
- `issues/28-cos-visitors-and-serialization.md`
- `issues/29-cos-regression-and-fixture-coverage.md`
- `issues/30-cos-traceability-and-closeout.md`

---

## Chunk 2 - Parser/writer low-level bridge

### Scope boundaries
- Port low-level parser/writer support needed for token/object flow.
- Include dependencies in `filter` only where required for parser/writer correctness.
- Exclude high-level feature APIs and broad `pdmodel` expansion.

### Target upstream areas/classes
- `pdfbox/src/main/java/org/apache/pdfbox/filter/*` (required subset).
- `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/*` (low-level flow subset).
- `pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/*` (low-level support subset).

### Expected test scope
- Add parser/writer focused tests for token/object read-write roundtrip.
- Validate deterministic behavior on representative small fixtures.

### Entry criteria
- Chunk 1 COS milestone complete and stable.
- No open compile or traceability gaps in COS dependency layer.

### Exit criteria
- Minimal object parse/serialize roundtrip passes.
- Parser/writer low-level components compile and pass targeted tests.
- Reporting artifacts updated for all introduced mappings.

### Risk register
- Tokenization and lexical rules differ subtly under .NET string/stream behavior.
- Compression/filter API adaptation may introduce hidden semantic drift.
- Error/exception handling parity (`IOException`-family behavior) may diverge.

### PR slicing rule
- Split by parser vs writer vs required filter support, but each PR must preserve runnable roundtrip progress and avoid temporary dead-end scaffolding.

### Definition of done
- `dotnet build` passes.
- Parser/writer roundtrip tests pass.
- Traceability and normalization records updated with explicit adaptation notes.

### Issue drafts prepared for the parser/writer bridge execution path
- `issues/37-pdf-loading-parser-scaffold-and-startxref.md`
- `issues/38-pdf-loading-xref-table-and-trailer-resolution.md`
- `issues/39-pdf-loading-xref-stream-and-object-stream-parser.md`
- `issues/40-pdf-loading-cosdocument-resolution-and-pddocument-integration.md`
- `issues/41-pdf-loading-regression-fixtures-roundtrip-and-report-closeout.md`

---

## Chunk 3 - Minimal document pipeline

### Scope boundaries
- Deliver first viable `pdmodel` path to open, inspect basic structure/metadata, and save.
- Keep API footprint minimal and dependency-aligned with completed parser/writer base.
- Exclude advanced features (forms, text extraction breadth, rendering).

### Target upstream areas/classes
- Initial subset of `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/*` required for open/read/save pipeline.

### Expected test scope
- End-to-end smoke pipeline tests using deterministic fixture PDFs.
- Basic open -> inspect -> save regression checks.

### Entry criteria
- Chunk 2 parser/writer bridge stable and roundtrip green.

### Exit criteria
- Minimal document pipeline executes end-to-end with passing smoke regressions.
- Reports and provenance updated for all newly ported classes/tests.

### Risk register
- Document object lifecycle and ownership differences in .NET disposal model.
- Metadata/date/encoding handling parity gaps.
- Fixture brittleness if tests depend on non-deterministic external state.

### PR slicing rule
- Slice by pipeline stages (open/read, inspect, save), but each merged slice must preserve an executable partial pipeline and maintain green tests.

### Definition of done
- `dotnet build` passes.
- Pipeline smoke tests pass deterministically.
- No unresolved compile leftovers in new `pdmodel` subset.

---

## Chunk 4 - Functional parity expansion

### Scope boundaries
- Expand prioritized capabilities after minimal pipeline is stable.
- Priority order: metadata completeness, outlines/forms, then text extraction baseline.
- Keep scope feature-driven, not file-driven.

### Target upstream areas/classes
- Feature-specific subsets across `pdmodel`, `text`, and related support namespaces as needed.

### Expected test scope
- Feature-focused parity tests per capability group.
- Update/maintain a parity matrix for selected v1 features.

### Entry criteria
- Chunk 3 end-to-end pipeline complete and stable.

### Exit criteria
- Selected feature parity targets are green in tests and parity matrix.
- Traceability statuses remain current and auditable.

### Risk register
- Cross-cutting dependencies causing fan-out beyond planned feature boundaries.
- Text extraction behavioral parity (layout/ordering nuances).
- Forms/outlines edge cases tied to incomplete low-level coverage.

### PR slicing rule
- Slice by feature package (metadata, outlines/forms, text baseline), but each package PR must include tests and updated parity tracking.

### Definition of done
- Build and feature test scopes pass.
- Parity matrix updated with clear pass/known-gap markers.
- Mapping/report artifacts complete for each feature package.

### Issue drafts prepared for the text-baseline execution path
- `issues/14-contentstream-execution-core.md`
- `issues/15-contentstream-operator-processors.md`
- `issues/16-text-extraction-pdmodel-support.md`
- `issues/17-text-functional-baseline.md`
- `issues/18-text-extraction-regression-coverage.md`

---

## Chunk 5 - Hardening + sync workflow maturity

### Scope boundaries
- Improve robustness, regression reliability, and upstream sync execution quality.
- Formalize recurring B/C/D sync operation with conflict handling and auditability.
- Exclude major new feature expansion.

### Target upstream areas/classes
- Cross-cutting; includes touched modules from prior chunks and sync-impact areas.

### Expected test scope
- Broader regression corpus validation.
- Negative/malformed input stability tests where available.
- Sync scenario checks for update/new-file/delete pathways.

### Entry criteria
- Chunks 1-4 completed with stable green baseline.

### Exit criteria
- Repeatable sync workflow with low manual conflict rate.
- Robust regression confidence across previously delivered chunks.
- All traceability/parity reporting remains current and consistent.

### Risk register
- Upstream churn introduces repeated merge/update conflicts.
- Hidden semantic drift accumulates across many adaptation points.
- Test runtime/corpus management overhead reduces iteration speed.

### PR slicing rule
- Slice by hardening focus (sync workflow, robustness tests, reporting automation), but all slices must preserve current behavior and keep audit trails complete.

### Definition of done
- Full solution build/tests pass.
- Sync/update/delete/new-file flows are documented and repeatable.
- Reports reflect current sync state and known manual-sync exceptions.

---

## Global quality gates (apply to every chunk)
- `dotnet build` passes.
- Relevant `dotnet test` scope passes for touched areas.
- Provenance headers are present and correct on all mechanically ported files.
- Required report rows are updated in:
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
  - `reports/traceability-parity-report.json`
- Skill G mapping decisions are documented for non-trivial substitutions.
- No unresolved compile leftovers or silent semantic drift.

## Acceptance criteria for this planning task
- The plan is specific enough to open the next 4-5 implementation issues directly.
- Chunk boundaries are larger than prior file-level slices while staying dependency-safe.
- Every chunk includes explicit scope, entry/exit criteria, risks, PR slicing, and definition of done.

---

## Status refresh (2026-05-25) and next recommended execution chunk

- Chunks 1 and 2 are effectively complete for current parity targets (COS foundation + parser/writer bridge).
- The `pdmodel.documentinterchange` milestone is complete through issues #43–#47 with fixture-backed regression coverage.
- The `pdmodel.font` milestone has advanced through #50 (TrueType/CIDType2 parity); #51 and #52 remain as closeout slices.
- The next large dependency-safe chunk to execute fully after font closeout is now **`pdmodel.graphics` completion**.

## Parity execution refresh (2026-05-29)

- Canonical progress sources are now fixed to:
  - `reports/upstream-port-coverage-state.json`
  - `reports/pdfbox-main-gap-analysis.md`
- Current baseline snapshot:
  - **692 / 1067** mapped (64.9%)
  - **375** missing
  - **31** non-`in-sync` traceability rows
- Every slice now follows the locked closeout loop:
  1. code/test completion
  2. green build/tests
  3. traceability/conversion/normalization updates
  4. canonical rescan regeneration
  5. tracker counter/status capture before marking done
- Updated dependency-first implementation order:
  1. parser/writer/filter foundations (`issues/60`)
  2. `contentstream/operator` gaps
  3. `pdmodel` resource/cache/content-stream core
  4. remaining `pdmodel` graphics/font/image/shading classes
  5. `xmpbox` slices after core stabilization

### Remaining font closeout issues
- `issues/51-pdmodel-font-type0-cidtype0-and-unicode-integration.md`
- `issues/52-pdmodel-font-regression-coverage-and-traceability-closeout.md`

### Next issue series prepared for the graphics chunk
- `issues/53-pdmodel-graphics-shading-and-core-types.md`
- `issues/54-pdmodel-graphics-patterns-optional-content-and-inline-image.md`
- `issues/55-pdmodel-graphics-state-and-xobject-integration.md`
- `issues/56-graphics-contentstream-and-rendering-integration.md`
- `issues/57-pdmodel-graphics-regression-coverage-and-traceability-closeout.md`

### Follow-on issue series prepared for full main-module parity
- `issues/58-report-traceability-and-small-gap-hygiene.md`
- `issues/59-pdmodel-font-backfill-after-closeout.md`
- `issues/60-filter-parser-writer-completeness.md`
- `issues/61-pdmodel-interactive-slice-a-utilities-names-and-viewer-preferences.md`
- `issues/62-pdmodel-interactive-slice-b-destinations-outlines-and-navigation.md`
- `issues/63-pdmodel-interactive-slice-c-actions-and-additional-actions.md`
- `issues/64-pdmodel-interactive-slice-d-annotation-core-and-appearance-dictionaries.md`
- `issues/65-pdmodel-interactive-slice-e-acroform-core-and-field-tree.md`
- `issues/66-pdmodel-interactive-slice-f-annotation-appearance-handlers-and-generation.md`
- `issues/67-pdmodel-interactive-slice-g-digital-signatures-visible-signatures-and-measurement.md`
- `issues/68-pdmodel-interactive-slice-h-regression-traceability-and-closeout.md`
- `issues/69-pdmodel-fdf-document-core.md`
- `issues/70-pdmodel-fdf-field-page-template-model.md`
- `issues/71-pdmodel-fdf-annotation-mirror-types.md`
- `issues/72-pdmodel-fdf-regression-traceability-and-closeout.md`
- `issues/73-pdmodel-fixup-core-and-processors.md`
- `issues/74-multipdf-clone-merge-foundation.md`
- `issues/75-multipdf-page-extraction-and-splitting.md`
- `issues/76-multipdf-overlay-layer-and-closeout.md`
- `issues/77-encryption-publickey-factory-provider-closeout.md`
