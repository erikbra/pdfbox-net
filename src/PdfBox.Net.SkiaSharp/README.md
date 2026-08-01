# PdfBox.Net.SkiaSharp

Complete cross-platform SkiaSharp rendering and glyph layout backend for PdfBox.Net.

Install:

```sh
dotnet add package PdfBox.Net.SkiaSharp
```

Register the backend once at application startup before using `PDFRenderer` or
APIs that need bitmap/image encoding:

```csharp
using PdfBox.Net.Loader;
using PdfBox.Net.Rendering;

SkiaRenderingBackend.Register();

using PDDocument document = Loader.LoadPDF("input.pdf");
BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 144);
```

For text that needs glyph shaping, register the SkiaSharp glyph layout
processor on the content stream and load the Type 0 font through the processor:

```csharp
using PdfBox.Net.GlyphLayout.SkiaSharp;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;

using PDDocument document = new();
PDPage page = new();
document.AddPage(page);

using SkiaGlyphLayoutProcessor glyphLayout = new();
using Stream fontStream = File.OpenRead("NotoSans-Regular.ttf");
PDType0Font font = glyphLayout.LoadFont(document, fontStream,
    new SkiaGlyphLayoutProcessor.FontOptions()
        .SetKerningOn()
        .SetLigaturesOn());

using PDPageContentStream contents = new(document, page);
contents.BeginText();
contents.SetFont(font, 12);
contents.SetGlyphLayoutProcessor(glyphLayout);
contents.ShowText("AV office");
contents.EndText();
```

Glyph layout uses Unicode.Bidi and HarfBuzzSharp internally. These dependencies
stay in this optional backend package; `PdfBox.Net.Core` continues to expose
only the Java-shaped PDFBox glyph layout interfaces.

This shapes generated content streams that opt into the processor. Existing PDF
page rendering still follows the glyph codes and positions already present in
the PDF content stream. The processor resolves bidirectional visual runs with
Unicode.Bidi before shaping each run with HarfBuzz.

This package intentionally owns the SkiaSharp dependency so the core
`PdfBox.Net.Core` package can keep Java-shaped graphics proxy APIs without
exposing SkiaSharp types directly.
