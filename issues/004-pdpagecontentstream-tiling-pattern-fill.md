### Title
Implement `PDPageContentStream` tiling pattern fill operators

### Summary

`PDPageContentStream` is missing support for tiling pattern fills. The Java API allows registering a `PDTilingPattern` on the page resources and then using it as a fill color via `setPatternColorSpace` / `setNonStrokingColorWithPattern`. This requires:

1. `PDResources.Add(PDTilingPattern pattern)` — register a tiling pattern resource and return its resource name.
2. `PDPageContentStream.SetNonStrokingColorWithPattern(PDColorSpace patternCS, COSName patternName)` — emit `cs scn` with the pattern name.
3. `PDPageContentStream.SetStrokingColorWithPattern(PDColorSpace patternCS, COSName patternName)` — emit `CS SCN` with the pattern name.

### Affected example files (currently stubs)

- `PDModel/CreatePatternsPDF.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java`
`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDResources.java`

### Acceptance criteria

- `PDResources.Add(PDTilingPattern)` is implemented and returns the auto-generated resource name.
- `SetNonStrokingColorWithPattern` / `SetStrokingColorWithPattern` are implemented and emit `cs`/`scn` (or `CS`/`SCN`) with the pattern colorspace and name.
- `PDModel/CreatePatternsPDF.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
