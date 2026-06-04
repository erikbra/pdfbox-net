# Issue 015 — Platform printing integration

## Summary

Implement .NET platform printing support so that the Printing examples compile and work on
supported platforms (Windows, and where possible Linux/macOS via a printing abstraction).

## Background

The upstream Java printing examples use `java.awt.print.*` (PrinterJob, Pageable, Printable).
In .NET, the equivalent is `System.Drawing.Printing` on Windows, or a cross-platform adapter.

## Required API surface

- `PDFPrintable` (or equivalent) — a `System.Drawing.Printing.IPrintPageEventHandler`-compatible
  adapter that renders a PDF page to a `Graphics` object
- `PDFPageable` (or equivalent) — adapts a multi-page `PDDocument` to a `PrintDocument`
- `PDFRenderer.RenderPageToGraphics(int pageIndex, Graphics graphics, float dpi)` — renders
  a single PDF page into a `System.Drawing.Graphics` context
- `PrinterJob` equivalent — wraps `System.Drawing.Printing.PrintDocument` for headless printing

## Affected example files

- `Printing/Printing.cs`
- `Printing/OpaquePDFRenderer.cs`

## Acceptance criteria

- Both files compile without stubs on the target platform.
- A basic print-to-file (print to PDF or XPS) integration test verifies that printing does not
  throw, on at least Windows (Linux/macOS may be skipped if no printing backend is available).
- Traceability rows for both affected source paths are `in-sync`.

## Notes

- Cross-platform printing may require a conditional implementation or a guard that skips on
  non-Windows platforms.
- `System.Drawing` is Windows-only unless `System.Drawing.Common` is used; evaluate whether
  a headless rasterize-and-print path is more appropriate.
