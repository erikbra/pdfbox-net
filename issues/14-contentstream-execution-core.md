### Title
Port `contentstream/**` execution core

### Depends on
- #4 next-stage conversion plan
- Chunk 3 minimal document pipeline baseline

### Background
The current content-stream base is still skeletal:
- `src/PdfBox.Net/ContentStream/PDFStreamEngine.cs` resets a small amount of local state but does
  not execute page content streams.
- `src/PdfBox.Net/ContentStream/PDFStreamEngineStubs.cs` only provides placeholder operator types.
- `src/PdfBox.Net/Text/PDFTextStripper.cs` and `src/PdfBox.Net/Text/PDFMarkedContentExtractor.cs`
  therefore expose API surface without a real execution engine underneath.

Before text extraction can reach behavioral parity, the shared content-stream execution layer must
become real and reusable.

### Scope
- Replace the stub behavior in `src/PdfBox.Net/ContentStream/PDFStreamEngine.cs` with a real
  execution core.
- Port the operator registry and dispatch model needed to execute parsed content streams.
- Add graphics-state stack handling, text matrix management, and page-level execution flow.
- Support processing simple page/content streams produced by the existing parser/writer layer.
- Keep the scope focused on the reusable execution core; do not fold rendering backend work into
  this issue.

### Expected test scope
- Add focused tests for operator registration/dispatch and graphics-state stack transitions.
- Add deterministic fixture coverage proving that a simple page/content stream is executed rather
  than ignored.

### Exit criteria
- `src/PdfBox.Net/ContentStream/PDFStreamEngine.cs` no longer behaves as a stub-only reset shell.
- Simple page/content streams execute through registered operators.
- State stack and text matrices survive realistic save/restore and text-begin/text-end sequences.
- `dotnet build` and `dotnet test` remain green.

