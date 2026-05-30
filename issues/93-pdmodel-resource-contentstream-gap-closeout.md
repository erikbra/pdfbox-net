### Title
Close pdmodel resource and content-stream foundation mapping gaps (11 files)

### Depends on
- #92 core operator/parser/writer gap closeout

### Background
After parser/operator/writer parity is closed, the next block is pdmodel foundation gaps around resource cache, content stream abstractions, and destination/name-tree roots.

### Scope
- Port remaining missing pdmodel foundation files:
  - `DefaultResourceCache`
  - `DefaultResourceCacheCreateImpl`
  - `MissingResourceException`
  - `PDAbstractContentStream`
  - `PDDocumentNameDestinationDictionary`
  - `PDFormContentStream`
  - `PDPatternContentStream`
  - `PDStructureElementNameTreeNode`
  - `ResourceCache`
  - `ResourceCacheCreateFunction`
  - `ResourceCacheFactory`
- Align integration points with existing pdmodel document/page/resource flow.
- Update traceability/conversion/normalization rows for touched paths.

### Expected test scope
- Targeted pdmodel content stream/resource/document tests.

### Exit criteria
- No remaining missing files in this pdmodel foundation slice.
- Touched traceability rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
