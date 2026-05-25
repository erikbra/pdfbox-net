### Title
Port documentinterchange marked-content and object-reference classes

### Background
After structure-tree core is in place, tagged PDFs need the bridge objects that link
structure elements to page content and external objects.

### Depends on
- Issue #43 (structure tree core)

### Scope
Port reference classes used by tagged content:
- `PDMarkedContentReference`
- `PDObjectReference`
- related helper/base types used by these classes in `logicalstructure`

Ensure MCID and referenced object lookups are wired to existing COS/PDModel layers.

### Expected test scope
- Parse tagged content with MCIDs and verify structure element references resolve.
- Parse object references and verify target object dictionary wiring.

### Entry criteria
- Issue #43 merged and green.

### Exit criteria
- Marked-content and object references parse correctly from fixture PDFs.
- No unresolved TODO/stub placeholders in the newly ported classes.

### Risk register
- Reference loops and missing targets in malformed PDFs.
- Subtle null-handling differences between Java and C# collections.

### Definition of done
- Build passes.
- New reference-resolution tests pass.
- Conversion + traceability report rows updated.
