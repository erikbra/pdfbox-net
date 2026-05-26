### Title
Port PDModel/fixup core and processor classes

### Depends on
- #65 interactive AcroForm core and field tree
- #66 annotation appearance handlers and generation

### Background
`pdmodel.fixup` depends on completed form and appearance infrastructure and is small enough to land
as a focused post-AcroForm milestone.

### Scope
- Port:
  - `AbstractFixup`
  - `AcroFormDefaultFixup`
  - `PDDocumentFixup`
  - processor base and AcroForm processor classes
- Align fixup execution with completed AcroForm and widget behavior.

### Expected test scope
- Processor dispatch and document-fixup tests on deterministic form fixtures.

### Entry criteria
- AcroForm and appearance-generation slices merged and green.

### Exit criteria
- `pdmodel.fixup` reaches 8 / 8 mapped for the current parity target.

### Risk register
- Processor behavior can expose latent form-default and orphan-widget edge cases.

### Definition of done
- `dotnet build` passes.
- Fixup-targeted tests pass.
- Traceability artifacts are updated.
