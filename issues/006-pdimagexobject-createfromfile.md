### Title
Implement `PDImageXObject.CreateFromFile` — image loading factory from file path

### Summary

`PDImageXObject` has no factory method that loads an image from a file path and picks the correct codec (JPEG, TIFF, PNG, etc.) automatically. The Java `PDImageXObject.createFromFile(String filename, PDDocument doc)` inspects the file extension and MIME type and delegates to the appropriate sub-factory (`JPEGFactory`, `CCITTFactory`, `LosslessFactory`, etc.).

### Missing API

```csharp
// Target signature
public static PDImageXObject CreateFromFile(string imagePath, PDDocument document)
```

This should inspect the file extension / magic bytes and delegate to the appropriate loader:
- JPEG → `JPEGFactory.CreateFromStream`
- TIFF → `CCITTFactory.CreateFromFile` (for Group 4 fax) or `LosslessFactory`
- PNG/BMP/GIF → `LosslessFactory.CreateFromImage`

### Affected example files (currently stubs)

- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDImageXObject.java`

### Acceptance criteria

- `PDImageXObject.CreateFromFile(string imagePath, PDDocument document)` is implemented and correctly dispatches to the appropriate image codec.
- Example stubs in `PDModel/AddImageToPDF.cs`, `PDModel/ImageToPDF.cs`, and `PDModel/RubberStampWithImage.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical` (together with issue #002 `DrawImage`).
