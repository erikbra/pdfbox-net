### Title
Close `debugger` module parity milestone

### Depends on
- #83 examples module parity closeout

### Background
`debugger` follows examples in M5 after lower-complexity modules are complete.

### Scope
- Port remaining `debugger/**` files.
- Validate core debugger workflows and touched integration points.
- Update traceability and canonical parity counters.

### Expected test scope
- Debugger-focused tests/smoke flows for touched paths.

### Entry criteria
- `examples` milestone complete and green.

### Exit criteria
- `debugger` mapped coverage reaches 100% for scoped upstream files.

### Definition of done
- `dotnet build` passes.
- Relevant tests pass.
- Canonical reports are updated post-rescan.
