### Title
Replace AwtStubs with real .NET graphics rendering

### Background
`src/PdfBox.Net/Rendering/AwtStubs.cs` provides empty placeholder types for Java AWT
classes (`BufferedImage`, `Graphics2D`, `Color`, `Shape`, `Raster`, `WritableRaster`, etc.)
that allow the rendering code to compile but produce no actual output.

Until these are replaced with real .NET graphics API calls, `PDFRenderer.renderImage()` will
not produce any rasterized pixel output. This is a prerequisite for PDF-to-image conversion
and any visual validation of the rendered output.

### Depends on
- Rendering layer logic (already ported: `PDFRenderer`, `PageDrawer`, `GroupGraphics`, etc.)
- Filter layer (complete) — for decoding image streams
- Issue #31 (full PDF loading) — to have a real document to render
- Partial dependency on issue #32 (PDModel/Interactive) for annotations rendering

### Scope

Choose a .NET-compatible 2D graphics backend and adapt the rendering classes to use it.
Recommended options (in order of preference):

1. **System.Drawing.Common** — available via NuGet, familiar AWT-like API, wide
   existing test infrastructure in .NET world. Works on Linux/macOS via libgdiplus.
2. **SkiaSharp** — cross-platform, actively maintained, performant; closer to a
   canvas model than AWT but well-suited for PDF rendering.
3. **Microsoft.Maui.Graphics** — newest, cross-platform, but less mature.

Adaptation steps:
1. Choose the backend and add the NuGet dependency to `PdfBox.Net.csproj`.
2. Create a `DotNetGraphics2D` adapter that wraps the chosen backend and implements
   the same interface currently provided by `AwtStubs.cs / Graphics2D`.
3. Update `PageDrawer.cs` and `GroupGraphics.cs` to use the real adapter.
4. Update `PDFRenderer.RenderImage()` to return a real image type (e.g. `SKBitmap`
   or `System.Drawing.Bitmap`).
5. Delete or stub-out `AwtStubs.cs` once all usages are replaced.
6. Add `BufferedImage`-equivalent output to `PDFRenderer.renderImageWithDPI()`.

### Expected test scope
- Render a simple single-page text PDF to a bitmap and verify non-blank pixel output.
- Render a PDF with an embedded image and verify image dimensions.
- Compare output against a known-good rasterized reference image (regression test).

### Entry criteria
- Issue #31 (PDF loading) functional.
- `dotnet build` passes with current AwtStubs.

### Exit criteria
- `PDFRenderer.renderImage()` produces a real bitmap with correct dimensions.
- At least one rendering smoke test passes with non-blank output.
- `AwtStubs.cs` is replaced or repurposed.

### Risk register
- System.Drawing.Common requires libgdiplus on Linux; SkiaSharp avoids this.
- PageDrawer is large; adaptation may be incremental — some rendering paths may stay
  no-op until shading/pattern/transparency group support is complete.
- Shading fills (sh operator) require PDShading implementations (issue #22 scope).

### Definition of done
- `dotnet build` passes.
- Rendering smoke test produces a non-blank bitmap.
- NuGet dependency added with version pinned in `Directory.Packages.props`.
- Advisory DB check run for any new NuGet package.
