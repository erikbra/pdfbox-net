### Title
Complete remaining COS containers and primitives

### Depends on
- #3 COS foundation milestone (this issue series owner)

### Scope
- Port remaining foundational classes in `pdfbox/cos/**` for primitive/value/container completeness.
- Prioritize types required by parser/writer entry paths.
- Exclude stream-specific lifecycle concerns (handled in #27).

### Expected test scope
- Extend/add COS primitive/container tests in `tests/PdfBox.Net.Tests/*COS*`.
- Add roundtrip assertions for dictionary/array/name/number/string interactions.

### Exit criteria
- Remaining primitive/container COS types in this slice are ported and compile.
- Targeted COS tests pass.
- Required conversion/normalization/traceability records are updated.
