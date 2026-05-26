### Title
Port PDModel interactive slice G: digital signatures, visible signatures, and measurement

### Depends on
- #66 interactive slice F
- #77 encryption public-key closeout

### Background
Digital signatures, visible-signature helpers, and measurement types are currently zero-coverage
subareas and must be handled as a dedicated advanced slice.

### Scope
- Port digital-signature core types, signing support interfaces, seed-value dictionaries, and
  signature options.
- Port visible-signature helper/build classes needed for parity in the current upstream package.
- Port the `interactive.measurement` package.

### Expected test scope
- Signature dictionary/property tests.
- Visible-signature model tests that avoid external signing dependencies.
- Measurement dictionary tests.

### Entry criteria
- Slice F merged and green.

### Exit criteria
- Signature and measurement packages are structurally mapped for the current parity target.
- Signing-support types compile and participate in deterministic model-level tests.

### Risk register
- External signing flows can fan out into save/incremental-save and security details.

### Definition of done
- `dotnet build` passes.
- Signature/measurement targeted tests pass.
- Traceability artifacts are updated.
