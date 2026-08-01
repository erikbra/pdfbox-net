# Issue 912 Upstream Sync Assessment

## Scope

- Apache PDFBox branch: `trunk`
- Previous tracked commit: `a2ca944312187dd809c4b203001d4b782fd5b0b0`
- Assessed through commit: `fee11b453d66725c2b3a28b6f862a8dc24d33177`
- Upstream commits reviewed: 51

## Ported behavior

- PDFBOX-6220: cache GSUB glyph-array splitters per OpenType feature in all six
  script workers.
- PDFBOX-6221: use `/Helv` for visible-signature appearance text and let the
  builder own the output stream lifetime.
- PDFBOX-6222: expose `GlyphSubList.ToIntArray()`.
- PDFBOX-6223: regenerate stale form appearances when their bounding box no
  longer matches the widget, including rotated widgets.
- PDFBOX-6224: expose the radio-button `NoToggleToOff` field flag setter.
- PDFBOX-6226: cache ASCII operator names and eliminate repeated string-to-byte
  conversions while writing content streams; serialize `null` using the
  canonical `COSNull` bytes.
- PDFBOX-6227/PDFBOX-6228: stop cyclic AcroForm parent and structure-parent
  traversal. The structure-parent guard was already present in the .NET port.
- PDFBOX-6229: validate cross-reference `/W` arrays and reject zero or excessive
  line widths.
- Annotation appearance generation now tolerates a missing rectangle when
  initializing the base appearance stream.

## Already represented or not applicable

- PDFBOX-6175 removes the parent Type 0 font from `PDCIDFont`. The .NET font
  model is already parentless; `PDType0Font` owns the CMap and delegates to its
  descendant explicitly.
- The FDF option, Type 4 function, DeviceN, Separation, and optional-content
  null-safety changes were already present in their .NET counterparts.
- The TIFF unsigned-LONG fix is not applicable to `CCITTFactory`: the .NET
  implementation delegates TIFF decoding to the configured image provider and
  has no Java-style `readlong` parser.
- PNG and ASCIIHex changes that only alter Java logging or local visibility do
  not change .NET behavior.
- Debugger visibility and JavaDoc changes do not alter the public .NET debugger
  behavior. The Type 0 debugger already obtains the descendant base-font value
  through `PDFont.GetName()`.
- `PDAbstractContentStream` delegates operator serialization to
  `ContentStreamWriter`, so the operator-byte cache is implemented once there.

## Excluded optional module

The new `pdfbox-layout-fop` module contains four production Java files and
depends on the Apache FOP Java runtime. PDFBox.Net has no Apache FOP runtime or
equivalent dependency. It is explicitly excluded from the .NET parity inventory
rather than being counted as silently mapped. The existing native/AWT-style
glyph-layout integration remains in scope.

## Regression coverage

- Cached operator bytes and canonical `null` serialization.
- Repeated GSUB substitutions reuse the feature splitter.
- Zero-width and valid cross-reference `/W` arrays.
- Stale widget appearance bounding-box replacement.
- Cyclic orphan-widget parent chains.
- Radio-button `NoToggleToOff` round trip.
- Missing annotation rectangle handling.
- `GlyphSubList.ToIntArray()` output.
