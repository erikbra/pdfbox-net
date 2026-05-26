### Title
Port multipdf overlay/layer helpers and close the milestone

### Depends on
- #75 multipdf page extraction and splitting
- #57 graphics closeout for overlay/resource interactions

### Background
The remaining `multipdf` files are overlay and layer helpers, which fit best as the final slice in
the package after clone/merge and page-copy behavior are stable.

### Scope
- Port:
  - `Overlay`
  - `LayerUtility`
- Refresh tests and reports to close the `multipdf` milestone.

### Expected test scope
- Overlay/layer fixture tests.
- Multipdf regression suite covering merge, split, extract, and overlay paths.

### Entry criteria
- #75 merged and green.

### Exit criteria
- `multipdf` reaches 6 / 6 mapped for the current parity target.

### Risk register
- Overlay behavior crosses graphics resources, optional content, and page import logic.

### Definition of done
- `dotnet build` passes.
- Multipdf regression tests pass.
- Coverage and traceability artifacts are refreshed.
