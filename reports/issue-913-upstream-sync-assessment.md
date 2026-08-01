# Issue 913 Upstream 3.0 Sync Assessment

## Scope

- Apache PDFBox branch: `3.0`
- Previous tracked commit: `a1685ce5bccd2397737b056663fcf4697686fea3`
- Assessed through commit: `ddb7e78992bebc36140ba0d864c8212ec5da697b`
- Upstream commits reviewed: 48

## Ported behavior

- PDFBOX-4951: port the core glyph-layout contracts, glyph/position container,
  content-stream integration, AcroForm propagation, and the AWT-facing loader,
  processor, and example API.
- PDFBOX-6220: cache GSUB glyph-array splitters per OpenType feature in the five
  script workers changed on the 3.0 branch.
- PDFBOX-6221: use `/Helv` for visible-signature appearance text and let the
  builder own the output stream lifetime.
- PDFBOX-6223: regenerate stale form appearances when their bounding box no
  longer matches the widget, including rotated widgets.
- PDFBOX-6224: expose the radio-button `NoToggleToOff` field flag setter.
- PDFBOX-6226: cache ASCII operator names and eliminate repeated
  string-to-byte conversions while writing content streams; serialize `null`
  using the canonical `COSNull` bytes.
- PDFBOX-6227/PDFBOX-6228: stop cyclic AcroForm parent and structure-parent
  traversal. The structure-parent guard was already present in the .NET port.
- PDFBOX-6229: validate cross-reference `/W` arrays and reject zero or excessive
  line widths.
- Annotation appearance generation now tolerates a missing rectangle when
  initializing the base appearance stream.

## Adapted or already represented behavior

- Java AWT font lookup is represented by the existing .NET AWT proxy. The
  processor currently emits conservative Identity-H glyph codes through the
  same public layout contracts; backend-specific advanced shaping remains an
  explicit adaptation.
- The FDF option, Type 4 function, DeviceN, Separation, and optional-content
  null-safety changes were already present in their .NET counterparts.
- The TIFF unsigned-LONG fix is not applicable to `CCITTFactory`: the .NET
  implementation delegates TIFF decoding to the configured image provider and
  has no Java-style `readlong` parser.
- PNG and ASCIIHex changes that only alter Java logging or local visibility do
  not change .NET behavior.
- Debugger visibility and JavaDoc changes do not alter public .NET debugger
  behavior.
- Java/Graal AWT initialization handling in `PDDocument` is runtime-specific
  and does not require a .NET code path.

## Excluded optional module

The new `pdfbox-layout-fop` module contains four production Java files and
depends on the Apache FOP Java runtime. PDFBox.Net has no Apache FOP runtime or
equivalent dependency. It is explicitly excluded from the .NET parity inventory
rather than being counted as silently mapped. The core and AWT-facing glyph
layout APIs remain in scope.

## Regression coverage

- Glyph-layout page content streams emit hexadecimal `Tj`/`TJ` operands and
  text-rise operators.
- `GlyphSubList.ToIntArray()` preserves glyph codes.
- Cached operator bytes and canonical `null` serialization.
- Repeated GSUB substitutions reuse the feature splitter.
- Zero-width and valid cross-reference `/W` arrays.
- Stale widget appearance bounding-box replacement.
- Cyclic orphan-widget parent chains.
- Radio-button `NoToggleToOff` round trip.
- Missing annotation rectangle handling.

All changed production files have provenance, conversion, normalization, and
traceability records at the synchronized commit. Native regression tests are
recorded in `reports/conversion-records.json`; none are silently deferred.
