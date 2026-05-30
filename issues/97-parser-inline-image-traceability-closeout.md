### Title
Close parser/contentstream semantic parity debt (`Loader`, `BaseParser`, `COSObject`, inline-image operators)

### Depends on
- #96 final parity lock rerun

### Background
Coverage mapping is now 100% (`1067/1067`, missing `0`), but parity lock is still blocked by scoped non-`in-sync` traceability rows. The highest-coupling parser/contentstream slice should be resolved first to stabilize document read semantics.

### Scope
- Resolve non-`in-sync` rows tied to:
  - `pdfbox/src/main/java/org/apache/pdfbox/Loader.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/BaseParser.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/cos/COSObject.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/BeginInlineImage.java`
- Reconcile split-operator mapping (`BeginInlineImageData.cs`, `EndInlineImage.cs`) with upstream behavior and traceability notes.
- Update conversion/normalization/traceability records for touched rows.

### Expected test scope
- Targeted parser/contentstream tests (`Loader`, parser, inline-image operator paths).

### Exit criteria
- All scoped rows in this slice are `in-sync`.
- No new parser/contentstream regressions in touched tests.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted parser/contentstream tests pass.
- Canonical parity reports regenerated and checked in.
