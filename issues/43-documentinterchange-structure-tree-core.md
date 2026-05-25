### Title
Port PDModel DocumentInterchange structure tree core

### Background
`org.apache.pdfbox.pdmodel.documentinterchange` is still the least-converted major
`pdfbox` package. Only `PDMarkedContent` is currently ported, leaving the logical
structure tree foundation for tagged PDFs mostly unimplemented.

### Depends on
- Issue #31 (full PDF loading)
- COS + parser milestones (#37–#41) already merged

### Scope
Port the structure-tree core classes first so higher-level tagged PDF features have a stable base:
- `PDStructureTreeRoot`
- `PDStructureNode`
- `PDStructureElement`
- `PDMarkedContent`

Include dictionary/array wiring, revision tracking fields, and parent/child traversal
helpers needed by downstream documentinterchange classes.

### Expected test scope
- Parse a fixture with structure tree root and verify child element hierarchy.
- Validate basic read/write of structure element type/children in COS dictionaries.

### Entry criteria
- `dotnet build` and `dotnet test` are green on current mainline.

### Exit criteria
- Structure tree root/node/element classes compile and are usable from `PDDocumentCatalog`.
- Core structure tree traversal tests pass.

### Risk register
- Parent/child cycles in structure trees can cause accidental recursion.
- COS array vs dictionary shape variance across PDFs requires defensive parsing.

### Definition of done
- Build passes.
- Targeted structure-tree tests pass.
- Conversion + traceability report rows updated.
