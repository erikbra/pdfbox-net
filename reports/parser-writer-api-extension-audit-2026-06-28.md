# Parser/Writer API Extension-Point Audit - 2026-06-28

Issue: #548

## Summary

Reviewed the remaining `pdfbox:pdfparser` and `pdfbox:pdfwriter` rows in
`reports/api-surface-comparison.json`. This pass closes the stable public file-load
compatibility gap on `PDFParser` and reaffirms that most parser/writer rows are
implementation hooks that should stay internal in the adapted .NET pipeline.

## Reduced Rows

| Family | Java type | Java member | .NET action |
|---|---|---|---|
| `pdfbox:pdfparser` | `org.apache.pdfbox.pdfparser.PDFParser` | `load(File)` | Added `PDFParser.Load(string)` delegating to `PDDocument.Load(string)`. |
| `pdfbox:pdfparser` | `org.apache.pdfbox.pdfparser.PDFParser` | `load(File, String)` | Added `PDFParser.Load(string, string?)` delegating to `PDDocument.Load(string, string?)`. |

## Accepted Internal Implementation Hooks

| Family | Area | Decision |
|---|---|---|
| `pdfbox:pdfparser` | `COSParser` protected parse/read/decryption/header helpers | Keep internal. The C# parser has an adapted lifecycle centered on `PDFParser`, `SyntaxReader`, and `PDDocument.Load`; exposing Java protected hooks would create unsupported extension points. |
| `pdfbox:pdfparser` | `BruteForceParser` recovery helpers | Keep internal. Recovery state and xref repair are implementation details of the adapted parser path. |
| `pdfbox:pdfparser` | `PDFObjectStreamParser` stepwise methods | Keep adapted. The public C# surface exposes consolidated `Parse()` results; Java stepwise `parseObject`, `parseAllObjects`, and `readObjectNumbers` remain implementation details unless a concrete external use case appears. |
| `pdfbox:pdfparser` | `PDFXRefStream` and `XrefParser` mutation/parse helpers | Keep adapted. Xref tables and trailers are surfaced through `COSDocument` and `XrefTrailerResolver`, not Java builder-style entry points. |
| `pdfbox:pdfparser` | `XReferenceEntry.getType()` and `XReferenceType.getNumericValue()` | Keep adapted. `getType()` conflicts with `object.GetType()` and is exposed as `GetXReferenceType`; enum numeric values are available by casting. |
| `pdfbox:pdfwriter` | `COSWriter` body/header/trailer/xref helpers | Keep internal. Document serialization is owned by the adapted `PDDocument` and writer pipeline; exposing Java low-level hooks would imply unsupported byte-layout extension behavior. |
| `pdfbox:pdfwriter` | `COSWriter` signing/compression/reference public helpers | Keep adapted until the writer supports matching Java incremental-signing and compression controls at this layer. |
| `pdfbox:pdfwriter` | `COSWriter.getBytes` rows | Keep not-applicable. These are source-parser artifacts from Java static byte-array initializer calls, not public API methods. |

## Follow-Up

Do not add parser/writer compatibility methods unless they delegate to already supported
public behavior. Public parser/writer APIs should avoid exposing protected Java hooks that
would couple users to internal object-numbering, xref, trailer, signing, or stream-layout
details that the .NET port does not promise to preserve.
