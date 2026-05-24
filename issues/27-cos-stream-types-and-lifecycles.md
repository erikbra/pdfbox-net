### Title
Complete COS stream types and lifecycle parity

### Depends on
- #26 COS containers/primitives complete

### Scope
- Port remaining stream-adjacent COS types under `pdfbox/cos/**`.
- Preserve Java parity for stream ownership/disposal behavior in .NET equivalents.
- Keep parser orchestration out of scope.

### Expected test scope
- Add/extend COS stream tests for lifecycle, dictionary coupling, and byte-level behavior.
- Cover edge cases for disposal/close semantics.

### Exit criteria
- Stream-focused COS classes in this slice are ported and compile.
- Targeted stream/COS tests pass deterministically.
- Reporting artifacts are updated for all mapped files.
