### Title
Implement `PDFTextStripper` subclassing hooks: `WriteString` and `ProcessTextPosition`

### Summary

The Java `PDFTextStripper` exposes two protected overridable methods that subclasses use to intercept text extraction at different granularities:

- `writeString(String string, List<TextPosition> textPositions)` — called for each run of characters that have been sorted and assembled into a logical string.
- `processTextPosition(TextPosition text)` — called once per individual `TextPosition` glyph, before any sorting or assembly.

These methods let subclasses record the bounding box of each character for tasks such as drawing highlight rectangles, extracting word-level bounding boxes, or building structured text reports.

The .NET `PDFTextStripper` base class does not expose these extension points.

### Missing protected virtual methods

```csharp
// Called with each assembled string run and its TextPosition list
protected virtual void WriteString(string @string, List<TextPosition> textPositions)

// Called for each individual glyph
protected virtual void ProcessTextPosition(TextPosition text)
```

### Affected example files (currently stubs)

- `Util/DrawPrintTextLocations.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripper.java`

### Acceptance criteria

- `PDFTextStripper.WriteString(string, List<TextPosition>)` and `ProcessTextPosition(TextPosition)` are `protected virtual` and called at the appropriate points in the text extraction pipeline.
- `Util/DrawPrintTextLocations.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
