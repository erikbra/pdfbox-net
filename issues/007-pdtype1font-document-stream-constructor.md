### Title
Implement `PDType1Font(PDDocument, Stream)` constructor for custom Type 1 font loading

### Summary

The .NET `PDType1Font` only supports the Standard-14 fonts (via `PDType1Font(FontName)`) and direct dictionary construction (`PDType1Font(COSDictionary)`). It has no constructor that accepts a `PDDocument` and a `Stream` containing a PFB/PFM font file, embeds the font data, and builds the required font metrics dictionary.

The Java constructor `new PDType1Font(PDDocument document, InputStream pfbStream)` embeds the raw Type 1 font binary, builds the `Widths` array, and populates the font descriptor.

### Missing API

```csharp
// Target signature
public PDType1Font(PDDocument document, Stream pfbStream)
```

### Affected example files (currently stubs)

- `PDModel/HelloWorldType1.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java`

### Acceptance criteria

- `PDType1Font(PDDocument document, Stream pfbStream)` is implemented, embeds the PFB font program in the PDF, and constructs a valid font dictionary with widths and descriptor.
- `PDModel/HelloWorldType1.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
