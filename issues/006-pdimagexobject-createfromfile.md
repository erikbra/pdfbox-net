# Issue 006 — `PDImageXObject.CreateFromFile`

## Summary

Implement `PDImageXObject.CreateFromFile(string imagePath, PDDocument document)` so that examples
that load raster images from disk can embed them into PDFs without stubs.

## Required API surface

- `PDImageXObject.CreateFromFile(string imagePath, PDDocument document)` — auto-detects format
  (JPEG, PNG, TIFF, BMP, GIF) by file extension and delegates to the appropriate factory
- Underlying image factories must be functional:
  - `JPEGFactory.CreateFromFile` — JPEG
  - `LosslessFactory.CreateFromImage` — PNG / BMP / GIF
  - `CCITTFactory.CreateFromFile` — TIFF (optional for basic support)

## Affected example files

- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

## Acceptance criteria

- `PDImageXObject.CreateFromFile` successfully loads at least JPEG and PNG files.
- The three example files compile without stubs and produce valid PDFs containing the embedded image.
- Integration tests include at least one JPEG and one PNG fixture.
- Traceability rows for all affected source paths are `in-sync`.
