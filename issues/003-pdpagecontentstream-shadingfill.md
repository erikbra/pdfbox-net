# Issue 003 — `PDPageContentStream.ShadingFill`

## Summary

Implement `PDPageContentStream.ShadingFill(PDShading)` so that gradient/shading examples can fill
areas with shading patterns.

## Required API surface

- `ShadingFill(PDShading shading)` — emits the `sh` operator with the shading resource name

## Affected example files

- `PDModel/CreateGradientShadingPDF.cs`

## Acceptance criteria

- `ShadingFill` is implemented and emits the correct `sh` operator.
- `CreateGradientShadingPDF` compiles without stubs and produces a valid PDF containing a gradient.
- Integration test verifies the output PDF exists and is loadable.
- Traceability row for the affected source path is `in-sync`.
