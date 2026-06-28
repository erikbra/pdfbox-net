# Printing Backend Policy

Generated UTC: 2026-06-28

Issue: #529

## Current State

`PdfBox.Net.Printing.PDFPrintable` and `PDFPageable` remain the Java-shaped
printing model classes. They are platform-neutral and keep the upstream scaling,
orientation, page-format, and raster-DPI behavior.

`PDFPrinter` is now a core coordinator rather than a platform print
implementation. It validates common printer settings, snapshots them into a
`PDFPrintJob`, and delegates to either its per-instance `PrintBackend` or the
registered `PrintingBackend.Current`.

## Backend Boundary

Core `PdfBox.Net` owns:

- `PDFPrinter`
- `PDFPrintJob`
- `IPDFPrintBackend`
- `PrintingBackend`
- the unsupported default backend, which throws a clear
  `PlatformNotSupportedException`

Optional backend packages own platform submission. The first concrete backend is
`PdfBox.Net.SystemDrawing`:

- `SystemDrawingPrintBackend` submits pages through `System.Drawing.Printing`
- it is Windows-only
- it does not replace the rendering backend
- callers should register a complete rendering backend, currently
  `PdfBox.Net.SkiaSharp`, before printing real pages

## Capability Matrix

| Area | Status |
|---|---|
| Core printing API | Available on all supported .NET platforms. |
| No-backend behavior | Fails clearly with guidance to register an optional backend. |
| Windows printer submission | Implemented by `PdfBox.Net.SystemDrawing` through `System.Drawing.Printing`. |
| Windows print-to-file | Supported when the configured printer driver supports print-to-file. |
| Cross-platform printer submission | Not implemented yet; add a new `IPDFPrintBackend` implementation without changing core. |
| Full page rendering for print | Supplied by the registered rendering backend; use `PdfBox.Net.SkiaSharp` today. |

## Test Coverage

- Core tests cover default unsupported behavior and deterministic print-to-file
  delegation through a fake backend.
- SystemDrawing tests cover Windows backend registration.
- Real printer integration remains environment-gated because CI cannot assume a
  configured printer device or PDF/XPS print driver.

## Follow-Up Guidance

Future print backends should implement `IPDFPrintBackend` in optional packages
and register themselves through `PrintingBackend.Register(...)`. Keep platform
types out of core `PdfBox.Net` unless they are already part of a separate,
documented Java-shaped proxy layer.
