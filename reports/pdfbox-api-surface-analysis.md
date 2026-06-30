# PDFBox API Surface Parity Analysis

Generated (UTC): 2026-06-30T16:15:21Z
Apache PDFBox source commit: `ea68b6feae80e671b3d26565b12eccc79e74d967`
PdfBox.Net commit: `07ab46a18c6329ac9dddb2d3610a4fc681066c28`

## Scope

- Compared public/protected API surface from Apache PDFBox library modules: `io`, `fontbox`, `xmpbox`, and `pdfbox`.
- Java side is parsed from `**/src/main/java/**/*.java` because this environment has only Apple Java stubs, not a runnable JDK.
- .NET side is reflected from Release `net10.0` assemblies after building the core projects.
- Matching allows normal C# capitalization and JavaBean accessor-to-property mappings, and records arity drift separately from missing members.
- This is an API-shape comparison. It does not prove behavioral equivalence; the runtime parity corpus covers behavior separately.

## Summary

| Metric | Count |
|---|---:|
| Java public/protected types | 579 |
| Matched public .NET types | 577 |
| Same-name public .NET types | 571 |
| Renamed public .NET replacements | 6 |
| Mapped but non-public/replacement-marker types | 2 |
| Missing mapped public .NET types | 0 |
| Java public/protected members | 6261 |
| Matched members | 5349 |
| Arity-drift members | 45 |
| Missing members | 867 |
| Reflected .NET extra members on matched types | 1700 |

Member coverage by name/signature heuristic: **5394 / 6261 = 86.2%**.

## Review Disposition Backlog

Disposition ledger: `reports/api-surface-dispositions.json`

| Delta kind | Raw | Reviewed | Unreviewed |
|---|---:|---:|---:|
| Missing members | 867 | 867 | 0 |
| Arity-drift members | 45 | 45 | 0 |
| Type-name/visibility gaps | 8 | 8 | 0 |
| Total reviewable deltas | 920 | 920 | 0 |

| Disposition | Reviewed rows |
|---|---:|
| `behavior-covered` | 116 |
| `intentional-dotnet-adaptation` | 710 |
| `internal-by-design` | 69 |
| `not-applicable` | 25 |

Unused disposition keys: **20**. See `review.unused_disposition_keys` in the JSON report.

## API Ratchet

Ratchet baseline: `reports/api-surface-ratchet-baseline.json`

- CI fails when new API deltas are unreviewed or when reviewed gap counts exceed the ratchet baseline.
- Lower the ratchet baseline whenever compatibility overloads reduce missing, arity-drift, or type-name/visibility gaps.
- Use `reports/api-compatibility-backlog-2026-06-28.md` as the family-level backlog for issue #533 follow-up work.

## Module Breakdown

| Module | Java types | Same-name types | Renamed public types | Non-public/replacement types | Missing types | Java members | Matched/arity-drift members | Missing members | Member coverage |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `fontbox` | 109 | 104 | 4 | 1 | 0 | 976 | 949 | 27 | 97.2% |
| `io` | 15 | 15 | 0 | 0 | 0 | 131 | 126 | 5 | 96.2% |
| `pdfbox` | 390 | 389 | 0 | 1 | 0 | 4209 | 3726 | 483 | 88.5% |
| `xmpbox` | 65 | 63 | 2 | 0 | 0 | 945 | 593 | 352 | 62.8% |

## Highest Missing-Member Types

