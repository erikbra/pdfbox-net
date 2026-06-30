# PdfBox.Net.FontBox

Font parsing and metrics support ported from Apache FontBox.

The package includes support for AFM, CFF, CMap, TrueType, OpenType, and Type 1
font data used by PdfBox.Net text extraction, rendering, and PDF creation.

Install:

```sh
dotnet add package PdfBox.Net.FontBox
```

Most PDF workloads should reference `PdfBox.Net`, which depends on this package
automatically. Install `PdfBox.Net.FontBox` directly when you need the font
parsing APIs without the full PDF document model.
