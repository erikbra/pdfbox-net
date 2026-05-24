### Title
Make `text/**` functional for baseline extraction workflows

### Depends on
- #14 port `contentstream/**` execution core
- #15 replace `PDFStreamEngineStubs.cs` with real operators
- #16 port supporting `pdmodel` state/resources

### Background
The repository already contains mechanical ports of:
- `src/PdfBox.Net/Text/PDFTextStripper.cs`
- `src/PdfBox.Net/Text/PDFMarkedContentExtractor.cs`
- `src/PdfBox.Net/Text/PDFTextStripperByArea.cs`

However, the current tests in `tests/PdfBox.Net.Tests/RenderingTextTest.cs` are still largely API
surface checks. The next step is to make the text package materially useful for simple extraction
scenarios.

### Scope
- Hook the text package to the real content-stream execution stack.
- Make `PDFTextStripper` work for basic open/process/extract flows on deterministic fixtures.
- Make `PDFMarkedContentExtractor` capture marked-content sequences and extracted text in baseline
  cases.
- Make `PDFTextStripperByArea` work for simple region-based extraction cases that do not depend on a
  future full rendering backend.
- Keep advanced layout heuristics and rendering-coupled features out of scope unless required for
  the baseline path.

### Expected test scope
- Add fixture-driven tests for simple text extraction, line breaks, word spacing, and marked-content
  capture.
- Add at least one basic region-extraction test for `PDFTextStripperByArea`.

### Exit criteria
- `GetText()` returns meaningful extracted output for simple fixture PDFs.
- `PDFMarkedContentExtractor` returns real marked-content results instead of empty/surface-only
  behavior.
- `PDFTextStripperByArea` supports baseline deterministic region extraction.
- `tests/PdfBox.Net.Tests/RenderingTextTest.cs` is no longer limited to API-surface validation for
  the covered scenarios.
- `dotnet build` and `dotnet test` remain green.
