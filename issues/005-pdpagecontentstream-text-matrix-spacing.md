### Title
Implement `PDPageContentStream` text matrix and character/word spacing operators

### Summary

`PDPageContentStream` is missing three text-mode operators:

| PDF operator | Java method | Description |
|---|---|---|
| `Tm` | `setTextMatrix(Matrix matrix)` | Set the text and text line matrices — controls arbitrary text placement and rotation |
| `Tc` | `setCharacterSpacing(float spacing)` | Horizontal character spacing in text space |
| `Tw` | `setWordSpacing(float spacing)` | Additional spacing added after space characters |

Without `SetTextMatrix`, examples that position text at arbitrary locations or rotate it cannot be ported. Without `SetCharacterSpacing`/`SetWordSpacing`, kerning/tracking examples cannot be ported.

### Affected example files (currently stubs)

- `PDModel/ShowTextWithPositioning.cs`
- `PDModel/UsingTextMatrix.cs`
- `PDModel/BengaliPdfGenerationHelloWorld.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java`

### Acceptance criteria

- `SetTextMatrix(Matrix matrix)` is implemented and emits `Tm` with the six matrix coefficients.
- `SetCharacterSpacing(float spacing)` is implemented and emits `Tc`.
- `SetWordSpacing(float spacing)` is implemented and emits `Tw`.
- Example stubs in `PDModel/ShowTextWithPositioning.cs`, `PDModel/UsingTextMatrix.cs`, and `PDModel/BengaliPdfGenerationHelloWorld.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
