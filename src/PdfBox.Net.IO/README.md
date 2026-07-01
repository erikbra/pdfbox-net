# PdfBox.Net.IO

Random-access IO primitives ported from Apache PDFBox's `pdfbox-io` module.

Most applications should reference the `PdfBox.Net.Core` package instead of
installing this package directly. It is published separately so the .NET package
graph mirrors Apache PDFBox's Java artifacts and so low-level consumers can use
the IO layer without the full PDF model.

Install:

```sh
dotnet add package PdfBox.Net.IO
```

This package provides the random-access read/write abstractions used by the PDF
parser, FontBox, and stream handling code.
