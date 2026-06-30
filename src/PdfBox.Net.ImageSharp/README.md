# PdfBox.Net.ImageSharp

Optional rendering backend that proves the core Java AWT/ImageIO proxy layer can
also be backed by `SixLabors.ImageSharp`.

This package intentionally depends only on `SixLabors.ImageSharp` 3.1.x. It does
not reference `SixLabors.ImageSharp.Drawing` because the current 3.x Drawing
package requires a Six Labors license key during build. ImageSharp remains under
the Six Labors split license, so this backend should stay optional and should
not become a core dependency without an explicit project licensing decision.

This package currently implements:

- `BufferedImage` pixel storage through `Image<Rgba32>`
- `Graphics2D` basic clear, image draw, and transform operations through
  ImageSharp pixels
- Image decode/encode for PNG and JPEG through ImageSharp codecs

It intentionally does not yet implement a full `PageDrawer` equivalent. Calling
PDF page rendering with this backend throws a clear `NotSupportedException`.
`PdfBox.Net.SkiaSharp` remains the complete cross-platform rendering backend.

Install:

```sh
dotnet add package PdfBox.Net.ImageSharp
```

Register the backend once at application startup when you want ImageSharp to
back the Java AWT/ImageIO proxy APIs for supported image operations.
