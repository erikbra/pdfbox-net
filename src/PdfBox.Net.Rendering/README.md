# PdfBox.Net.Rendering

Convenience package for the supported PdfBox.Net rendering stack.

Install:

```sh
dotnet add package PdfBox.Net.Rendering
```

Register once at application startup:

```csharp
using PdfBox.Net.Rendering;

PdfBoxNetRendering.Register();
```

This registers:

- `PdfBox.Net.SkiaSharp` for page rendering, bitmap creation, and image codec support
- `PdfBox.Net.ImageMagick` for JPX/JPEG2000, CMYK JPEG, TIFF import, and ICC color conversion

Applications that only parse, inspect, edit, save, or extract text from PDFs can
use `PdfBox.Net.Core` directly without referencing this package.
