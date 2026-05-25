### Title
Establish PDModel/font core foundation (descriptor, base hierarchy, factory wiring)

### Background
`org.apache.pdfbox.pdmodel.font` is now the next largest dependency-safe conversion
chunk. Before concrete font-type parity, the core foundation layer must be completed
so higher-level paths (text, rendering, interactive appearance) can rely on stable
font descriptor metrics and construction flow.

### Depends on
- Parser/load baseline complete (#37–#41)
- Documentinterchange closeout complete (#43–#47)
- Existing FontBox baseline (ported)

### Scope
- Complete/align the base `PDFont` and `PDSimpleFont`/`PDDictionaryFont` behavior required
  by downstream concrete font types.
- Harden `PDFontDescriptor` parity for metrics and descriptor dictionary access patterns.
- Close remaining foundation gaps in `PDFontFactory`, `FontMapper`, `FontMappers`,
  `DefaultFontProvider`, and `FileSystemFontProvider` so construction is deterministic.
- Keep any unsupported advanced behavior explicitly marked as deferred with traceability notes.

### Expected test scope
- Add/extend focused tests for descriptor metric extraction and factory dispatch.
- Validate deterministic mapping for standard dictionary-driven font construction paths.

### Entry criteria
- `dotnet build` and `dotnet test` baseline green.
- No unresolved parser/load regressions.

### Exit criteria
- Foundation layer compiles without placeholder-only behavior in touched classes.
- `PDFontFactory` can construct expected base/derived font wrappers for covered cases.
- Descriptor and mapper/provider behavior has regression tests for current parity target.

### Risk register
- Subtle descriptor fallback differences can cascade into text width/layout behavior.
- Mapper/provider behavior may vary across platforms if not constrained in tests.

### Definition of done
- Build + targeted tests pass.
- Provenance headers and conversion/traceability artifacts updated for touched files.
