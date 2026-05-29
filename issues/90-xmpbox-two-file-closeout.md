### Title
Close final XmpBox delta (`DomHelper` + `PdfaExtensionHelper`) with regression and traceability lock

### Depends on
- #89 pdmodel feature-cluster closeout

### Background
XmpBox has two remaining missing files and should be closed in one focused pass before final parity lock.

### Scope
- Port `DomHelper`.
- Port `PdfaExtensionHelper`.
- Add/extend XmpBox regression coverage for touched flows.
- Complete traceability/conversion/normalization updates and canonical rescan.

### Expected test scope
- `PdfBox.Net.XmpBox.Tests`
- Relevant integration smoke tests

### Exit criteria
- XmpBox missing count reaches zero.
- Touched XmpBox traceability rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- `dotnet test PdfBoxNet.slnx --no-build --filter "FullyQualifiedName~XmpBox"` passes.
- Canonical reports are regenerated and checked in.
