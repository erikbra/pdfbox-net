# Issue 005 — `SetTextMatrix`, `SetCharacterSpacing`, `SetWordSpacing`

## Summary

Implement text-positioning and spacing operators on `PDPageContentStream` so that examples that
use fine-grained text layout compile and produce correct output.

## Required API surface

- `SetTextMatrix(Matrix matrix)` — emits the `Tm` operator
- `SetCharacterSpacing(float spacing)` — emits the `Tc` operator
- `SetWordSpacing(float spacing)` — emits the `Tw` operator

## Affected example files

- `PDModel/ShowTextWithPositioning.cs`
- `PDModel/UsingTextMatrix.cs`
- `PDModel/BengaliPdfGenerationHelloWorld.cs`

## Acceptance criteria

- All three operators are implemented and emit the correct PDF content-stream tokens.
- The three example files compile without stubs and produce valid PDF output.
- Integration tests verify that the output PDFs exist and contain expected text positioning.
- Traceability rows for all affected source paths are `in-sync`.
