### Title
Port XmpBox slice 1: metadata entry points and XML parser/serializer pipeline

### Depends on
- #42 xmpbox porting plan

### Background
This slice establishes the executable XMP entry path so later schema/type work can
land against stable parser and serializer wiring.

### Scope
- Port core metadata entry points under `org/apache/xmpbox/**` needed to parse and
  serialize representative XMP packets.
- Port the XML parser/serializer path and keep behavior aligned with upstream.
- Add/update focused tests in `PdfBox.Net.XmpBox.Tests` for deterministic parse/write.

### Expected test scope
- XMP packet parse/serialize roundtrip tests.
- Invalid XML/metadata handling tests for key edge cases.

### Entry criteria
- Baseline `dotnet build`/`dotnet test` is green.

### Exit criteria
- XMP entry points can parse and serialize fixture packets with stable output.
- Traceability rows for touched XmpBox paths are updated.

### Risk register
- XML namespace handling drift can cascade into all later schema/type slices.

### Definition of done
- `dotnet build` passes.
- Targeted XmpBox tests pass.
- Conversion/normalization/traceability artifacts are refreshed for touched files.
