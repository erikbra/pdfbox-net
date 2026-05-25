### Title
Complete graphics-state and XObject integration in PDModel/graphics

### Background
Once graphics resource types are complete, state and XObject integration should be hardened so
content-stream execution and downstream rendering consume real graphics objects end-to-end.

### Depends on
- #54 Patterns, optional-content, and inline-image completion

### Scope
- Complete parity-critical behavior in `PDExtendedGraphicsState`, `PDGraphicsState`,
  `PDSoftMask`, and related integration points.
- Harden `PDFormXObject`, `PDTransparencyGroup`, and `PDImageXObject` integration behaviors
  used by existing content-stream operator processors.
- Resolve remaining graphics-model placeholders encountered during integration tests.

### Expected test scope
- Graphics-state dictionary application tests (line styles, alpha, blend mode paths).
- XObject/form/transparency integration tests with deterministic fixture PDFs.

### Entry criteria
- #54 complete and green.

### Exit criteria
- Graphics state + XObject paths are deterministic and regression tested for milestone scenarios.
- No unresolved stub-only behavior remains in touched integration paths.

### Risk register
- Integration spans multiple packages (`contentstream`, `pdmodel`, `rendering`) and can fan out.
- Fixture selection must cover both form XObject and transparency-group paths.

### Definition of done
- Build + targeted graphics/contentstream tests pass.
- Conversion/traceability records updated for all changed mappings.
