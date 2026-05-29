### Title
Close `tools` module parity milestone

### Depends on
- #81 XmpBox regression, traceability, and closeout

### Background
M5 starts with `tools` to maximize global parity gain with lower UI/runtime complexity.

### Scope
- Port remaining `tools/**` Java files in dependency-safe slices.
- Add/update tests for touched tool behaviors.
- Refresh traceability and canonical counters after closeout.

### Expected test scope
- Targeted tool behavior tests and solution smoke validation.

### Entry criteria
- M4 XmpBox closeout merged and green.

### Exit criteria
- `tools` mapped coverage reaches 100% for scoped upstream files.

### Definition of done
- `dotnet build` passes.
- Relevant tests pass.
- Canonical reports are updated post-rescan.
