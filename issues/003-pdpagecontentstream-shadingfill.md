### Title
Implement `PDPageContentStream.ShadingFill` — shading fill operator (`sh`)

### Summary

`PDPageContentStream` is missing the `ShadingFill(PDShadingResources)` method, which emits the `sh` operator after registering the shading resource on the page. This is required to render gradient-fill effects using PDF shading dictionaries.

### Missing method

| Java method | PDF operator | Description |
|---|---|---|
| `shadingFill(PDShadingResources shading)` | `sh` | Fill current clipping region with a shading |

### Affected example files (currently stubs)

- `PDModel/CreateGradientShadingPDF.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java`

### Acceptance criteria

- `ShadingFill(PDShadingResources shading)` is implemented, registers the shading on the page resources, and emits `sh`.
- `PDModel/CreateGradientShadingPDF.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
