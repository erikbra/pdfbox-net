### Title
Implement `PDPageContentStream` color operators: `SetNonStrokingColor`, `SetStrokingColor`, and pattern/colorspace variants

### Summary

`PDPageContentStream` is missing all color-setting operators. The Java `PDPageContentStream` supports setting both non-stroking (fill) and stroking colours via overloads accepting `PDColor`, individual `float` component values, grayscale, RGB, CMYK, and colorspace-qualified variants. None of these are ported to the .NET `PDPageContentStream`.

### Missing operators

| PDF operator | Java method | Description |
|---|---|---|
| `g` | `setNonStrokingColor(float gray)` | Grayscale fill |
| `G` | `setStrokingColor(float gray)` | Grayscale stroke |
| `rg` | `setNonStrokingColor(float r, float g, float b)` | RGB fill |
| `RG` | `setStrokingColor(float r, float g, float b)` | RGB stroke |
| `k` | `setNonStrokingColor(float c, float m, float y, float k)` | CMYK fill |
| `K` | `setStrokingColor(float c, float m, float y, float k)` | CMYK stroke |
| `sc`/`scn` | `setNonStrokingColor(PDColor color)` | Colorspace fill |
| `SC`/`SCN` | `setStrokingColor(PDColor color)` | Colorspace stroke |
| `cs` | `setNonStrokingColorSpace(PDColorSpace colorSpace)` | Set fill colorspace |
| `CS` | `setStrokingColorSpace(PDColorSpace colorSpace)` | Set stroke colorspace |

### Affected example files (currently stubs)

- `PDModel/ShowColorBoxes.cs`
- `PDModel/AddMessageToEachPage.cs`
- `Util/AddWatermarkText.cs`
- `Util/PDFHighlighter.cs`
- `Util/PrintTextColors.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java`

### Acceptance criteria

- `SetNonStrokingColor` / `SetStrokingColor` overloads accepting grayscale, RGB, CMYK, and `PDColor` are implemented and emit the correct PDF operators.
- The corresponding colorspace-setting methods (`SetNonStrokingColorSpace`, `SetStrokingColorSpace`) are implemented.
- Example stubs in `PDModel/ShowColorBoxes.cs`, `PDModel/AddMessageToEachPage.cs`, `Util/AddWatermarkText.cs`, `Util/PDFHighlighter.cs`, and `Util/PrintTextColors.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
