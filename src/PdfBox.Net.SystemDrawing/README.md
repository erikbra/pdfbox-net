# PdfBox.Net.SystemDrawing

Optional Windows-only rendering backend that proves the core Java AWT/ImageIO
proxy layer can be backed by more than one concrete graphics implementation.

This package currently implements:

- `BufferedImage` pixel storage through `System.Drawing.Bitmap`
- `Graphics2D` basic clear, image draw, and transform operations through
  `System.Drawing.Graphics`
- Image decode/encode for PNG and JPEG through `System.Drawing.Image`

It intentionally does not yet implement a full `PageDrawer` equivalent. Calling
PDF page rendering with this backend throws a clear `NotSupportedException`.
`PdfBox.Net.SkiaSharp` remains the complete cross-platform rendering backend.
