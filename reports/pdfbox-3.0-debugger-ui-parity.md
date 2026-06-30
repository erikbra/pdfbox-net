# PDFBox 3.0 Debugger UI Parity

Issue #602 resolves the debugger product-scope decision for the `release/3.0`
branch.

## Decision

PdfBox.Net 3.0 does not claim a full Java Swing `PDFDebugger` desktop UI.
Instead, the 3.0 line treats the Swing application as an accepted product
adaptation and keeps the ported debugger surface as `PdfBox.Net.Debugger`, a
non-packable inspection/model library, plus `apps/PdfBox.Net.Debugger.App`, a
non-packable `pdfdebugger` console inspection entry point.

This preserves source comparability for the debugger module without forcing a
cross-platform desktop framework into the core 3.0 package set. A future
desktop, web, or IDE debugger can be built as a separate package or application
on top of these models.

## Covered Debugger Behavior

- COS tree and trailer inspection through `PDFDebugger.InspectDocument` and
  `PDFDebugger.DumpCOSTree`.
- Console document inspection through the non-packable `pdfdebugger` app.
- Page labels and xref entry summarization.
- Content stream token parsing for debugger stream inspection flows.
- Text extraction/searchability flows through the normal text stripper.
- Font encoding panes for simple, Type 0, and Type 3 fonts.
- Type 3 font table glyph previews now render to `BufferedImage` through the
  registered rendering backend, matching the Java debugger's temporary-PDF
  approach without taking a direct UI dependency.

## Accepted Adaptations

- `pdfbox debug` remains an explicit unsupported command in the core
  `PdfBox.Net.Tools` dispatcher because Java starts a Swing application there;
  use the non-packable `pdfdebugger` console app for inspection.
- `PdfBox.Net.Debugger` and `PdfBox.Net.Debugger.App` are intentionally
  non-packable on the 3.0 branch.
- Swing-specific window, menu, dialog, recent-file, and macOS integration
  behavior is not part of the 3.0 stable package gate.
- UI widgets are represented by data/model classes where practical; any full UI
  shell should live in a separate package or application.

## Validation

Issue #602 adds non-GUI debugger tests for:

- COS tree inspection.
- stream token inspection.
- searchable text inspection.
- Type 3 font controller/model selection.
- Type 3 glyph preview rendering with the registered Skia backend.

## Result

The debugger scope is resolved for `release/3.0`: all Apache debugger source
stems are mapped, the practical non-GUI inspection behavior is covered, and the
Swing UI is an accepted product adaptation rather than an open parity blocker.
