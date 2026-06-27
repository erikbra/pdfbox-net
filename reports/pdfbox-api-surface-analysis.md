# PDFBox API Surface Parity Analysis

Generated (UTC): 2026-06-27T02:17:54Z
Apache PDFBox source commit: `833ed8f378f00838fd8df8c01bfc4b915b4c350b`
PdfBox.Net commit: `0d9a6a8a20f32958788562eee761ecacefabdcb8`

## Scope

- Compared public/protected API surface from Apache PDFBox library modules: `io`, `fontbox`, `xmpbox`, and `pdfbox`.
- Java side is parsed from `**/src/main/java/**/*.java` because this environment has only Apple Java stubs, not a runnable JDK.
- .NET side is reflected from Release `net10.0` assemblies after building the core projects.
- Matching allows normal C# capitalization and JavaBean accessor-to-property mappings, and records arity drift separately from missing members.
- This is an API-shape comparison. It does not prove behavioral equivalence; the runtime parity corpus covers behavior separately.

## Summary

| Metric | Count |
|---|---:|
| Java public/protected types | 581 |
| Matched public .NET types | 579 |
| Same-name public .NET types | 573 |
| Renamed public .NET replacements | 6 |
| Mapped but non-public/replacement-marker types | 2 |
| Missing mapped public .NET types | 0 |
| Java public/protected members | 6305 |
| Matched members | 5200 |
| Arity-drift members | 46 |
| Missing members | 1059 |
| Reflected .NET extra members on matched types | 998 |

Member coverage by name/signature heuristic: **5246 / 6305 = 83.2%**.

## Review Disposition Backlog

Disposition ledger: `reports/api-surface-dispositions.json`

| Delta kind | Raw | Reviewed | Unreviewed |
|---|---:|---:|---:|
| Missing members | 1059 | 635 | 424 |
| Arity-drift members | 46 | 25 | 21 |
| Type-name/visibility gaps | 8 | 8 | 0 |
| Total reviewable deltas | 1113 | 668 | 445 |

| Disposition | Reviewed rows |
|---|---:|
| `behavior-covered` | 104 |
| `intentional-dotnet-adaptation` | 472 |
| `internal-by-design` | 69 |
| `not-applicable` | 23 |

## Module Breakdown

| Module | Java types | Same-name types | Renamed public types | Non-public/replacement types | Missing types | Java members | Matched/arity-drift members | Missing members | Member coverage |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `fontbox` | 110 | 105 | 4 | 1 | 0 | 1028 | 1009 | 19 | 98.2% |
| `io` | 15 | 15 | 0 | 0 | 0 | 131 | 126 | 5 | 96.2% |
| `pdfbox` | 391 | 390 | 0 | 1 | 0 | 4204 | 3528 | 676 | 83.9% |
| `xmpbox` | 65 | 63 | 2 | 0 | 0 | 942 | 583 | 359 | 61.9% |

## Highest Missing-Member Types

