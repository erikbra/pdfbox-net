### Title
Introduce dedicated `PdfBox.Net.FontBox` project split

### Depends on
- #6 through #10

### Scope
- Create `src/PdfBox.Net.FontBox/PdfBox.Net.FontBox.csproj` based on `docs/csproj-package-mapping.md`.
- Move ported FontBox code into the new project with solution/test wiring updates.

### Exit criteria
- New project builds and is referenced correctly from dependent projects/tests.
- No namespace/reference regressions from project split.
- `dotnet test` remains green.
