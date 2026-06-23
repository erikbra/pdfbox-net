# PDFBox vs PdfBox.Net Runtime Gap Analysis

Generated: 2026-06-23

## Scope

This report summarizes the scan state from `/tmp/pdfbox-gap-scan`.

Inputs:
- Corpus: 104 PDFs from `/tmp/pdfbox-gap-scan/manifest.txt`
- Single-document operations: load, text extraction, save/copy, first-page render at 36 DPI
- Merge operations: 25 two-document merge pairs
- Java baseline results: `/tmp/pdfbox-gap-scan/java-headless-results.clean.jsonl`, `/tmp/pdfbox-gap-scan/java-merge-results.clean.jsonl`
- .NET results: `/tmp/pdfbox-gap-scan/dotnet-results.clean.jsonl`, `/tmp/pdfbox-gap-scan/dotnet-merge-results.clean.jsonl`
- Java probe: `/tmp/pdfbox-gap-scan/src/JavaPdfProbe.java`
- .NET probe: `/tmp/pdfbox-gap-scan/src/DotnetPdfProbe.cs`

Source revisions:
- Apache PDFBox upstream checkout: `2589dc979`
- PdfBox.Net checkout: `a257b22`

Timing note: timings below are the per-operation elapsed milliseconds emitted by the probes. I did not find a separate wall-clock orchestration log for the original full scan run.

## Runtime Results

| Runtime | Operation | Count | OK | Fail | Total ms | Median ms | Mean ms |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Java single | load | 104 | 96 | 8 | 1171 | 0.0 | 11.3 |
| Java single | text | 96 | 96 | 0 | 3129 | 3.0 | 32.6 |
| Java single | save | 96 | 95 | 1 | 565 | 1.0 | 5.9 |
| Java single | render | 96 | 96 | 0 | 3372 | 15.5 | 35.1 |
| .NET single | load | 104 | 96 | 8 | 663 | 1.0 | 6.4 |
| .NET single | text | 96 | 89 | 7 | 8582 | 2.5 | 89.4 |
| .NET single | save | 96 | 96 | 0 | 269 | 1.0 | 2.8 |
| .NET single | render | 96 | 94 | 2 | 1507 | 7.0 | 15.7 |
| Java merge | merge | 25 | 22 | 3 | 23151 | 870.0 | 926.0 |
| .NET merge | merge | 25 | 22 | 3 | 4605 | 166.0 | 184.2 |

## Paired Runtime Gaps

### Load

Both runtimes loaded the same 96 of 104 files and failed the same 8 encrypted/protected files. The failure reasons differ:
- Java reports incompatible decryption material or invalid password.
- .NET reports `InvalidDataException:The archive entry was compressed using an unsupported compression method.` for 6 of the 8 failures, and invalid password for 2.

The .NET failures indicate the encrypted/public-key path is surfacing lower-level compressed-entry errors before reaching the same decryption diagnostics as Java.

### Text Extraction

Java text extraction succeeded for all 96 loaded PDFs. .NET failed on 7:

