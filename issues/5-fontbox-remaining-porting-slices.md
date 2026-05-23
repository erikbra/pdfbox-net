### Title
Fully port `fontbox/**` in dependency-ordered slices (follow-up to initial `BoundingBox` port)

### Context
- This repository currently has a partial/stubbed `PDModel.Font` surface and no complete `FontBox` module split yet.
- This change set ports `org.apache.fontbox.util.BoundingBox` and wires the existing `PDModel.Font.BoundingBox` shim to it.
- A full one-to-one `fontbox/**` parity effort is too large for one PR and should proceed in smaller, reviewable issues.

### Suggested sub-issues
- [ ] #6 Port `fontbox/util/**` core utilities beyond `BoundingBox`.
- [ ] #7 Port `fontbox/encoding/**` with parity tests for code/name lookup behavior.
- [ ] #8 Port `fontbox/type1/**` and `fontbox/cff/**` in dependency order, including parser tests.
- [ ] #9 Port `fontbox/ttf/**` tables and TrueType/OpenType parsing pipeline with fixture-driven tests.
- [ ] #10 Replace `src/PdfBox.Net/PDModel/Font/FontStubs.cs` types incrementally with real implementations backed by ported FontBox types.
- [ ] #11 Introduce the dedicated `PdfBox.Net.FontBox` project split from `docs/csproj-package-mapping.md`.

### Issue drafts prepared for separate dependency-ordered PRs
- `issues/6-fontbox-util-follow-up.md`
- `issues/7-fontbox-encoding-port.md`
- `issues/8-fontbox-type1-cff-port.md`
- `issues/9-fontbox-ttf-pipeline-port.md`
- `issues/10-font-stubs-replacement-with-fontbox-types.md`
- `issues/11-fontbox-project-split.md`

### Done criteria per sub-issue
- [ ] Ported files include provenance headers/comments and doc comments.
- [ ] Focused tests are ported/added for the touched area.
- [ ] `dotnet test` remains green after each slice.
