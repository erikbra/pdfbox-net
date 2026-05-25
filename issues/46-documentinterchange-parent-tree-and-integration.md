### Title
Port documentinterchange parent-tree wiring and PDModel integration

### Background
To complete tagged PDF read-path parity, documentinterchange classes must integrate
with page-level marked-content parent trees and catalog access points.

### Depends on
- Issues #43–#45

### Scope
Complete parent-tree and integration work:
- Parent tree number-tree resolution for marked content
- Integration hooks from `PDDocumentCatalog`/`PDPage` to structure tree accessors
- Remaining documentinterchange classes needed to make tagged-tree access end-to-end

### Expected test scope
- Fixture test: load tagged PDF and walk from page marked-content IDs to structure elements.
- Verify parent-tree lookup stability for multi-page tagged documents.

### Entry criteria
- Issues #43–#45 merged and stable.

### Exit criteria
- End-to-end logical structure traversal from catalog + page APIs works.
- No documentinterchange integration stubs remain in touched areas.

### Risk register
- Parent-tree index mismatches can silently return wrong nodes.
- Integration touches multiple pdmodel layers and may expose hidden parser assumptions.

### Definition of done
- Build passes.
- End-to-end tagged traversal tests pass.
- Traceability statuses updated for integrated mappings.