| File | .NET failure |
| --- | --- |
| `arxiv-sample.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `4PP-Highlighting.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `Acroform-PDFBOX-2333.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `Liste732004001452_001_0.pdf_0_.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `PDFBOX-3498-Y5TLCWTIAE3FYDVJTV2TXRZGXLEDUNSW.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `PDFBOX-4322-Empty-ToUnicode-reduced.pdf` | `NullReferenceException:Object reference not set to an instance of an object.` |
| `orimi-test.pdf` | `InvalidDataException:The archive entry was compressed using an unsupported compression method.` |

Among successful text operations, 50 of 89 .NET text hashes differed from Java. That is a broad semantic parity gap in `PDFTextStripper` behavior, not just isolated crashes.

### Save

.NET saved all 96 loaded documents. Java saved 95 and rejected `orimi-test.pdf` because the document still contains an encryption dictionary. This is a .NET behavioral difference worth checking: the .NET save path may be more permissive than Java or may be preserving/writing encrypted state differently.

### Rendering

Java rendered all 96 loaded PDFs. .NET rendered 94 and failed on:

| File | .NET failure |
| --- | --- |
| `orimi-test.pdf` | `InvalidDataException:The archive entry was compressed using an unsupported compression method.` |
| `survey.pdf` | `NotSupportedException:Pattern color space cannot be converted to RGB without an underlying color space.` |

The bigger rendering issue is output quality. Java produced 82 unique first-page PNG size signatures; .NET produced only 25. The most common .NET render signatures were repeated across many unrelated documents:
- `306x396:1240` for 46 files
- `297x420:1256` for 16 files
- `297x421:1259` for 9 files

This matches source stubs in `PageDrawer`: glyph rendering, images, shadings, annotations, form XObjects, transparency groups, and compositing are still TODOs. Most .NET renders are likely blank or near-blank page-size placeholders rather than faithful rendering.

### Merge

Both runtimes succeeded on 22 of 25 merge pairs and failed the same 3 encrypted/public-key pairs. Failure reasons differ in the same way as load:
- Java: incompatible decryption material.
- .NET: unsupported compression method.

## Source Coverage

Filename-level source coverage is high, but not complete:

| Module | Java source files | Missing by C# stem | Coverage |
| --- | ---: | ---: | ---: |
| `io` | 18 | 0 | 100.0% |
| `fontbox` | 143 | 0 | 100.0% |
| `xmpbox` | 74 | 3 | 95.9% |
| `pdfbox` | 618 | 8 | 98.7% |
| `tools` | 26 | 0 | 100.0% |
| `examples` | 93 | 0 | 100.0% |
| `debugger` | 91 | 0 | 100.0% |

Missing upstream classes by filename stem:

`pdfbox`:
- `org/apache/pdfbox/pdfparser/BaseParser.java`
- `org/apache/pdfbox/filter/CCITTFaxFilter.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineMiterLimit.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineJoinStyle.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineCapStyle.java`
- `org/apache/pdfbox/pdmodel/font/FontMapperImpl.java`
- `org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationStrikeout.java`
- `org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationRubberStamp.java`

`xmpbox`:
- `org/apache/xmpbox/type/StructuredType.java`
- `org/apache/xmpbox/type/PropertyType.java`
- `org/apache/xmpbox/type/Attribute.java`

## Missing or Stubbed .NET Implementation

The following are explicit implementation gaps found by scanning `src/PdfBox.Net`, `src/PdfBox.Net.FontBox`, `src/PdfBox.Net.IO`, and `src/PdfBox.Net.XmpBox`.

### Filters and Image Decoding

Missing/stubbed:
- `CCITTFaxDecode` decode/encode
- CCITT fax decoder/encoder streams
- `JBIG2Decode`
- `DCTDecode` / JPEG decode and encode
- `JPXDecode`

Observed impact:
- Load/text/render failures with `InvalidDataException:The archive entry was compressed using an unsupported compression method.`
- Image-heavy rendering and extraction paths are incomplete.

Relevant files:
- `src/PdfBox.Net/Filter/CCITTFaxDecodeFilter.cs`
- `src/PdfBox.Net/Filter/CCITTFaxDecoderStream.cs`
- `src/PdfBox.Net/Filter/CCITTFaxEncoderStream.cs`
- `src/PdfBox.Net/Filter/JBIG2Filter.cs`
- `src/PdfBox.Net/Filter/DCTFilter.cs`
- `src/PdfBox.Net/Filter/JPXFilter.cs`

### Rendering

Missing/stubbed:
- Tiling paint / AWT-equivalent paint support
- Soft masks
- Glyph rendering and glyph outline rendering
- Type 3 glyph rendering
- Image rendering and buffered image compositing
- Shading fills
- Annotation rendering
- Form XObject rendering
- Transparency group rendering and compositing

Observed impact:
- 94 .NET render operations report success, but most outputs collapse to a small number of repeated PNG sizes, strongly indicating blank or near-blank render output.
- `survey.pdf` fails on pattern color space conversion.

Relevant files:
- `src/PdfBox.Net/Rendering/PageDrawer.cs`
- `src/PdfBox.Net/Rendering/TilingPaint.cs`
- `src/PdfBox.Net/Rendering/SoftMask.cs`
- `src/PdfBox.Net/PDModel/Graphics/Color/PDPattern.cs`

### Text Extraction

Missing/incorrect behavior:
- 7 .NET text extraction crashes where Java succeeds.
- 50 successful .NET text outputs hash differently from Java.

Observed impact:
- `PDFTextStripper` parity is not yet reliable for positioning, font/CMap handling, content stream edge cases, or malformed PDFs.
- The repeated `NullReferenceException` should be treated as the first actionable bug class because Java handles those files.

Primary failing files:
- `arxiv-sample.pdf`
- `4PP-Highlighting.pdf`
- `Acroform-PDFBOX-2333.pdf`
- `Liste732004001452_001_0.pdf_0_.pdf`
- `PDFBOX-3498-Y5TLCWTIAE3FYDVJTV2TXRZGXLEDUNSW.pdf`
- `PDFBOX-4322-Empty-ToUnicode-reduced.pdf`

### Encryption

Missing/stubbed:
- Public-key encrypted PDF decryption and writing
- Standard security handler encryption flow
- More Java-like failure diagnostics for unsupported decryption material

Observed impact:
- The encrypted/public-key load and merge failures match Java by pass/fail outcome, but .NET often reports unsupported compression instead of decryption material errors.
- Save behavior differs on `orimi-test.pdf`: .NET saves where Java rejects because the document still contains an encryption dictionary.

Relevant files:
- `src/PdfBox.Net/PDModel/Encryption/PublicKeySecurityHandler.cs`
- `src/PdfBox.Net/PDModel/Encryption/StandardSecurityHandler.cs`
- `src/PdfBox.Net/PDModel/PDDocument.cs`

### Font Embedding and Subsetting

Missing/stubbed:
- TrueType font subsetting
- `PDTrueTypeFont` subsetting
- `PDCIDFontType2` subsetting
- Upstream `FontMapperImpl.java` does not have a same-stem C# file.

Observed impact:
- This did not dominate the current runtime corpus failures, but it is a major functional gap for create/edit workflows and faithful save output.

Relevant files:
- `src/PdfBox.Net/PDModel/Font/TrueTypeEmbedder.cs`
- `src/PdfBox.Net/PDModel/Font/PDTrueTypeFontEmbedder.cs`
- `src/PdfBox.Net/PDModel/Font/PDCIDFontType2Embedder.cs`

### Image Factories

Missing/stubbed:
- `SampledImageReader.GetRGBImage`
- `CCITTFactory.CreateFromFile`
- `CCITTFactory.CreateFromStream`
- `LosslessFactory.CreateFromRawData`
- `PNGConverter.Convert`
- `CustomFactory.CreateFromRaw`

Observed impact:
- Image creation and image extraction parity is incomplete even where basic document load/save succeeds.

Relevant files:
- `src/PdfBox.Net/PDModel/Graphics/Image/SampledImageReader.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/CCITTFactory.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/LosslessFactory.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/PNGConverter.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/CustomFactory.cs`

## Priority Fix List

1. Fix `PDFTextStripper` `NullReferenceException` cases using the 6 Java-success/.NET-fail files as regression tests.
2. Add render assertions that compare against Java output or at least detect blank/near-blank placeholder renders; current render success counts are misleading.
3. Implement or integrate image decoders for DCT/JPEG, JPX/JPEG2000, JBIG2, and CCITT paths.
4. Fill the `PageDrawer` rendering TODOs for glyphs, images, shadings, annotations, form XObjects, soft masks, and transparency.
5. Align encrypted/public-key document failure paths and diagnostics with Java, then decide whether .NET should reject `orimi-test.pdf` save like Java.
6. Port or intentionally replace the 11 missing upstream classes listed above.
7. Implement font subsetting and image factory stubs for create/edit workflow parity.

## Restart Notes

Progress checkpoint for the original scan: `/tmp/pdfbox-gap-scan/PROGRESS.md`

To resume:
1. Read this report and `/tmp/pdfbox-gap-scan/PROGRESS.md`.
2. Use the clean JSONL files listed in Scope as the source of truth for existing runtime results.
3. If changing PdfBox.Net, rerun the same probes from `/tmp/pdfbox-gap-scan/src` against `/tmp/pdfbox-gap-scan/manifest.txt` and compare against the Java clean results.
