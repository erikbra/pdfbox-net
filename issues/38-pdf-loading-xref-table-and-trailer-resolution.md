### Title
Port xref table parsing and trailer resolution for PDF loading

### Goal
Enable classic xref-table based PDFs to load by implementing trailer parsing and object-offset resolution on top of the parser scaffold.

### Depends on
- #37 parser scaffold/startxref flow
- `XrefTrailerResolver` and COS types already ported

### Scope
- Port/complete parser paths for:
  - classic `xref` table parsing
  - trailer dictionary parsing
  - object offset registration and resolver integration
- Wire resolved offsets into lazy object fetch flow in parser context.
- Defer cross-reference streams and object streams to #39.

### Expected test scope
- Load minimal table-based PDF fixture and assert:
  - trailer root is resolved
  - selected object offsets resolve correctly
- Negative tests for malformed xref sections.

### Entry criteria
- #37 merged and green.

### Exit criteria
- Table-based xref parsing and trailer resolution are functional.
- Parser can resolve at least root/catalog objects via xref table.

### Risk register
- Numeric parsing edge cases for subsection ranges.
- Strict vs permissive behavior on malformed xref/trailer content.

### Definition of done
- `dotnet build` passes.
- New xref/trailer tests pass.
- Traceability/conversion records updated.
