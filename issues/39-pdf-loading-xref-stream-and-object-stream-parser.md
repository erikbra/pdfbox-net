### Title
Port xref-stream and object-stream parsing for PDF 1.5+ loading

### Goal
Add support for modern PDFs that use cross-reference streams and compressed object streams.

### Depends on
- #38 classic xref table and trailer resolution
- Filter pipeline already ported (`FlateFilter`, etc.)

### Scope
- Port parser logic for cross-reference streams from upstream parser classes.
- Port `PDFObjectStreamParser.java` behavior for `ObjStm` extraction.
- Integrate object stream entries with xref resolver for indirect object loading.
- Defer encryption handling to #34.

### Expected test scope
- Load PDF 1.5+ fixture using cross-reference stream.
- Validate object stream extraction and indirect object resolution.

### Entry criteria
- #38 merged and green.

### Exit criteria
- Parser handles xref streams and object streams on deterministic fixtures.
- No regressions on classic xref-table fixtures.

### Risk register
- Stream filter/decompression boundary differences.
- Incorrect field-width handling in xref stream decoding.

### Definition of done
- `dotnet build` passes.
- Xref-stream/object-stream tests pass.
- Required report rows updated.
