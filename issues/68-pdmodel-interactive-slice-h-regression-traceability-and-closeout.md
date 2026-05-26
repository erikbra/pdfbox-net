### Title
Close PDModel interactive milestone with regression coverage, traceability, and report refresh

### Depends on
- #61-#67 interactive implementation slices

### Background
The interactive package is the largest remaining main-module family. Its milestone should close
with explicit regression, traceability classification, and refreshed coverage reporting.

### Scope
- Expand fixture-backed regression coverage across bookmarks, actions, annotations, forms, and
  signatures.
- Resolve remaining blank/partial statuses in touched interactive mappings.
- Refresh gap-analysis and coverage reports to reflect the completed interactive milestone.

### Expected test scope
- Interactive package regression suite plus smoke validation through document load/save paths.

### Entry criteria
- Slices A-G merged and green.

### Exit criteria
- `pdmodel.interactive` reaches 144 / 144 mapped for the current parity target.
- Touched interactive mappings have explicit, auditable status classification.

### Risk register
- Late-stage report refresh can expose missed support types or partial hotspots.

### Definition of done
- `dotnet build` passes.
- Interactive regression tests pass.
- Coverage and traceability artifacts are refreshed.
