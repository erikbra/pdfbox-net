# PdfBox.Net.ImageMagick

Optional Magick.NET-backed image and color providers for PdfBox.Net.

Install:

```sh
dotnet add package PdfBox.Net.ImageMagick
```

Register the provider once at application startup:

```csharp
using PdfBox.Net.ImageMagick;

PdfBoxNetImageMagick.Register();
```

This package provides the heavier image/color features that are intentionally
kept out of `PdfBox.Net.Core`:

- JPXDecode / JPEG 2000 image decoding
- CMYK/YCCK JPEG DCTDecode raster decoding
- TIFF import for `CCITTFactory`
- ICC color profile transforms

For full rendering, use `PdfBox.Net.Rendering`, which registers both this
package and the SkiaSharp rendering backend.
