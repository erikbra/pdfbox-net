### Title
Complete shading stabilization wave and close remaining shading parity gaps

### Depends on
- #87 core foundation mapping + quality closeout

### Background
Shading is the largest concentrated graphics debt area and must be closed as one stabilization wave before moving to the remaining pdmodel feature-cluster closeout.

### Scope
- Port remaining missing `graphics/shading` files.
- Resolve shading `partial` and `partially-in-sync` traceability notes together.
- Capture shading traceability evidence and verify all shading rows are `in-sync` post-rescan.

### Expected test scope
- Targeted shading and rendering tests.

### Exit criteria
- Missing shading files are zero.
- Shading traceability rows are all `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted shading/rendering tests pass.
- Canonical reports are regenerated and checked in.
