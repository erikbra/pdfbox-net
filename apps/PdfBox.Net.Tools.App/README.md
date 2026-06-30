# PdfBox.Net.Tools

.NET global tool facade for the Apache PDFBox command-line tools port.

Install:

```sh
dotnet tool install --global PdfBox.Net.Tools
```

Run:

```sh
pdfbox help
pdfbox export:text input.pdf output.txt
pdfbox render --help
```

The tool delegates to the ported `PdfBox.Net.Tools.PDFBox` dispatcher and
supports Apache PDFBox 3.0 command names such as `decode`, `export:text`,
`render`, `merge`, `split`, `overlay`, `fromimage`, and `fromtext`.

The Java Swing debugger UI is not packaged as this tool. Debugger parity is
provided by non-packable inspection models and can be wrapped by a separate UI
or application package later.
