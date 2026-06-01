### Title
Implement `PDPageContentStream.DrawImage` — image drawing operator

### Summary

`PDPageContentStream` does not have a `DrawImage` method. The Java `PDPageContentStream.drawImage(PDImageXObject, float x, float y)` and its overloads (with explicit width/height, and with `Matrix` for full affine placement) emit the `Do` operator after pushing the image onto the resource dictionary. Without this, no raster image can be placed on a page.

### Missing methods

| Java method | Description |
|---|---|
| `drawImage(PDImageXObject image, float x, float y)` | Draw image at natural size at `(x, y)` |
| `drawImage(PDImageXObject image, float x, float y, float width, float height)` | Draw image scaled to `width × height` |
| `drawImage(PDImageXObject image, Matrix matrix)` | Draw image with full affine transform |

### Affected example files (currently stubs)

- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java`

### Acceptance criteria

- `DrawImage(PDImageXObject, float, float)`, `DrawImage(PDImageXObject, float, float, float, float)`, and `DrawImage(PDImageXObject, Matrix)` are implemented and emit a `q … cm … Do Q` sequence with the image resource registered on the page.
- Example stubs in `PDModel/AddImageToPDF.cs`, `PDModel/ImageToPDF.cs`, and `PDModel/RubberStampWithImage.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
