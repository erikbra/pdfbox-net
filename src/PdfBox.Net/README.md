# PdfBox.Net.Core

Core .NET port of Apache PDFBox for working with PDF documents.

This package contains the main PDF model, COS objects, parser, filters, writer,
text extraction, rendering entry points, form handling, annotations, and
standard-security support. The package ID is `PdfBox.Net.Core`; the assembly
and namespaces remain `PdfBox.Net`. It depends on `PdfBox.Net.IO` and
`PdfBox.Net.FontBox`, matching the Java PDFBox module split.

Install:

```sh
dotnet add package PdfBox.Net.Core
```

Typical usage:

```csharp
using PdfBox.Net.Loader;
using PdfBox.Net.Text;

using PDDocument document = Loader.LoadPDF("input.pdf");
string text = new PDFTextStripper().GetText(document);
```

Rendering needs a concrete backend package. Reference `PdfBox.Net.Rendering`
for the supported full stack and register it at process startup. That package
registers both the SkiaSharp renderer and the ImageMagick-backed image/color
providers for JPX/JPEG2000, CMYK JPEG, TIFF import, and ICC conversion.

Applications that only parse, inspect, edit, save, or extract text can use
`PdfBox.Net.Core` without the rendering package. Public-key encrypted PDFs need
`PdfBox.Net.Cryptography`.
