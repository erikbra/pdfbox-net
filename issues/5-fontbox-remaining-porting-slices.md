### Title
Fully port `fontbox/**` in dependency-ordered slices (follow-up to initial `BoundingBox` port)

### Context
- This repository currently has a partial/stubbed `PDModel.Font` surface and no complete `FontBox` module split yet.
- This change set ports `org.apache.fontbox.util.BoundingBox` and wires the existing `PDModel.Font.BoundingBox` shim to it.
- A full one-to-one `fontbox/**` parity effort is too large for one PR and should proceed in smaller, reviewable issues.

### Suggested sub-issues
- [ ] Port `fontbox/util/**` core utilities beyond `BoundingBox` (including parser/stream helpers used by downstream font parsers).
- [ ] Port `fontbox/encoding/**` with parity tests for code/name lookup behavior.
- [ ] Port `fontbox/type1/**` and `fontbox/cff/**` in dependency order, including parser tests.
- [ ] Port `fontbox/ttf/**` tables and TrueType/OpenType parsing pipeline with fixture-driven tests.
- [ ] Replace `src/PdfBox.Net/PDModel/Font/FontStubs.cs` types incrementally with real implementations backed by ported FontBox types.
- [ ] Introduce the dedicated project split from `docs/csproj-package-mapping.md` (`src/PdfBox.Net.FontBox/PdfBox.Net.FontBox.csproj`) once a critical mass of files has been ported.

### Done criteria per sub-issue
- [ ] Ported files include provenance headers/comments and doc comments.
- [ ] Focused tests are ported/added for the touched area.
- [ ] `dotnet test` remains green after each slice.