| Missing | Java members | Module | Java type | .NET type | Source |
|---:|---:|---|---|---|---|
| 108 | 149 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDLayoutAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDLayoutAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDLayoutAttributeObject.java` |
| 78 | 100 | `xmpbox` | `org.apache.xmpbox.schema.PhotoshopSchema` | `PdfBox.Net.XmpBox.Schema.PhotoshopSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PhotoshopSchema.java` |
| 62 | 82 | `xmpbox` | `org.apache.xmpbox.schema.XMPMediaManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPMediaManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPMediaManagementSchema.java` |
| 49 | 83 | `xmpbox` | `org.apache.xmpbox.schema.DublinCoreSchema` | `PdfBox.Net.XmpBox.Schema.DublinCoreSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/DublinCoreSchema.java` |
| 46 | 54 | `xmpbox` | `org.apache.xmpbox.schema.XMPSchema` | `PdfBox.Net.XmpBox.Schema.XMPSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchema.java` |
| 35 | 60 | `xmpbox` | `org.apache.xmpbox.schema.XMPBasicSchema` | `PdfBox.Net.XmpBox.Schema.XMPBasicSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicSchema.java` |
| 32 | 52 | `pdfbox` | `org.apache.pdfbox.pdfwriter.COSWriter` | `PdfBox.Net.PdfWriter.COSWriter` | `pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSWriter.java` |
| 27 | 32 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.COSArrayList` | `PdfBox.Net.PDModel.Common.COSArrayList` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSArrayList.java` |
| 24 | 27 | `pdfbox` | `org.apache.pdfbox.pdfparser.COSParser` | `PdfBox.Net.PdfParser.COSParser` | `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java` |
| 23 | 23 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.blend.BlendMode` | `PdfBox.Net.PDModel.Graphics.BlendMode` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/blend/BlendMode.java` |
| 22 | 29 | `xmpbox` | `org.apache.xmpbox.schema.XMPRightsManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPRightsManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPRightsManagementSchema.java` |
| 19 | 27 | `xmpbox` | `org.apache.xmpbox.schema.PDFAIdentificationSchema` | `PdfBox.Net.XmpBox.Schema.PDFAIdentificationSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PDFAIdentificationSchema.java` |
| 18 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDExportFormatAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDExportFormatAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDExportFormatAttributeObject.java` |
| 17 | 18 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDCIDFontType0` | `PdfBox.Net.PDModel.Font.PDCIDFontType0` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java` |
| 16 | 16 | `fontbox` | `org.apache.fontbox.cff.CharStringCommand` | `PdfBox.Net.FontBox.CFF.CharStringCommand` | `fontbox/src/main/java/org/apache/fontbox/cff/CharStringCommand.java` |
| 14 | 40 | `xmpbox` | `org.apache.xmpbox.schema.TiffSchema` | `PdfBox.Net.XmpBox.Schema.TiffSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/TiffSchema.java` |
| 14 | 23 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1CFont` | `PdfBox.Net.PDModel.Font.PDType1CFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1CFont.java` |
| 13 | 47 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType0Font` | `PdfBox.Net.PDModel.Font.PDType0Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType0Font.java` |
| 13 | 25 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1Font` | `PdfBox.Net.PDModel.Font.PDType1Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java` |
| 12 | 16 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDListAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDListAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDListAttributeObject.java` |
| 11 | 25 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDTrueTypeFont` | `PdfBox.Net.PDModel.Font.PDTrueTypeFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFont.java` |
| 10 | 32 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.PDRectangle` | `PdfBox.Net.PDModel.Common.PDRectangle` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java` |
| 9 | 18 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.COSDictionaryMap` | `PdfBox.Net.PDModel.Common.COSDictionaryMap` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSDictionaryMap.java` |
| 9 | 17 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDPrintFieldAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDPrintFieldAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDPrintFieldAttributeObject.java` |
| 8 | 11 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDFourColours` | `PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf.PDFourColours` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDFourColours.java` |
| 7 | 49 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.StandardStructureTypes` | `PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf.StandardStructureTypes` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/StandardStructureTypes.java` |
| 7 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDTableAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDTableAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDTableAttributeObject.java` |
| 6 | 25 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.image.PDImage` | `PdfBox.Net.PDModel.Graphics.Image.PDImage` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDImage.java` |
| 6 | 24 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType3Font` | `PdfBox.Net.PDModel.Font.PDType3Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType3Font.java` |
| 6 | 12 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.color.PDDeviceCMYK` | `PdfBox.Net.PDModel.Graphics.Color.PDDeviceCMYK` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceCMYK.java` |

