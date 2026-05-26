### Title
Port PDModel interactive slice A: utilities, names dictionaries, and viewer preferences

### Depends on
- #58 report hygiene and small-gap closeout
- #60 filter/parser/writer completeness for stable document loading

### Background
The interactive package needs dependency-ordered execution. The root utility and dictionary types
must land before actions, annotations, forms, and signatures can be layered on safely.

### Scope
- Port root interactive utilities:
  - `AppearanceStyle`
  - `PlainText`
  - `PlainTextFormatter`
  - `TextAlign`
- Port document-level support types needed by later slices:
  - `PDDocumentNameDictionary`
  - `PDEmbeddedFilesNameTreeNode`
  - `PDViewerPreferences`
- Align name-tree and viewer-preference integration with existing document/catalog behavior.

### Expected test scope
- Name-dictionary and embedded-file tree construction tests.
- Viewer-preference dictionary read/write tests.

### Entry criteria
- Current document load baseline is stable.

### Exit criteria
- Interactive slice A utilities and support dictionaries are ported and regression tested.
- Later interactive slices can depend on concrete types rather than placeholders.

### Risk register
- Name-tree behaviors depend on object resolution and catalog wiring.

### Definition of done
- `dotnet build` passes.
- Targeted interactive slice A tests pass.
- Traceability artifacts are updated for all touched files.
