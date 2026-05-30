### Title
Burn down final non-`in-sync` traceability rows to zero (21 rows)

### Depends on
- #94 pdmodel graphics/blend/state gap closeout

### Background
After mapping reaches 100%, parity lock still requires all scoped traceability rows to be `in-sync`. The current baseline has 21 non-`in-sync` rows (`partial`: 11, `partially-in-sync`: 10).

### Scope
- Close remaining `partial` rows for:
  - `fontbox` parser/charstring classes (`CFFParser`, `Type1CharString`, `Type2CharString`, `TTFParser`)
  - `pdfbox` loader/filter/security classes (`Loader`, `CCITTFaxFilter`, `CryptFilter`, `DCTFilter`, `JBIG2Filter`, `JPXFilter`, `StandardSecurityHandler`)
- Close remaining `partially-in-sync` rows for:
  - inline-image/contentstream/parsing (`BeginInlineImage`, `BaseParser`, `COSObject`)
  - pdmodel document pipeline (`PDDocument`, `PDDocumentCatalog`, `PDDocumentInformation`, `PDPage`, `PDPageTree`, `PDRectangle`)
- Reconcile traceability/conversion/normalization notes to remove residual drift.

### Expected test scope
- Targeted parser/filter/font/pdmodel regression tests.

### Exit criteria
- No scoped traceability rows remain in `partial` or `partially-in-sync`.
- All touched rows are `in-sync` with current implementation.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