## Java-Named Public API Type Gaps

| Status | Module | Java type | .NET type | Members | Source |
|---|---|---|---|---:|---|
| mapped but non-public/replacement | `fontbox` | `org.apache.fontbox.ttf.SubstitutingCmapLookup` | `SubstitutingCmapLookup` | 3 | `fontbox/src/main/java/org/apache/fontbox/ttf/SubstitutingCmapLookup.java` |
| mapped but non-public/replacement | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationUnknown` | `PDAnnotationUnknown` | 1 | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationUnknown.java` |
| renamed public replacement | `fontbox` | `org.apache.fontbox.ttf.gsub.GlyphArraySplitter` | `PdfBox.Net.FontBox.TTF.GSub.IGlyphArraySplitter` | 1 | `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GlyphArraySplitter.java` |
| renamed public replacement | `fontbox` | `org.apache.fontbox.ttf.gsub.GsubWorker` | `PdfBox.Net.FontBox.TTF.GSub.IGsubWorker` | 1 | `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorker.java` |
| renamed public replacement | `fontbox` | `org.apache.fontbox.ttf.model.GsubData` | `PdfBox.Net.FontBox.TTF.Model.IGsubData` | 6 | `fontbox/src/main/java/org/apache/fontbox/ttf/model/GsubData.java` |
| renamed public replacement | `fontbox` | `org.apache.fontbox.ttf.model.ScriptFeature` | `PdfBox.Net.FontBox.TTF.Model.IScriptFeature` | 4 | `fontbox/src/main/java/org/apache/fontbox/ttf/model/ScriptFeature.java` |
| renamed public replacement | `xmpbox` | `org.apache.xmpbox.type.PropertyType` | `PdfBox.Net.XmpBox.Type.PropertyTypeAttribute` | 1 | `xmpbox/src/main/java/org/apache/xmpbox/type/PropertyType.java` |
| renamed public replacement | `xmpbox` | `org.apache.xmpbox.type.StructuredType` | `PdfBox.Net.XmpBox.Type.StructuredTypeAttribute` | 2 | `xmpbox/src/main/java/org/apache/xmpbox/type/StructuredType.java` |

## Assessment

- The port has complete source-file coverage for the scoped library modules, and the current runtime corpus is green, but the Java-compatible API surface is not yet complete.
- The largest API-shape gaps are public/protected overloads and extension points, especially where Java exposes `File`, `InputStream`, `RandomAccessRead`, AWT, collection, and checked-exception-oriented signatures that were narrowed or adapted in C#.
- Missing members in a matched type do not automatically mean the underlying feature is absent; some are deliberate .NET idioms or overload collapses. They do identify places where Java client code cannot be mechanically ported without adaptation.
- Renamed public replacements and non-public compatibility markers are source-coverage wins but Java API-compatibility gaps unless the project intentionally documents them as .NET-only API design.
- Arity-drift rows require manual review: the member name exists in .NET, but overload coverage does not match Java.
- The machine-readable detail in `reports/api-surface-comparison.json` should be used as the backlog seed for API compatibility issues, with behavioral parity tests added before marking each family complete.

## Next API-Parity Work

1. Use `reports/api-compatibility-backlog-2026-06-28.md` to work API compatibility by module/family.
2. Add compatibility overloads for stable, low-risk entry points such as `Loader`, `PDDocument`, `PDFMergerUtility`, `PDFTextStripper`, font loaders, image factories, and annotation/form models.
3. Keep harmful or misleading Java-shape APIs documented as accepted .NET adaptations in `reports/api-surface-dispositions.json`.
4. Lower `reports/api-surface-ratchet-baseline.json` after each compatibility PR that reduces reviewed gaps.