| Missing | Java members | Module | Java type | .NET type | Source |
|---:|---:|---|---|---|---|
| 108 | 149 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDLayoutAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDLayoutAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDLayoutAttributeObject.java` |
| 78 | 100 | `xmpbox` | `org.apache.xmpbox.schema.PhotoshopSchema` | `PdfBox.Net.XmpBox.Schema.PhotoshopSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PhotoshopSchema.java` |
| 60 | 80 | `xmpbox` | `org.apache.xmpbox.schema.XMPMediaManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPMediaManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPMediaManagementSchema.java` |
| 49 | 83 | `xmpbox` | `org.apache.xmpbox.schema.DublinCoreSchema` | `PdfBox.Net.XmpBox.Schema.DublinCoreSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/DublinCoreSchema.java` |
| 46 | 54 | `xmpbox` | `org.apache.xmpbox.schema.XMPSchema` | `PdfBox.Net.XmpBox.Schema.XMPSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchema.java` |
| 42 | 54 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDFTemplateStructure` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDFTemplateStructure` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateStructure.java` |
| 35 | 60 | `xmpbox` | `org.apache.xmpbox.schema.XMPBasicSchema` | `PdfBox.Net.XmpBox.Schema.XMPBasicSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicSchema.java` |
| 32 | 53 | `pdfbox` | `org.apache.pdfbox.pdfwriter.COSWriter` | `PdfBox.Net.PdfWriter.COSWriter` | `pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSWriter.java` |
| 31 | 34 | `pdfbox` | `org.apache.pdfbox.pdfparser.COSParser` | `PdfBox.Net.PdfParser.COSParser` | `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java` |
| 29 | 31 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDVisibleSigBuilder` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDVisibleSigBuilder` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSigBuilder.java` |
| 28 | 29 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDFTemplateBuilder` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDFTemplateBuilder` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateBuilder.java` |
| 27 | 32 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.COSArrayList` | `PdfBox.Net.PDModel.Common.COSArrayList` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSArrayList.java` |
| 23 | 23 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.blend.BlendMode` | `PdfBox.Net.PDModel.Graphics.BlendMode` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/blend/BlendMode.java` |
| 22 | 60 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.state.PDGraphicsState` | `PdfBox.Net.PDModel.Graphics.State.PDGraphicsState` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDGraphicsState.java` |
| 22 | 29 | `xmpbox` | `org.apache.xmpbox.schema.XMPRightsManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPRightsManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPRightsManagementSchema.java` |
| 19 | 27 | `xmpbox` | `org.apache.xmpbox.schema.PDFAIdentificationSchema` | `PdfBox.Net.XmpBox.Schema.PDFAIdentificationSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PDFAIdentificationSchema.java` |
| 18 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDExportFormatAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDExportFormatAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDExportFormatAttributeObject.java` |
| 17 | 18 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDCIDFontType0` | `PdfBox.Net.PDModel.Font.PDCIDFontType0` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java` |
| 14 | 46 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.state.PDExtendedGraphicsState` | `PdfBox.Net.PDModel.Graphics.State.PDExtendedGraphicsState` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDExtendedGraphicsState.java` |
| 14 | 40 | `xmpbox` | `org.apache.xmpbox.schema.TiffSchema` | `PdfBox.Net.XmpBox.Schema.TiffSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/TiffSchema.java` |
| 14 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1CFont` | `PdfBox.Net.PDModel.Font.PDType1CFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1CFont.java` |
| 13 | 46 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType0Font` | `PdfBox.Net.PDModel.Font.PDType0Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType0Font.java` |
| 13 | 24 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1Font` | `PdfBox.Net.PDModel.Font.PDType1Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java` |
| 12 | 16 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDListAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDListAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDListAttributeObject.java` |
| 11 | 24 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDTrueTypeFont` | `PdfBox.Net.PDModel.Font.PDTrueTypeFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFont.java` |
| 11 | 19 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.color.PDDeviceN` | `PdfBox.Net.PDModel.Graphics.Color.PDDeviceN` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceN.java` |
| 10 | 32 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.PDRectangle` | `PdfBox.Net.PDModel.Common.PDRectangle` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java` |
| 10 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDTableAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDTableAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDTableAttributeObject.java` |
| 9 | 18 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.COSDictionaryMap` | `PdfBox.Net.PDModel.Common.COSDictionaryMap` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSDictionaryMap.java` |
| 9 | 17 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDPrintFieldAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDPrintFieldAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDPrintFieldAttributeObject.java` |

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

1. Review the top missing-member types and decide which Java overloads should be preserved versus documented as intentional .NET adaptations.
2. Add compatibility overloads for stable, low-risk entry points such as `Loader`, `PDDocument`, `PDFMergerUtility`, `PDFTextStripper`, font loaders, image factories, and annotation/form models.
3. Split high-risk areas into feature issues where API shape and behavior must land together: encryption/public-key loading, external signing, image factories, rendering extension points, and font embedding/subsetting.
4. Add an API parity gate that fails only on newly introduced missing Java API rows, then ratchet reviewed gaps downward.

