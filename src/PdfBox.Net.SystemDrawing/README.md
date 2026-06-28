# PdfBox.Net.SystemDrawing

Optional Windows-only rendering backend that proves the core Java AWT/ImageIO
proxy layer can be backed by more than one concrete graphics implementation.

This package currently implements:

- `BufferedImage` pixel storage through `System.Drawing.Bitmap`
- `Graphics2D` basic clear, image draw, and transform operations through
  `System.Drawing.Graphics`
- Image decode/encode for PNG and JPEG through `System.Drawing.Image`
- Windows print-spooler integration for `PDFPrinter` through
  `System.Drawing.Printing`

It intentionally does not yet implement a full `PageDrawer` equivalent. Calling
PDF page rendering with this backend throws a clear `NotSupportedException`.
`PdfBox.Net.SkiaSharp` remains the complete cross-platform rendering backend.

Printing is split into two concerns:

- page rasterization/rendering, supplied by the currently registered
  `RenderingBackend` (use `PdfBox.Net.SkiaSharp` for full page rendering today)
- printer submission, supplied by `SystemDrawingPrintBackend.Register()`

For Windows printing with the current complete renderer:

```csharp
SkiaRenderingBackend.Register();
SystemDrawingPrintBackend.Register();
new PDFPrinter(document).Print();
```

On non-Windows platforms the System.Drawing print backend throws a clear
`PlatformNotSupportedException`. Core `PdfBox.Net` remains available without a
platform print dependency and can be paired with other future print backends.
