### Title
Port PDModel/FDF field, page, and template model

### Depends on
- #69 FDF document core

### Background
Once the FDF document shell is present, the field/page/template model can be added without mixing
annotation mirror work into the same slice.

### Scope
- Port:
  - `FDFField`
  - `FDFPage`
  - `FDFPageInfo`
  - `FDFTemplate`
  - `FDFOptionElement`
  - `FDFIconFit`
- Align FDF field/page structures with existing interactive form conventions where relevant.

### Expected test scope
- FDF field hierarchy tests.
- Page/template dictionary tests.

### Entry criteria
- #69 merged and green.

### Exit criteria
- FDF field/page/template model is mapped and regression tested.

### Risk register
- FDF field semantics overlap with incomplete or partially adapted AcroForm behavior.

### Definition of done
- `dotnet build` passes.
- Targeted FDF field/page tests pass.
- Traceability artifacts are updated.
