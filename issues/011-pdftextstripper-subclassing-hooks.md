# Issue 011 — `PDFTextStripper` subclassing hooks

## Summary

Expose protected subclassing hooks on `PDFTextStripper` so that examples that extend it to
intercept per-character, per-word, or per-line text positions compile and work correctly.

## Required API surface

- `WriteString(string text, IList<TextPosition> textPositions)` — virtual/override hook called
  for each text run with per-character position data
- `WriteLineSeparator()` — virtual hook called at line boundaries
- `WriteWordSeparator()` — virtual hook called between words
- `TextPosition` properties accessible in subclasses:
  - `GetX()` / `GetY()` — baseline position
  - `GetXDirAdj()` / `GetYDirAdj()` — direction-adjusted position
  - `GetWidth()` / `GetWidthDirAdj()` — glyph width
  - `GetHeight()` / `GetHeightDir()` — glyph height
  - `GetFontSizeInPt()` — effective font size in points
  - `GetUnicode()` — unicode string

## Affected example files

- `Util/DrawPrintTextLocations.cs`

## Acceptance criteria

- `DrawPrintTextLocations` compiles without stubs.
- When run against a sample PDF, it produces a PDF with text-location rectangles drawn over
  each character or word.
- Integration test verifies that the output PDF exists and is non-empty.
- Traceability row for the affected source path is `in-sync`.
