# QA assessment — issue #81 XmpBox regression traceability and closeout

Date: 2026-05-29 (UTC)

## Scope reviewed

### New test file
- `tests/PdfBox.Net.XmpBox.Tests/XmpRegressionFixturesTest.cs`

### New fixture files
- `tests/PdfBox.Net.XmpBox.Tests/Fixtures/XmpBox/dc-format-packet.xmp`
- `tests/PdfBox.Net.XmpBox.Tests/Fixtures/XmpBox/multi-schema-packet.xmp`
- `tests/PdfBox.Net.XmpBox.Tests/Fixtures/XmpBox/pdfa-identification-packet.xmp`
- `tests/PdfBox.Net.XmpBox.Tests/Fixtures/XmpBox/lenient-no-xpacket.xmp`

### Updated project file
- `tests/PdfBox.Net.XmpBox.Tests/PdfBox.Net.XmpBox.Tests.csproj` — added `<Content>` glob to copy `Fixtures/XmpBox/*.xmp` to output

### Updated report files
- `reports/traceability-parity-report.json` — added `in-sync` row for `XmpRegressionFixturesTest.cs`

## Upstream-test parity for completed production classes

All 72 XmpBox production source files (parser, schema, and type layers) were verified
`in-sync` in `traceability-parity-report.json` prior to this closeout slice. No new
production classes were added; this slice adds only fixture-backed regression coverage.

## Tests added (`XmpRegressionFixturesTest.cs`, 16 tests)

### Roundtrip determinism (3 theory cases)
- `Parser_RoundtripFixture_ProducesDeterministicOutput(dc-format-packet.xmp)`
- `Parser_RoundtripFixture_ProducesDeterministicOutput(multi-schema-packet.xmp)`
- `Parser_RoundtripFixture_ProducesDeterministicOutput(pdfa-identification-packet.xmp)`

### Parser — xpacket attribute preservation
- `Parser_DcFormatFixture_PreservesXpacketAttributes` — begin/id/end round-trip.

### Schema extraction — DublinCore from dc-format-packet
- `Parser_DcFormatFixture_RegistersDublinCoreSchema` — typed accessor, prefix, namespace.

### Schema extraction — multi-schema packet
- `Parser_MultiSchemaFixture_RegistersAllThreeSchemas` — dc, pdf, xmp; exact count 3.
- `Parser_MultiSchemaFixture_PdfSchemaHasCorrectPrefix` — `pdf` prefix.
- `Parser_MultiSchemaFixture_XmpBasicSchemaHasCorrectPrefix` — `xmp` prefix.

### Schema extraction — PDF/A identification packet
- `Parser_PdfaIdentificationFixture_RegistersBothSchemas` — dc and pdfaid; exact count 2.
- `Parser_PdfaIdentificationFixture_PdfaSchemaHasCorrectNamespaceAndPrefix` — namespace + prefix.

### Lenient mode
- `Parser_LenientNoXpacketFixture_ParsesWithDefaultXpacketValues` — defaults applied when xpacket PIs absent.
- `Parser_LenientNoXpacketFixture_StillRegistersDublinCoreSchema` — schema extraction unaffected by lenient mode.
- `Parser_LenientNoXpacketFixture_StrictModeThrows` — strict parse of no-xpacket fixture → `XpacketBadStart`.

### Serializer
- `Serializer_MultiSchemaFixture_SchemaCountPreservedAfterRoundtrip` — count identical after serialize/reparse.

### Schema lookup overloads
- `Parser_DcFormatFixture_GetSchemaByNamespaceUriReturnsDublinCore` — `GetSchema(nsUri)`.
- `Parser_PdfaIdentificationFixture_GetSchemaByPrefixAndNamespaceUri` — `GetSchema(prefix, nsUri)`.

## Traceability status after this slice

- All 74 XmpBox traceability rows: **in-sync**.
- New row added: `tests/PdfBox.Net.XmpBox.Tests/XmpRegressionFixturesTest.cs` → **in-sync**.

## Source-to-port similarity confidence

- `XmpRegressionFixturesTest.cs` is a native-test file with no upstream Java counterpart.
  Fixtures are hand-authored representative XMP packets matching canonical PDFBox test
  data patterns. No functional redesign of production code was introduced.

## Report-row gaps

- Added traceability row for `XmpRegressionFixturesTest.cs` in `traceability-parity-report.json`.
- No conversion-records or normalization-records changes required (native-test only).
- No production source paths changed; no upstream port coverage delta.
