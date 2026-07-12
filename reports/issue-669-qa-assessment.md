# Issue 669 upstream-sync QA assessment

## Scope

- Previous tracked trunk: `229bd6596aa35ecb2fcd95538c6a7241f5cfbc34`
- Synced trunk: `fc00e427de8a1046efe6348d64d5529b479aea13`
- Upstream commits reviewed: `c2b9c10e`, `03edeed4`, `1fe357f1`, `7631028e`, `36c0a167`, `02c54994`, and `fc00e427`
- Production mappings changed: 19
- Upstream test sources changed: 4
- Upstream binary test fixtures changed: 1

The two final input-closing commits change Java test resource ownership only. The `TTFParser.java`
production delta in this range is JavaDoc correction for `parseTableHeaders(TTFDataStream)`; the
existing .NET parser already closes `RandomAccessRead` before returning, and that contract is now
documented and covered by `TTFParserTest`.

## Skill B sync log

All mapped files have zero `PORT-LOCAL` regions. `semantic-divergence` identifies the existing,
intentional .NET rendering/parser adaptations; none of the upstream changes conflicted with local
behavior.

| Source path | Target path | Previous sync | New sync | Conflict | Result | Local regions | Sync note |
|---|---|---|---|---|---|---:|---|
| `fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java` | `src/PdfBox.Net.FontBox/FontBox/TTF/TTFParser.cs` | `56575fd5` | `fc00e427` | semantic-divergence | in-sync | 0 | Synced changed JavaDoc and preserved the adapted parser; added XML ownership docs and a close-before-return regression. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/AxialShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/AxialShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Removed only the obsolete shared-base matrix flow; preserved the concrete matrix parameter. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/AxialShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/AxialShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/GouraudShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/GouraudShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Removed the unused protected matrix parameter and synced constructor XML docs. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PatchMeshesShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/PatchMeshesShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the concrete matrix parameter; stopped forwarding it to the shared base. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/RadialShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/RadialShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Removed only the obsolete shared-base matrix flow; preserved the concrete matrix parameter. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/RadialShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/RadialShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Removed unused matrix state and synced the narrowed constructor documentation. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/TriangleBasedShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/TriangleBasedShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Removed the unused protected matrix parameter and synced constructor XML docs. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type1ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type1ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the public matrix parameter; stopped forwarding it to the shared base. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type1ShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type1ShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type4ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type4ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the public matrix parameter; stopped forwarding it to the shared base. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type4ShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type4ShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type5ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type5ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the public matrix parameter; stopped forwarding it to the shared base. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type5ShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type5ShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type6ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type6ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the concrete matrix parameter and synced constructor XML docs. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type6ShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type6ShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type7ShadingContext.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type7ShadingContext.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | Preserved the concrete matrix parameter and synced constructor XML docs. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/Type7ShadingPaint.java` | `src/PdfBox.Net/PDModel/Graphics/Shading/Type7ShadingPaint.cs` | `ccd281cf` | `fc00e427` | semantic-divergence | in-sync | 0 | The adapted call already omitted the removed color-model argument. |

## Upstream-test parity

| Upstream test or fixture | Status | Assessment |
|---|---|---|
| Shading production constructor cleanup | converted | Existing `PDShadingTest` and `AdvancedRenderingIssue419Test.RenderImage_ShadingFill_RendersWithoutThrowing` cover mapped shading behavior; the upstream range adds no shading tests or behavior. |
| `GlyphLayoutDin91379Test.testWithEndPositionAdjustment` | deferred | No mapped AWT DIN 91379 test or Arimo fixture exists. The .NET AWT proxy exposes the new `embedSubset` overload, but a truthful visual parity test requires an operational AWT shaping backend. |
| `GlyphLayoutLigaturesAndKerningTest` | deferred | No mapped Java AWT visual test exists. Skia tests cover Thai/Bengali shaping and kerning computationally, but not the upstream two-call Bengali end-position visual assertion. |
| `GlyphLayoutLigaturesAndKerning.pdf` | deferred | The binary fixture is output from the deferred Java AWT visual test and is not copied as an unused .NET resource. |
| `PDFontTest.testPDFBox6172` | deferred | The changed Java code only closes the Noto Sans SC input. This repository has no mapped test or matching external OTF fixture for that failure path. |
| `TestFontEmbedding.testPDFBox6243` and duplicate-GID test | deferred | The Java changes only close the Noto CJK input. Equivalent tests require the unavailable CJK fixture and fuller Type 0 embedding parity; no synthetic coverage was fabricated. |
| `TTFParser` input ownership | converted | `TTFParserTest.TestMinimalTrueTypeParsesFromRandomAccessRead` now asserts that the supplied `RandomAccessRead` is closed before `Parse` returns. |

## Similarity and report-gap assessment

- Shading source similarity: **medium confidence, adapted**. The .NET classes are rendering-abstraction
  scaffolding rather than line-for-line AWT ports. The PDFBOX-5660 constructor cleanup maps cleanly,
  with concrete public signatures retained for compatibility.
- TTF parser source similarity: **high confidence for this delta, adapted overall**. The changed upstream
  production text is documentation-only; existing parser ownership behavior matches the public contract.
- Report rows: all 19 production mappings are present in conversion, normalization, and traceability
  records after this sync. No mapped production row is missing.
- Deferred tests and the unused generated PDF fixture are listed above; they do not block production-file
  traceability for this maintenance batch.

## Validation

- `PdfBox.Net.FontBox.Tests` filtered to `TTFParserTest`: 4 passed.
- `PdfBox.Net.Tests` filtered to `PDShadingTest` and the shading rendering smoke test: 47 passed.
- Full solution (`dotnet test PdfBoxNet.slnx --no-restore`): 1,546 passed, 13 skipped, 0 failed.
- API surface generator against `fc00e427` with `--fail-on-unreviewed`: passed with 916/916 deltas reviewed and no invalid or unused disposition entries.
- `git diff --check`: passed before full validation.

The optional unsupported-API audit remains red on pre-existing classifications in `PDFont`,
`PublicKeySecurityProvider`, `BuiltInEncoding`, and `SampledImageReader`. None of those files is changed
by issue 669; this sync does not widen that unrelated scope.
