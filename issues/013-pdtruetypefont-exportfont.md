### Title
Implement `PDTrueTypeFont.ExportFont()` — extract raw font binary from embedded TrueType font

### Summary

The Java `PDTrueTypeFont` provides a method to extract the raw TrueType font binary (the embedded font program) from a PDF's font descriptor. The .NET `PDTrueTypeFont` is missing this method.

### Missing method

```csharp
// Java: byte[] extractFontFile(PDDocument doc) — returns raw TrueType bytes from the /FontFile2 stream
public byte[] ExportFont()
```

### Affected example files (currently stubs)

- `PDModel/ExtractTTFFonts.cs` — partially: the file-enumeration and font-type-detection logic works, but the final `font.ExportFont()` call is stubbed.

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFont.java`
`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2.java` (CIDFont case)

### Acceptance criteria

- `PDTrueTypeFont.ExportFont()` (or equivalent) returns the raw TrueType/OpenType bytes embedded in the font's `/FontFile2` (or `/FontFile3`) stream.
- `PDModel/ExtractTTFFonts.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
