# Issue 013 — `PDTrueTypeFont.ExportFont()`

## Summary

Implement `PDTrueTypeFont.ExportFont()` so that examples that extract and re-export embedded
TrueType fonts from PDFs compile and produce correct output.

## Required API surface

- `PDTrueTypeFont.ExportFont()` — returns the raw TrueType font bytes (the embedded font
  program stream), allowing the font to be written back to disk as a `.ttf` file

## Affected example files

- `PDModel/ExtractTTFFonts.cs`

## Acceptance criteria

- `ExportFont()` returns the raw bytes of the embedded TrueType font program.
- `ExtractTTFFonts` compiles without stubs and, when run against a sample PDF with embedded
  fonts, writes valid `.ttf` files to the output directory.
- Integration test verifies that the exported font files are non-empty and have a valid
  TrueType header.
- Traceability row for the affected source path is `in-sync`.
