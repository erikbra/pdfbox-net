# QA assessment for issue #42 XmpBox slice 1

Date: 2026-05-25 (UTC)

## Scope reviewed

- New project: `src/PdfBox.Net.XmpBox/PdfBox.Net.XmpBox.csproj`
- New production files:
  - `src/PdfBox.Net.XmpBox/XmpBox/XmpConstants.cs`
  - `src/PdfBox.Net.XmpBox/XmpBox/Xml/XmpParsingException.cs`
  - `src/PdfBox.Net.XmpBox/XmpBox/Xml/XmpSerializationException.cs`
  - `src/PdfBox.Net.XmpBox/XmpBox/Schema/XmpSchemaException.cs`
- New test project/file:
  - `tests/PdfBox.Net.XmpBox.Tests/PdfBox.Net.XmpBox.Tests.csproj`
  - `tests/PdfBox.Net.XmpBox.Tests/XmpCoreTest.cs`
- Updated project/report wiring:
  - `PdfBoxNet.slnx`
  - `docs/csproj-package-mapping.md`
  - `reports/conversion-records.json`
  - `reports/normalization-records.json`
  - `reports/traceability-parity-report.json`

## Upstream-test parity for completed production classes

- `xmpbox/src/main/java/org/apache/xmpbox/XmpConstants.java` -> **converted**
- `xmpbox/src/main/java/org/apache/xmpbox/xml/XmpParsingException.java` -> **converted**
- `xmpbox/src/main/java/org/apache/xmpbox/xml/XmpSerializationException.java` -> **converted**
- `xmpbox/src/main/java/org/apache/xmpbox/schema/XmpSchemaException.java` -> **converted**

Upstream direct test classes targeting only these four units are not isolated in xmpbox test sources; coverage for these units is largely embedded in broader schema/parser tests. For this bootstrap slice, focused native tests were added in `XmpCoreTest.cs` and broad parser/schema test-porting is deferred to later xmpbox slices.

## Source-to-port similarity confidence

- `XmpConstants.cs`, `XmpParsingException.cs`, `XmpSerializationException.cs`, `XmpSchemaException.cs` are **mechanical** conversions with low-risk compile normalization (constructor/property syntax and C# casing/nullable adaptation).
- No functional redesign was introduced in this slice.

## Report-row completeness check

- Added rows for all new production files and the new focused test file in:
  - `conversion-records.json`
  - `normalization-records.json`
  - `traceability-parity-report.json`
- No known report-row gaps remain for this slice.
