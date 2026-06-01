### Title
Implement platform printing integration for the `Printing` examples module

### Summary

The two `Printing/` examples use Java AWT printing (`java.awt.print.PrinterJob`, `java.awt.print.Printable`) which has no direct cross-platform .NET equivalent. The .NET `PDFPrinter` / `PDFPageable` classes are not yet ported.

### Affected example files (currently stubs)

- `Printing/Printing.cs` — demonstrates printing a PDF via the OS print subsystem.
- `Printing/OpaquePDFRenderer.cs` — demonstrates printing with an opaque background using custom `Graphics2D` rendering.

### Recommended .NET equivalent

- `System.Drawing.Printing.PrintDocument` / `PrinterSettings` for Windows.
- For cross-platform printing, investigate `Microsoft.Maui.Essentials` printing or third-party libraries.

The port should provide a clean abstraction matching the upstream `PDFPrinter` API surface so that the examples can be upgraded to `PORT_MODE: mechanical`.

### Upstream Java reference

`examples/src/main/java/org/apache/pdfbox/examples/printing/`
`pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrinter.java`
`pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPageable.java`
`pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrintable.java`

### Acceptance criteria

- `PDFPrinter`, `PDFPageable`, and `PDFPrintable` are ported (with a Windows-primary implementation and no-op/stub for non-Windows).
- `Printing/Printing.cs` and `Printing/OpaquePDFRenderer.cs` are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
