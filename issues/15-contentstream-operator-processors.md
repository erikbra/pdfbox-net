### Title
Replace `PDFStreamEngineStubs.cs` with real text/state/marked-content operators

### Depends on
- #14 port `contentstream/**` execution core

### Background
`src/PdfBox.Net/ContentStream/PDFStreamEngineStubs.cs` currently contains placeholder operator
types with no execution behavior. That blocks baseline text extraction, because the stream engine
cannot yet respond to the PDF operators that manipulate text state, positioning, and marked
content.

### Scope
- Replace the placeholder operators in `src/PdfBox.Net/ContentStream/PDFStreamEngineStubs.cs`
  with real operator processors or move them into their final files/namespaces as needed.
- Cover the baseline extraction operators required for text and marked content:
  - graphics state: `q`, `Q`, `cm`
  - text lifecycle: `BT`, `ET`
  - text state: `Tf`, `Tc`, `Tw`, `Tz`, `TL`, `Tr`, `Ts`
  - text positioning/showing: `Td`, `TD`, `Tm`, `T*`, `Tj`, `TJ`, `'`, `"`
  - marked content: `BMC`, `BDC`, `EMC`, `MP`, `DP`
  - `Do` only where needed for baseline marked-content/XObject traversal
- Preserve dependency boundaries so rendering-specific operators remain out of scope.

### Expected test scope
- Add deterministic tests per operator family where behavior is externally visible.
- Verify text-state updates, text-matrix updates, marked-content nesting, and dispatch failures for
  unsupported inputs.

### Exit criteria
- Placeholder-only operator implementations are removed or reduced to genuinely deferred
  non-text cases.
- Baseline extraction operators mutate engine state and/or emit text events correctly.
- Operator behavior is covered by focused tests using deterministic fixtures or inline streams.
- `dotnet build` and `dotnet test` remain green.

