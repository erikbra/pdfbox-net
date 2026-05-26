### Title
Close PDModel/FDF milestone with regression coverage and report refresh

### Depends on
- #69-#71 FDF implementation slices

### Background
FDF should close as its own milestone once document, field/page/template, and annotation mirror
paths are implemented.

### Scope
- Expand FDF regression coverage.
- Resolve remaining FDF traceability statuses.
- Refresh gap-analysis and coverage reports.

### Expected test scope
- FDF regression suite plus smoke validation through related parser/load paths.

### Entry criteria
- #69-#71 merged and green.

### Exit criteria
- `pdmodel.fdf` reaches 30 / 30 mapped for the current parity target.

### Risk register
- Report refresh may reveal lingering parser or annotation-model gaps.

### Definition of done
- `dotnet build` passes.
- FDF regression tests pass.
- Coverage and traceability artifacts are refreshed.
