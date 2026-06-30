# PdfBox.Net.SkiaSharp

Complete cross-platform SkiaSharp rendering backend for PdfBox.Net.

Install:

```sh
dotnet add package PdfBox.Net.SkiaSharp
```

Register the backend once at application startup before using `PDFRenderer` or
APIs that need bitmap/image encoding:

```csharp
using PdfBox.Net.Loader;
using PdfBox.Net.Rendering;
using PdfBox.Net.SkiaSharp.Rendering;

SkiaRenderingBackend.Register();

using PDDocument document = Loader.LoadPDF("input.pdf");
BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 144);
```

This package intentionally owns the SkiaSharp dependency so the core
`PdfBox.Net` package can keep Java-shaped graphics proxy APIs without exposing
SkiaSharp types directly.
