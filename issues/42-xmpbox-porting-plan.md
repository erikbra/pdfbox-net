### Title
Plan port of `xmpbox/**` as a separate deliverable

### Goal
Create a dependency-ordered implementation plan for `xmpbox/**` with its own assembly/package boundary:
- `src/PdfBox.Net.XmpBox/PdfBox.Net.XmpBox.csproj`

### Background
- `xmpbox` has not been ported yet.
- `docs/csproj-package-mapping.md` already defines `org.apache.pdfbox:xmpbox` -> `PdfBox.Net.XmpBox`.
- `xmpbox` should be delivered independently from `PdfBox.Net`/`PdfBox.Net.FontBox`, with explicit project wiring and test coverage.

### Deliverable boundary (separate project)
- Introduce `src/PdfBox.Net.XmpBox/PdfBox.Net.XmpBox.csproj`.
- Keep `org.apache.xmpbox/**` ports under `src/PdfBox.Net.XmpBox/XmpBox/**`.
- Add solution/test wiring so XmpBox can be validated as its own deliverable.

### Suggested dependency-ordered slices
1. **Core model + namespaces**
   - Port low-fanout metadata model and shared constants/helpers first.
   - Establish XML namespace handling and value model primitives.
2. **Type system + structured properties**
   - Port typed property/value classes and structured/array containers.
   - Add focused tests for value normalization and schema/type behavior.
3. **Parsing/serialization layer**
   - Port parser and serializer flows for roundtrip read/write of representative XMP packets.
   - Add fixture-driven tests for deterministic parsing and serialization.
4. **Integration + project split closeout**
   - Wire consumers in `PdfBox.Net`/future `Preflight` only through project references.
   - Confirm `PdfBox.Net.XmpBox` remains independently buildable/testable.

### Proposed follow-up issue drafts
- `issues/43-xmpbox-core-model-and-namespaces.md`
- `issues/44-xmpbox-types-and-structured-properties.md`
- `issues/45-xmpbox-parser-serializer-roundtrip.md`
- `issues/46-xmpbox-project-split-and-integration.md`

### Exit criteria
- Dedicated `PdfBox.Net.XmpBox` project exists and builds.
- `xmpbox/**` implementation work is tracked as separate issues/deliverables.
- Focused XmpBox tests are added for each slice and `dotnet test` remains green.
- No cross-project namespace/reference regressions from the split.
