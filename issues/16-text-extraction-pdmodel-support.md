### Title
Port supporting `pdmodel` state/resources required by content-stream execution

### Depends on
- #14 port `contentstream/**` execution core
- #15 replace `PDFStreamEngineStubs.cs` with real operators

### Background
The text path still depends on adapted placeholder model types:
- `src/PdfBox.Net/PDModel/TextStubs.cs`
- `src/PdfBox.Net/PDModel/RenderingSupportStubs.cs`

These files currently provide enough surface area for compilation, but not enough real behavior for
content-stream execution and extraction parity.

### Scope
- Replace the relevant text-path placeholders in `src/PdfBox.Net/PDModel/TextStubs.cs`.
- Replace the relevant execution-path placeholders in
  `src/PdfBox.Net/PDModel/RenderingSupportStubs.cs`.
- Introduce real minimal implementations for:
  - `PDGraphicsState`
  - `PDTextState`
  - marked-content containers and related traversal helpers
  - the minimal resource/font/XObject wrappers required by the stream engine
- Keep the scope restricted to dependencies needed by the content-stream/text stack; broader
  rendering abstractions stay deferred.

### Expected test scope
- Add focused model/state tests where behavior is now non-trivial.
- Extend content-stream tests to prove the new model/state objects are actually used.

### Exit criteria
- No touched text-extraction path relies on stub-only `pdmodel` types.
- Graphics state, text state, and marked-content support have real behavior matching the needs of
  the stream engine.
- Newly introduced resource/font/XObject wrappers are sufficient for baseline extraction scenarios.
- `dotnet build` and `dotnet test` remain green.

