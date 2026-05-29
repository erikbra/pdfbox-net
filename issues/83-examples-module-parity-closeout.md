### Title
Close `examples` module parity milestone

### Depends on
- #82 tools module parity closeout

### Background
`examples` is the second M5 module in the locked execution order.

### Scope
- Port remaining `examples/**` files.
- Validate representative sample flows continue to run.
- Update traceability and canonical parity counters.

### Expected test scope
- Example-oriented smoke tests where available.

### Entry criteria
- `tools` milestone complete and green.

### Exit criteria
- `examples` mapped coverage reaches 100% for scoped upstream files.

### Definition of done
- `dotnet build` passes.
- Relevant tests pass.
- Canonical reports are updated post-rescan.
