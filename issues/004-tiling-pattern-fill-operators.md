# Issue 004 — Tiling pattern fill operators

## Summary

Implement tiling pattern color space and fill support in `PDPageContentStream` so that examples
that draw with tiling patterns compile and produce correct output.

## Required API surface

- `PDTilingPattern` — tiling pattern resource (bounding box, xStep, yStep, paint type, tiling type)
- `PDColorSpace` with pattern support — `PDPattern`, `PDUncoloredTilingPattern`, `PDColoredTilingPattern`
- `PDPageContentStream` pattern color operators:
  - `SetNonStrokingColorWithPattern(PDColorSpace, COSName)`
  - `SetStrokingColorWithPattern(PDColorSpace, COSName)`
- Ability to register a tiling pattern as a page resource

## Affected example files

- `PDModel/CreatePatternsPDF.cs`

## Acceptance criteria

- `CreatePatternsPDF` compiles without stubs and produces a valid PDF with tiling patterns.
- Integration test verifies the output PDF exists and is loadable.
- Traceability row for the affected source path is `in-sync`.
