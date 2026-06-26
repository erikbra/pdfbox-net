# PDFBox API Surface Parity Analysis

Generated (UTC): 2026-06-26T22:55:36Z
Apache PDFBox source commit: `833ed8f378f00838fd8df8c01bfc4b915b4c350b`
PdfBox.Net commit: `aa2b54d220846e85d52dd3b3fb3cca7f85750f81`

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
| Matched members | 4651 |
| Arity-drift members | 105 |
| Missing members | 1549 |
| Reflected .NET extra members on matched types | 963 |

Member coverage by name/signature heuristic: **4756 / 6305 = 75.4%**.

## Review Disposition Backlog

Disposition ledger: `reports/api-surface-dispositions.json`

| Delta kind | Raw | Reviewed | Unreviewed |
|---|---:|---:|---:|
| Missing members | 1549 | 359 | 1190 |
| Arity-drift members | 105 | 5 | 100 |
| Type-name/visibility gaps | 8 | 2 | 6 |
| Total reviewable deltas | 1662 | 366 | 1296 |

| Disposition | Reviewed rows |
|---|---:|
| `intentional-dotnet-adaptation` | 365 |
| `not-applicable` | 1 |

## Module Breakdown

| Module | Java types | Same-name types | Renamed public types | Non-public/replacement types | Missing types | Java members | Matched/arity-drift members | Missing members | Member coverage |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `fontbox` | 110 | 105 | 4 | 1 | 0 | 1028 | 881 | 147 | 85.7% |
| `io` | 15 | 15 | 0 | 0 | 0 | 131 | 126 | 5 | 96.2% |
| `pdfbox` | 391 | 390 | 0 | 1 | 0 | 4204 | 3166 | 1038 | 75.3% |
| `xmpbox` | 65 | 63 | 2 | 0 | 0 | 942 | 583 | 359 | 61.9% |

## Highest Missing-Member Types

| Missing | Java members | Module | Java type | .NET type | Source |
|---:|---:|---|---|---|---|
| 108 | 149 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDLayoutAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDLayoutAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDLayoutAttributeObject.java` |
| 78 | 100 | `xmpbox` | `org.apache.xmpbox.schema.PhotoshopSchema` | `PdfBox.Net.XmpBox.Schema.PhotoshopSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PhotoshopSchema.java` |
| 62 | 65 | `fontbox` | `org.apache.fontbox.afm.AFMParser` | `PdfBox.Net.FontBox.AFM.AFMParser` | `fontbox/src/main/java/org/apache/fontbox/afm/AFMParser.java` |
| 60 | 80 | `xmpbox` | `org.apache.xmpbox.schema.XMPMediaManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPMediaManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPMediaManagementSchema.java` |
| 49 | 83 | `xmpbox` | `org.apache.xmpbox.schema.DublinCoreSchema` | `PdfBox.Net.XmpBox.Schema.DublinCoreSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/DublinCoreSchema.java` |
| 46 | 54 | `xmpbox` | `org.apache.xmpbox.schema.XMPSchema` | `PdfBox.Net.XmpBox.Schema.XMPSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchema.java` |
| 42 | 54 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDFTemplateStructure` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDFTemplateStructure` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateStructure.java` |
| 35 | 60 | `xmpbox` | `org.apache.xmpbox.schema.XMPBasicSchema` | `PdfBox.Net.XmpBox.Schema.XMPBasicSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicSchema.java` |
| 32 | 53 | `pdfbox` | `org.apache.pdfbox.pdfwriter.COSWriter` | `PdfBox.Net.PdfWriter.COSWriter` | `pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSWriter.java` |
| 32 | 40 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationLine` | `PdfBox.Net.PDModel.Interactive.Annotation.PDAnnotationLine` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLine.java` |
| 31 | 34 | `pdfbox` | `org.apache.pdfbox.pdfparser.COSParser` | `PdfBox.Net.PdfParser.COSParser` | `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java` |
| 29 | 31 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDVisibleSigBuilder` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDVisibleSigBuilder` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSigBuilder.java` |
| 28 | 29 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.digitalsignature.visible.PDFTemplateBuilder` | `PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible.PDFTemplateBuilder` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateBuilder.java` |
| 27 | 32 | `pdfbox` | `org.apache.pdfbox.pdmodel.common.COSArrayList` | `PdfBox.Net.PDModel.Common.COSArrayList` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSArrayList.java` |
| 23 | 23 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.blend.BlendMode` | `PdfBox.Net.PDModel.Graphics.BlendMode` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/blend/BlendMode.java` |
| 22 | 60 | `pdfbox` | `org.apache.pdfbox.pdmodel.graphics.state.PDGraphicsState` | `PdfBox.Net.PDModel.Graphics.State.PDGraphicsState` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDGraphicsState.java` |
| 22 | 46 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType0Font` | `PdfBox.Net.PDModel.Font.PDType0Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType0Font.java` |
| 22 | 29 | `xmpbox` | `org.apache.xmpbox.schema.XMPRightsManagementSchema` | `PdfBox.Net.XmpBox.Schema.XMPRightsManagementSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPRightsManagementSchema.java` |
| 20 | 30 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationText` | `PdfBox.Net.PDModel.Interactive.Annotation.PDAnnotationText` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationText.java` |
| 20 | 26 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationFreeText` | `PdfBox.Net.PDModel.Interactive.Annotation.PDAnnotationFreeText` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationFreeText.java` |
| 19 | 27 | `xmpbox` | `org.apache.xmpbox.schema.PDFAIdentificationSchema` | `PdfBox.Net.XmpBox.Schema.PDFAIdentificationSchema` | `xmpbox/src/main/java/org/apache/xmpbox/schema/PDFAIdentificationSchema.java` |
| 18 | 24 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1Font` | `PdfBox.Net.PDModel.Font.PDType1Font` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java` |
| 18 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.documentinterchange.taggedpdf.PDExportFormatAttributeObject` | `PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure.PDExportFormatAttributeObject` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDExportFormatAttributeObject.java` |
| 18 | 22 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDType1CFont` | `PdfBox.Net.PDModel.Font.PDType1CFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1CFont.java` |
| 17 | 19 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationRubberStamp` | `PdfBox.Net.PDModel.Interactive.Annotation.PDAnnotationRubberStamp` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationRubberStamp.java` |
| 17 | 18 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDCIDFontType0` | `PdfBox.Net.PDModel.Font.PDCIDFontType0` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java` |
| 16 | 26 | `pdfbox` | `org.apache.pdfbox.pdmodel.interactive.annotation.PDAnnotationMarkup` | `PdfBox.Net.PDModel.Interactive.Annotation.PDAnnotationMarkup` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationMarkup.java` |
| 16 | 24 | `pdfbox` | `org.apache.pdfbox.pdmodel.font.PDTrueTypeFont` | `PdfBox.Net.PDModel.Font.PDTrueTypeFont` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFont.java` |
| 16 | 20 | `fontbox` | `org.apache.fontbox.ttf.CmapTable` | `PdfBox.Net.FontBox.TTF.CmapTable` | `fontbox/src/main/java/org/apache/fontbox/ttf/CmapTable.java` |
| 14 | 49 | `pdfbox` | `org.apache.pdfbox.pdmodel.PDPage` | `PdfBox.Net.PDModel.PDPage` | `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java` |

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

