### Title
Add fixture-driven regression coverage for text extraction parity

### Depends on
- #17 make `text/**` functional for baseline extraction workflows

### Background
Once the content-stream and text stack is made functional, the repository needs regression coverage
that validates behavior rather than only public API shape. This issue hardens the new baseline
before the project moves on to the larger rendering/backend epic.

### Scope
- Add deterministic fixture coverage for:
  - simple text extraction
  - line breaks and paragraph boundaries
  - spacing-sensitive extraction cases
  - marked-content capture
  - multi-page extraction
- Record a small parity matrix of supported scenarios and known gaps for the current baseline.
- Prefer fixture-driven tests in `tests/PdfBox.Net.Tests/` over synthetic assertions that only
  exercise getters/setters.

### Expected test scope
- Expand or split `tests/PdfBox.Net.Tests/RenderingTextTest.cs` as needed so behavioral coverage is
  clear and maintainable.
- Add new fixture files under `tests/PdfBox.Net.Tests/Fixtures/` where necessary.

### Exit criteria
- Text extraction tests validate real behavior against deterministic fixtures.
- The baseline supported/known-gap matrix is documented alongside the new tests or issue notes.
- Regressions in simple extraction, spacing, marked content, and multi-page behavior are caught by
  the test suite.
- `dotnet build` and `dotnet test` remain green.

