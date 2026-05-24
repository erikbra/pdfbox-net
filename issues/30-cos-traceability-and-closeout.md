### Title
Close COS milestone with traceability/report completeness

### Depends on
- #29 COS regression coverage complete

### Scope
- Reconcile any remaining COS mapping/report gaps.
- Ensure provenance metadata is present for all files ported in the COS milestone.
- Prepare clean handoff to parser/writer chunk.

### Expected test scope
- Run full solution verification (`dotnet build`, `dotnet test`).
- Confirm no regressions in COS-focused suites.

### Exit criteria
- COS milestone has no open conversion/normalization/traceability gaps.
- Full build/tests are green.
- Follow-on parser/writer work can start without COS blockers.
