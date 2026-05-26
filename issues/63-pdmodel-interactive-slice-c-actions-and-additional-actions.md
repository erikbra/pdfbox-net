### Title
Port PDModel interactive slice C: actions and additional-actions dictionaries

### Depends on
- #62 interactive slice B

### Background
Action handling fans out broadly across document catalog, pages, annotations, forms, and embedded
targets. It needs its own slice before annotation and form integration.

### Scope
- Port the remaining action classes and factories, including:
  - go-to / remote / embedded navigation actions
  - URI, launch, JavaScript, named, movie, sound, hide, import-data, submit/reset-form actions
  - target and URI dictionaries
  - Windows launch parameters
- Port all additional-actions dictionaries for document, page, annotation, and form field paths.

### Expected test scope
- Action-factory dispatch tests.
- Dictionary read/write tests for URI, GoTo, and submit/reset actions.

### Entry criteria
- Slice B merged and green.

### Exit criteria
- Action factories resolve covered action dictionaries deterministically.
- Additional-actions dictionaries are mapped and usable by later slices.

### Risk register
- Some actions reference embedded targets, forms, or annotations not yet completed.

### Definition of done
- `dotnet build` passes.
- Action-focused tests pass.
- Traceability artifacts are updated.
