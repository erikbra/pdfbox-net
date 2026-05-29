### Title
Close remaining pdmodel feature-cluster gaps (`color`/`image`/`font`/`documentinterchange`)

### Depends on
- #88 shading stabilization closeout

### Background
After shading stabilization, the remaining pdmodel feature clusters should be closed in one controlled pass with same-cycle traceability normalization.

### Scope
- Port remaining files in:
  - `graphics/color`
  - `graphics/image`
  - `font`
  - `documentinterchange`
- Normalize traceability/conversion/normalization rows for touched paths.
- Keep touched rows `in-sync` in the same closeout cycle.

### Expected test scope
- Targeted pdmodel graphics/font/documentinterchange tests.

### Exit criteria
- Missing files in the scoped feature-cluster set are zero.
- Touched rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
