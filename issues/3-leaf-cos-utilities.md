### Title
Complete `pdfbox/cos/**` foundation as the next major conversion chunk

### Recommendation
The **next largest dependency-safe piece to convert fully** should be the COS foundation layer (`pdfbox/cos/**`).

Why this chunk next:
- It is the base dependency for parser (`pdfparser`), writer (`pdfwriter`), and `pdmodel` pipelines.
- It can be completed with high mechanical parity and clear test boundaries.
- Finishing COS first reduces fan-out risk in later parser/document work.

### Series of implementation issues
- [ ] #26 `issues/26-cos-containers-and-primitives.md` — complete remaining COS containers/primitives.
- [ ] #27 `issues/27-cos-stream-types-and-lifecycles.md` — complete COS stream/object stream types and lifecycle parity.
- [ ] #28 `issues/28-cos-visitors-and-serialization.md` — complete visitor coverage + serializer interactions.
- [ ] #29 `issues/29-cos-regression-and-fixture-coverage.md` — add fixture-driven COS regression coverage.
- [ ] #30 `issues/30-cos-traceability-and-closeout.md` — close migration/reporting gaps and finalize COS milestone.

### Milestone completion criteria
- `dotnet build` and `dotnet test` pass.
- `pdfbox/cos/**` foundational classes compile without stubs in the selected scope.
- COS-focused tests cover primitive/container/stream/serialization behavior.
- Conversion traceability records are updated in:
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
  - `reports/traceability-parity-report.json`
