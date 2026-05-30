### Title
Close filter/security + FontBox parser semantic parity debt

### Depends on
- #98 PDModel document-pipeline semantic parity closeout

### Background
The remaining `partial` traceability rows are concentrated in filter/security adapters and FontBox parser/charstring classes. This slice closes the last non-`in-sync` hotspots before final lock.

### Scope
- Resolve non-`in-sync` rows tied to:
  - `pdfbox/src/main/java/org/apache/pdfbox/filter/CCITTFaxFilter.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/filter/CryptFilter.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/filter/DCTFilter.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/filter/JBIG2Filter.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/filter/JPXFilter.java`
  - `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/StandardSecurityHandler.java`
  - `fontbox/src/main/java/org/apache/fontbox/cff/CFFParser.java`
  - `fontbox/src/main/java/org/apache/fontbox/cff/Type1CharString.java`
  - `fontbox/src/main/java/org/apache/fontbox/cff/Type2CharString.java`
  - `fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java`
- Update conversion/normalization/traceability records for touched rows.

### Expected test scope
- Targeted filter, encryption, and FontBox parser regression tests.

### Exit criteria
- All scoped rows in this slice are `in-sync`.
- No regressions in filter/security/font parsing tests.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted filter/security/font tests pass.
- Canonical parity reports regenerated and checked in.
