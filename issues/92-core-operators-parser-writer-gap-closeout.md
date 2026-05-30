### Title
Close remaining core operator/parser/writer mapping gaps (14 files)

### Depends on
- #91 final parity rescan and lock execution

### Background
The latest canonical rescan left 32 missing `pdfbox` files. This slice closes the highest-dependency core mapping gaps in content operators, parser, and writer compression.

### Scope
- Port the remaining missing files for:
  - `contentstream/operator` (4)
  - `pdfparser` (7)
  - `pdfwriter/compress` (3)
- Ensure new ports are wired into existing parser/contentstream pipelines.
- Update traceability/conversion/normalization rows for touched paths.

### Expected test scope
- Targeted contentstream/parser/writer tests.

### Exit criteria
- No remaining missing files in the scoped core operator/parser/writer set.
- Touched traceability rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
