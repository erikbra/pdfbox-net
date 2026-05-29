### Title
Close XmpBox milestone with regression coverage, traceability, and report refresh

### Depends on
- #78-#80 XmpBox implementation slices

### Background
XmpBox parity should close with explicit regression and canonical report refresh before
non-core module execution begins.

### Scope
- Expand fixture-backed XmpBox regression coverage for parser/schema/type flows.
- Burn down remaining non-`in-sync` XmpBox traceability rows.
- Regenerate canonical parity reports and capture post-rescan counters.

### Expected test scope
- Full `PdfBox.Net.XmpBox.Tests` suite plus relevant integration smoke tests.

### Entry criteria
- Slices #78-#80 merged and green.

### Exit criteria
- XmpBox mappings are fully classified with no unresolved partial status in touched rows.
- Canonical reports reflect the completed XmpBox milestone.

### Risk register
- Late-stage report refresh can expose hidden gaps in earlier slices.

### Definition of done
- `dotnet build` passes.
- XmpBox regression tests pass.
- Canonical coverage/gap and traceability reports are republished.
