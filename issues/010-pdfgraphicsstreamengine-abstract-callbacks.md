### Title
Implement `PDFGraphicsStreamEngine` / `PageDrawer` abstract callback API for custom rendering engines

### Summary

The Java `PDFGraphicsStreamEngine` exposes abstract callback methods that subclasses override to intercept individual graphics operations during content stream processing. The .NET `PDFGraphicsStreamEngine` and `PageDrawer` classes exist but do not expose these extension points publicly.

### Missing abstract/virtual methods on `PDFGraphicsStreamEngine`

| Java method | Description |
|---|---|
| `appendRectangle(Point2D p0, p1, p2, p3)` | Intercept rectangle construction |
| `drawImage(PDImage pdImage)` | Intercept image draw |
| `clip(int windingRule)` | Intercept clipping path |
| `moveTo(float x, float y)` | Intercept path moveto |
| `lineTo(float x, float y)` | Intercept path lineto |
| `curveTo(float x1, y1, x2, y2, x3, y3)` | Intercept Bezier curve |
| `getCurrentPoint()` | Return current path position |
| `closePath()` | Intercept path close |
| `endPath()` | Intercept path end (no fill/stroke) |
| `strokePath()` | Intercept stroke operation |
| `fillPath(int windingRule)` | Intercept fill operation |
| `fillAndStrokePath(int windingRule)` | Intercept combined fill+stroke |
| `shadingFill(COSName shadingName)` | Intercept shading fill |

### Affected example files (currently stubs)

- `Rendering/CustomGraphicsStreamEngine.cs`
- `Rendering/CustomPageDrawer.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/rendering/PDFGraphicsStreamEngine.java`
`pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java`

### Acceptance criteria

- All listed abstract/virtual methods are exposed on the .NET `PDFGraphicsStreamEngine`.
- `PageDrawer` provides concrete implementations that subclasses can override.
- `Rendering/CustomGraphicsStreamEngine.cs` and `Rendering/CustomPageDrawer.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
