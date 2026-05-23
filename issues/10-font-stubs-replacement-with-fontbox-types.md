### Title
Replace `PDModel.Font` stubs with real FontBox-backed implementations

### Depends on
- #6 `fontbox/util` follow-up
- #7 `fontbox/encoding` port
- #8 `fontbox/type1` + `fontbox/cff` port
- #9 `fontbox/ttf` pipeline port

### Scope
- Incrementally replace types in `src/PdfBox.Net/PDModel/Font/FontStubs.cs`.
- Wire `PDModel.Font` surfaces to newly ported FontBox implementations.

### Exit criteria
- Stubbed types are removed/reduced for touched areas.
- Integration tests cover replaced pathways.
- `dotnet test` remains green.
