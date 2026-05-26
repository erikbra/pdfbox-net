### Title
Port PDModel interactive slice B: destinations, outlines, and page navigation

### Depends on
- #61 interactive slice A
- #35 PDModel/common completeness

### Background
Destinations and outline/navigation types provide the structural base for links, bookmarks, and
 document navigation features used by actions and annotations.

### Scope
- Port destination types and factories needed by outline and action paths.
- Replace existing outline/thread stubs with real implementations:
  - `PDDocumentOutline`
  - `PDOutlineNode`
  - `PDOutlineItem`
  - `PDThread`
  - `PDThreadBead`
  - `PDTransition`
- Complete related page-navigation wiring used by named and explicit destinations.

### Expected test scope
- Bookmark traversal tests against fixture PDFs.
- Destination and thread-bead dictionary parsing tests.

### Entry criteria
- Slice A merged and green.

### Exit criteria
- Outline traversal and destination resolution work on deterministic fixtures.
- Stub-only outline/navigation behaviors are removed from touched paths.

### Risk register
- Destination parsing is tightly coupled to page-tree/document resolution.

### Definition of done
- `dotnet build` passes.
- Outline/navigation tests pass.
- Traceability artifacts are updated.
