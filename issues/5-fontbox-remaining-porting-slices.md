### Title
Fully port `fontbox/**` in dependency-ordered slices (follow-up to initial `BoundingBox` port)

### Context
- This repository currently has a partial/stubbed `PDModel.Font` surface and no complete `FontBox` module split yet.
- This change set ports `org.apache.fontbox.util.BoundingBox` and wires the existing `PDModel.Font.BoundingBox` shim to it.
- A full one-to-one `fontbox/**` parity effort is too large for one PR and should proceed in smaller, reviewable issues.

### Current status
See `reports/fontbox-gap-analysis.md` for a full inventory and gap breakdown.
- **143** Java files in the fontbox module (upstream trunk `6b9b255`)
- **44** C# files ported (~31%)
- **99** files remaining across 8 packages

### Completed sub-issues
- [x] `fontbox/util/BoundingBox` — ported
- [x] `fontbox/encoding/**` — ported (4/4)
- [x] `fontbox/pfb/**` — ported (1/1)
- [x] `fontbox/type1/**` — ported (6/6)
- [x] `fontbox/util/autodetect/**` — ported (7/7)
- [x] `fontbox/cff/**` (partial) — 11/26 ported; charstring + full charset pending
- [x] `fontbox/ttf/**` (partial) — 12/44 root + 0/39 subdirs ported; core tables pending

### Suggested sub-issues
- [ ] #6 Port `fontbox/util/**` core utilities beyond `BoundingBox`. *(scope complete — no additional util files exist beyond BoundingBox)*
- [ ] #7 Port `fontbox/encoding/**` with parity tests for code/name lookup behavior. *(DONE)*
- [ ] #8 Complete `fontbox/cff/**` — charstring interpreter, full charset/encoding, CID support (15 missing files).
- [ ] #9 Port remaining `fontbox/ttf/**` tables and TrueType/OpenType parsing pipeline with fixture-driven tests (32 root + 39 subdir files).
- [ ] #10 Replace `src/PdfBox.Net/PDModel/Font/FontStubs.cs` types incrementally with real implementations backed by ported FontBox types.
- [ ] #11 Introduce the dedicated `PdfBox.Net.FontBox` project split from `docs/csproj-package-mapping.md`.
- [ ] #12 Port `fontbox/afm/**` — AFM parser and data model (8 files, new package).
- [ ] #13 Port `fontbox/cmap/**` — CMap parser and data model (5 files, new package).

### Issue drafts prepared for separate dependency-ordered PRs
- `issues/6-fontbox-util-follow-up.md`
- `issues/7-fontbox-encoding-port.md`
- `issues/8-fontbox-type1-cff-port.md` *(updated with detailed CFF gaps)*
- `issues/9-fontbox-ttf-pipeline-port.md` *(updated with detailed TTF gaps)*
- `issues/10-font-stubs-replacement-with-fontbox-types.md`
- `issues/11-fontbox-project-split.md`
- `issues/12-fontbox-afm-port.md` *(new)*
- `issues/13-fontbox-cmap-port.md` *(new)*

### Done criteria per sub-issue
- [ ] Ported files include provenance headers/comments and doc comments.
- [ ] Focused tests are ported/added for the touched area.
- [ ] `dotnet test` remains green after each slice.
