### Title
Port PDModel interactive slice E: AcroForm core and field tree types

### Depends on
- #64 interactive slice D
- #59 font backfill after closeout

### Background
Forms are one of the largest real-world interactive feature areas and depend on completed action,
annotation, and font infrastructure.

### Scope
- Port the remaining AcroForm and field-tree classes:
  - `PDAcroForm`
  - field factory/tree support
  - button, choice, combo, list, checkbox, text, and related field types
  - default-appearance support types required to parse field appearance strings
- Keep form behavior focused on structural parity first, with appearance generation deferred to the
  next slice.

### Expected test scope
- Field-tree traversal tests.
- Form dictionary/property tests for text, button, and choice fields.

### Entry criteria
- Slice D merged and green.

### Exit criteria
- Core AcroForm and field-tree types are mapped and usable for read/write structure tests.
- Field factory dispatch works for the covered baseline field set.

### Risk register
- Appearance/default-appearance plumbing can expose content-stream dependencies.

### Definition of done
- `dotnet build` passes.
- AcroForm core tests pass.
- Traceability artifacts are updated.
