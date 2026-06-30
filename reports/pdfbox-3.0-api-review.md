# PDFBox 3.0 API Review

Generated for issue #589 on 2026-06-30.

## Scope

- Target branch: `release/3.0`.
- Upstream source: Apache PDFBox `origin/3.0` at `ea68b6feae80e671b3d26565b12eccc79e74d967`.
- Compared modules: `io`, `fontbox`, `xmpbox`, and `pdfbox`.
- Generated artifacts:
  - `reports/api-surface-comparison.json`
  - `reports/pdfbox-api-surface-analysis.md`
  - `reports/api-surface-dispositions.json`
  - `reports/api-surface-ratchet-baseline.json`

The comparison is API-shape only. Behavioral parity remains covered by the runtime corpus and focused unit tests.

## Result

| Metric | Count |
|---|---:|
| Java public/protected types | 579 |
| Matched public .NET types | 577 |
| Missing mapped public .NET types | 0 |
| Java public/protected members | 6261 |
| Matched members | 5349 |
| Arity-drift members | 45 |
| Missing members | 867 |
| Reviewed API deltas | 920 |
| Unreviewed API deltas | 0 |

The 3.0 branch is now at **100% reviewed API surface** for the generated public/protected comparison: every type/member delta is either matched, implemented as a compatibility member, or recorded with an explicit reviewed disposition.

This does not mean 100% one-for-one Java API parity. The current heuristic member coverage is **5394 / 6261 = 86.2%** when counting exact matches plus arity-drift rows. The remaining differences are accepted adaptations or reviewed backlog families.

## Implemented 3.0 Compatibility Members

- Added the Java 3.0 `Type2CharStringParser.Parse(byte[], byte[][], byte[][], string?)` overload. The glyph name is accepted for API parity and delegates to the existing parser because the upstream 3.0 implementation also does not use it.
- Restored deprecated COS compatibility accessors:
  - `COSObject.GetObjectNumber()`
  - `COSObject.GetGenerationNumber()`
- Restored indirect-object traversal compatibility methods:
  - `COSArray.GetIndirectObjectKeys(...)`
  - `COSDictionary.GetIndirectObjectKeys(...)`
- Added the Java 3.0 `PDFXrefStreamParser(COSStream, COSDocument)` constructor overload. The document parameter is retained for API shape and the parser behavior remains stream-backed.
- Added deprecated color-space array constructors:
  - `PDDeviceN(COSArray)`
  - `PDSeparation(COSArray)`
- Added `XMPageTextSchema` constants that proxy the current `XMPPageTextSchema` constants, preserving the 3.0 class name and property metadata.
- Added the deprecated `PropertiesDescription.GetPropertiesName()` alias for `GetPropertiesNames()`.

Focused tests were added or extended for these entry points.

## Accepted Adaptations

These issue #589 rows were reviewed and recorded in `reports/api-surface-dispositions.json`:

- `CharStringCommand.equals/hashCode`: the port represents `CharStringCommand` as a C# enum, so runtime enum equality and hashing replace Java object overrides.
- `BruteForceParser` and `COSParser` repair/xref helper methods and protected fields: these are parser internals in the .NET implementation. The behavior is kept behind document load and repair parsing rather than exposing Java protected hooks.
- Font constructor drift:
  - `PDMMType1Font(COSDictionary, ResourceCache)`
  - `PDType0Font(COSDictionary)`
  - `PDType1CFont(COSDictionary)`
  These are recorded as constructor-shape adaptations because the .NET font stack resolves resource cache and embedded-font state through `PDResources`, `PDDocument`, and factory/load paths.
- `XMPMediaManagementSchema.getResourceRefProperty()` and `getHistory()`: these JavaBean helpers depend on the broader Java XmpBox complex-property helper layer. The current port keeps constants and XML round-trip behavior, with one-for-one helper expansion left to the XmpBox API backlog.

## Deferred Backlog

No unreviewed rows remain for issue #589. The generated report still records reviewed API gaps, especially in:

- XmpBox schema JavaBean helper coverage.
- Tagged PDF attribute object convenience methods.
- Parser/writer internal extension points.
- Java collection, stream, and resource-cache overload shapes where the port intentionally uses .NET-native or factory-based equivalents.

Future compatibility PRs should lower `reports/api-surface-ratchet-baseline.json` whenever they convert reviewed gaps into implemented compatibility members.
