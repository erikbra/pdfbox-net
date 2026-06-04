# Issue 002 — `PDPageContentStream.DrawImage`

## Summary

Implement `PDPageContentStream.DrawImage` overloads so that image-embedding examples can draw
raster images onto a PDF page.

## Required API surface

- `DrawImage(PDImageXObject image, float x, float y)`
- `DrawImage(PDImageXObject image, float x, float y, float width, float height)`
- `DrawImage(PDImageXObject image, Matrix matrix)`

## Affected example files

- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

## Acceptance criteria

- All three `DrawImage` overloads are implemented and emit correct `Do` operators with the image
  XObject reference.
- The three example files compile without stubs and produce valid PDF output containing the image.
- Integration tests verify that the image is embedded and referenced correctly.
- Traceability rows for all affected source paths are `in-sync`.
