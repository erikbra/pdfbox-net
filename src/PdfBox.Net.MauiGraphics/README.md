# PdfBox.Net.MauiGraphics

Optional rendering backend that proves the core Java AWT/ImageIO proxy layer can
also be backed through `Microsoft.Maui.Graphics`.

`Microsoft.Maui.Graphics` is an abstraction library; bitmap export requires a
concrete implementation. This package uses `Microsoft.Maui.Graphics.Skia`, so it
is a Maui.Graphics backend rather than a Skia-free backend. The Skia dependency
is still isolated in this optional package and does not leak into PdfBox.Net
core APIs.

This package currently implements:

- `BufferedImage` pixel storage through `SkiaBitmapExportContext`
- `Graphics2D` basic clear, image draw, and transform operations on the
  Maui.Graphics bitmap export surface
- Image decode/encode for PNG and JPEG through Maui.Graphics `IImage`

It intentionally does not yet implement a full `PageDrawer` equivalent. Calling
PDF page rendering with this backend throws a clear `NotSupportedException`.
`PdfBox.Net.SkiaSharp` remains the complete cross-platform rendering backend.
