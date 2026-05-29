### Title
Close `benchmark` module parity milestone

### Depends on
- #84 debugger module parity closeout

### Background
`benchmark` is the final non-core module in M5 before M6 final parity lock.

### Scope
- Port remaining `benchmark/**` files.
- Validate benchmark module compiles and behaves consistently for touched paths.
- Update traceability and canonical parity counters.

### Expected test scope
- Benchmark module compile/smoke coverage.

### Entry criteria
- `debugger` milestone complete and green.

### Exit criteria
- `benchmark` mapped coverage reaches 100% for scoped upstream files.

### Definition of done
- `dotnet build` passes.
- Relevant tests pass.
- Canonical reports are updated post-rescan.
