# PdfBox.Net Feature Parity Plan

Generated: 2026-06-23

Source report: `/tmp/pdfbox-gap-scan/pdfbox-net-gap-analysis-report.md`

## Goal

Reach practical 100% feature parity with the Java Apache PDFBox revision used by the scan, starting from the observed runtime and source gaps in PdfBox.Net.

The work should be tracked as capability parity, not only source-file parity. A class is not considered complete until it behaves like the corresponding Java implementation on shared regression fixtures.

## High-Level Plan

### Phase 1: Establish Parity Harness

Create a repeatable Java-vs-.NET parity test harness from the current `/tmp/pdfbox-gap-scan` probes and corpus. The harness should run load, text extraction, save/copy, render, and merge operations for both implementations and produce machine-readable diff reports.

This phase makes all later work measurable. It should include golden Java outputs or hashes where appropriate, explicit known-failure lists, and blank-render detection so success counts cannot hide placeholder output.

### Phase 2: Fix Crash-Level Runtime Gaps

Prioritize .NET failures where Java succeeds:

- `PDFTextStripper` `NullReferenceException` cases.
- `survey.pdf` rendering failure in pattern color space handling.
- `orimi-test.pdf` unsupported compression path if it is reachable through normal load/text/render behavior.

These are the fastest path to making the corpus fully runnable.

### Phase 3: Complete Filter and Image Decoding

Implement or integrate missing PDF filter support:

- CCITT Fax
- DCT/JPEG
- JPX/JPEG2000
- JBIG2

This unlocks rendering, image extraction, content processing, and better encrypted-file diagnostics.

### Phase 4: Bring Rendering to Functional Parity

Fill the major `PageDrawer` and rendering subsystem gaps:

- Glyph and Type 3 glyph rendering
- Image rendering and compositing
- Form XObjects
- Annotation appearance rendering
- Shadings and patterns
- Soft masks
- Transparency groups

Rendering should be validated by pixel comparison or perceptual/structural image checks against Java output.

### Phase 5: Bring Text Extraction to Semantic Parity

After crash fixes, reduce the 50 successful-but-different text outputs. This likely requires work in font/CMap handling, positioning, bidi/sorting behavior, whitespace heuristics, malformed content stream tolerance, and `PDFTextStripper` event processing.

### Phase 6: Align Encryption and Save Semantics

Implement or align:

- Public-key encrypted PDF handling
- Standard security handler behavior
- Java-like diagnostics for unsupported or incompatible decryption material
- Save restrictions for documents that still contain encryption dictionaries

The target is not just same pass/fail counts, but same observable semantics.

### Phase 7: Close Source Coverage Gaps

Port or intentionally replace missing upstream classes:

- `BaseParser`
- `CCITTFaxFilter`
- `SetLineMiterLimit`
- `SetLineJoinStyle`
- `SetLineCapStyle`
- `FontMapperImpl`
- `PDAnnotationStrikeout`
- `PDAnnotationRubberStamp`
- `StructuredType`
- `PropertyType`
- `Attribute`

Each class should include provenance headers, JavaDoc-to-XML documentation conversion, and focused parity tests.

### Phase 8: Complete Create/Edit Workflows

Implement font subsetting and image factory behavior:

- TrueType subsetting
- CID TrueType subsetting
- `PDTrueTypeFont` subsetting
- `SampledImageReader.GetRGBImage`
- CCITT, lossless, PNG, and custom image factories

This phase addresses functionality not heavily exposed by the current runtime corpus but required for full PDFBox API parity.

### Phase 9: Expand Corpus and Ratchet

Turn the parity corpus into a ratchet:

- Add every fixed failure as a regression fixture.
- Add upstream PDFBox issue fixtures for filters, rendering, text extraction, encryption, forms, annotations, and font embedding.
- Fail CI on new Java-vs-.NET parity regressions.
- Track temporary divergences as explicit, owned known failures.

## GitHub Issue Drafts

### Issue 1: Add Java-vs-.NET parity harness and CI report

Labels: `parity`, `testing`, `infrastructure`

#### Problem

The current gap scan exists under `/tmp/pdfbox-gap-scan`, but parity checks are not yet a first-class test workflow in the repository. Without a repeatable harness, fixes for text extraction, rendering, filters, encryption, and merge behavior cannot be measured consistently.

#### Scope

- Move or recreate the probe workflow in the repo test infrastructure.
- Run equivalent Java PDFBox and PdfBox.Net operations against the same corpus.
- Produce JSONL or structured test output for:
  - load
  - text extraction
  - save/copy
  - first-page render
  - merge
- Produce a summarized diff report.
- Support an explicit known-failures file.
- Make the harness runnable locally and in CI.

#### Acceptance Criteria

- A single documented command runs the parity suite.
- The suite compares Java and .NET results for pass/fail, exception category, output hash, and basic output metadata.
- Known divergences are listed in a tracked file with owner/reason fields.
- CI publishes or logs a concise parity summary.
- Existing scan results can be reproduced or explained by the new harness.

#### References

- `/tmp/pdfbox-gap-scan/src/JavaPdfProbe.java`
- `/tmp/pdfbox-gap-scan/src/DotnetPdfProbe.cs`
- `/tmp/pdfbox-gap-scan/manifest.txt`
- `/tmp/pdfbox-gap-scan/pdfbox-net-gap-analysis-report.md`

---

### Issue 2: Add render-output validity checks to prevent blank placeholder successes

Labels: `parity`, `rendering`, `testing`

#### Problem

.NET reported 94 successful first-page renders, but output signatures collapsed to a few repeated image sizes. This strongly suggests blank or near-blank placeholder renders are being counted as success.

#### Scope

- Add render assertions that detect blank, transparent, or near-uniform images.
- Compare basic metadata against Java output:
  - dimensions
  - non-background pixel count
  - color histogram summary
  - encoded size threshold
- Add fixtures for documents that currently render as placeholders.
- Mark expected rendering gaps explicitly until implementation catches up.

#### Acceptance Criteria

- Placeholder renders fail or are reported as known gaps.
- Render success counts distinguish "operation did not throw" from "output is meaningful".
- The parity report lists blank or near-blank renders separately.
- At least 10 current placeholder cases are captured as regression fixtures.

#### References

- `src/PdfBox.Net/Rendering/PageDrawer.cs`
- `src/PdfBox.Net/Rendering/TilingPaint.cs`
- `src/PdfBox.Net/Rendering/SoftMask.cs`

---

### Issue 3: Fix PDFTextStripper NullReferenceException failures where Java succeeds

Labels: `parity`, `text-extraction`, `bug`

#### Problem

Java text extraction succeeded for all 96 loaded PDFs. PdfBox.Net failed on 7, mostly with `NullReferenceException`.

#### Failing Fixtures

- `arxiv-sample.pdf`
- `4PP-Highlighting.pdf`
- `Acroform-PDFBOX-2333.pdf`
- `Liste732004001452_001_0.pdf_0_.pdf`
- `PDFBOX-3498-Y5TLCWTIAE3FYDVJTV2TXRZGXLEDUNSW.pdf`
- `PDFBOX-4322-Empty-ToUnicode-reduced.pdf`
- `orimi-test.pdf`

#### Scope

- Add regression tests for each Java-success/.NET-fail text extraction case.
- Diagnose the null dereference sources.
- Align missing defensive handling with Java behavior.
- Preserve Java-like behavior for malformed or edge-case content streams.

#### Acceptance Criteria

- The six `NullReferenceException` fixtures extract text without throwing.
- The seventh fixture is either fixed or categorized under the relevant filter/encryption issue if the root cause is not `PDFTextStripper`.
- Tests assert extracted text hash, non-empty text, or Java-compatible output depending on fixture stability.
- No broad exception swallowing is introduced.

#### References

- `PDFTextStripper` implementation and related content stream/font/CMap handling.
- `/tmp/pdfbox-gap-scan/dotnet-results.clean.jsonl`
- `/tmp/pdfbox-gap-scan/java-headless-results.clean.jsonl`

---

### Issue 4: Reduce PDFTextStripper semantic output mismatches against Java

Labels: `parity`, `text-extraction`

#### Problem

Among successful text extraction operations, 50 of 89 .NET text hashes differed from Java. This indicates broad semantic divergence, not only crash bugs.

#### Scope

- Categorize text mismatches by root cause:
  - whitespace and line break heuristics
  - text positioning and sorting
  - font encoding
  - ToUnicode/CMap behavior
  - bidi or vertical text handling
  - malformed content stream tolerance
- Add focused fixtures for each category.
- Align .NET behavior with Java PDFBox for default `PDFTextStripper` settings.

#### Acceptance Criteria

- Text mismatch report groups failures by actionable cause.
- A measurable ratchet is established, for example reducing mismatches from 50 to a tracked lower number.
- Fixed cases have stable regression tests.
- Differences intentionally left unresolved are documented with rationale.

#### References

- `PDFTextStripper`
- font and CMap classes under `src/PdfBox.Net/PDModel/Font`

---

### Issue 5: Implement CCITT Fax filter support

Labels: `parity`, `filters`, `images`

#### Problem

CCITT Fax decoding/encoding is missing or stubbed. Upstream `CCITTFaxFilter.java` is also missing by same-stem C# source coverage.

#### Scope

- Port or implement CCITT Fax decode and encode behavior.
- Cover Group 3 and Group 4 modes supported by Java PDFBox.
- Implement stream behavior needed by image XObjects and image factories.
- Port or replace upstream `CCITTFaxFilter`.

#### Acceptance Criteria

- CCITT-encoded image XObjects decode correctly.
- CCITT image creation paths work where Java supports them.
- Regression tests compare decoded image metadata and pixels against Java output.
- Missing-source report no longer lists `CCITTFaxFilter`.

#### References

- `src/PdfBox.Net/Filter/CCITTFaxDecodeFilter.cs`
- `src/PdfBox.Net/Filter/CCITTFaxDecoderStream.cs`
- `src/PdfBox.Net/Filter/CCITTFaxEncoderStream.cs`
- upstream `org/apache/pdfbox/filter/CCITTFaxFilter.java`

---

### Issue 6: Implement DCT/JPEG filter decode and encode parity

Labels: `parity`, `filters`, `images`, `rendering`

#### Problem

DCT/JPEG filter behavior is missing or stubbed, blocking faithful rendering and image extraction for common PDFs.

#### Scope

- Implement DCT decode using an appropriate .NET imaging backend.
- Preserve Java PDFBox behavior for color spaces, decode arrays, masks, and metadata where applicable.
- Implement encode support if exposed by the Java API surface.
- Add image extraction and rendering regression tests.

#### Acceptance Criteria

- JPEG-backed image XObjects render and extract correctly for representative fixtures.
- Java-vs-.NET decoded dimensions, color model handling, and image hashes are comparable.
- Unsupported edge cases fail with clear tracked diagnostics rather than low-level archive/compression errors.

#### References

- `src/PdfBox.Net/Filter/DCTFilter.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/SampledImageReader.cs`

---

### Issue 7: Implement JPX/JPEG2000 filter support

Labels: `parity`, `filters`, `images`

#### Problem

JPX/JPEG2000 support is missing or stubbed. This blocks rendering and extraction for PDFs using JPEG2000 image streams.

#### Scope

- Choose a .NET-compatible JPEG2000 decoder strategy.
- Implement JPX decode behavior aligned with Java PDFBox.
- Handle color space interactions and alpha where applicable.
- Add representative fixtures.

#### Acceptance Criteria

- JPX image streams decode for common fixtures.
- Rendering and extraction produce Java-compatible output metadata.
- Unsupported JPX variants are reported clearly and tracked as known gaps.

#### References

- `src/PdfBox.Net/Filter/JPXFilter.cs`

---

### Issue 8: Implement JBIG2 filter support or integration point

Labels: `parity`, `filters`, `images`

#### Problem

JBIG2 support is missing or stubbed. Java PDFBox typically relies on optional JBIG2 ImageIO plugins; PdfBox.Net needs an equivalent strategy.

#### Scope

- Decide whether to implement JBIG2 directly or integrate a maintained decoder package.
- Support global segments and common PDF JBIG2 image stream patterns.
- Match Java behavior when the optional decoder is unavailable, if applicable.
- Add tests for successful decode and unsupported decoder scenarios.

#### Acceptance Criteria

- JBIG2 fixtures either decode correctly or fail with documented Java-compatible diagnostics.
- Rendering and image extraction use the implemented decoder path.
- Optional dependency behavior is documented.

#### References

- `src/PdfBox.Net/Filter/JBIG2Filter.cs`

---

### Issue 9: Implement core PageDrawer glyph and image rendering

Labels: `parity`, `rendering`

#### Problem

The rendering subsystem reports success for many files but does not faithfully draw visible page content. Glyph rendering, image rendering, and compositing are major missing pieces.

#### Scope

- Implement glyph rendering for supported font types.
- Implement Type 3 glyph rendering.
- Implement image XObject drawing.
- Respect graphics state transforms, clipping, color spaces, masks, and alpha where currently supported.
- Add Java comparison fixtures.

#### Acceptance Criteria

- Text-only PDFs render visible glyphs comparable to Java output.
- Image-only and mixed text/image PDFs render visible image content.
- Placeholder-render count drops significantly in the parity report.
- Regression tests include pixel or perceptual comparisons with tolerances.

#### References

- `src/PdfBox.Net/Rendering/PageDrawer.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/SampledImageReader.cs`

---

### Issue 10: Implement advanced rendering: forms, annotations, shadings, patterns, soft masks, transparency

Labels: `parity`, `rendering`

#### Problem

Advanced rendering features are missing or stubbed, including annotations, form XObjects, shadings, patterns, soft masks, and transparency groups. `survey.pdf` currently fails on pattern color space conversion.

#### Scope

- Implement form XObject rendering.
- Implement annotation appearance rendering.
- Implement shading fills and pattern color spaces.
- Implement soft masks and transparency group compositing.
- Fix `survey.pdf` pattern color space failure.

#### Acceptance Criteria

- `survey.pdf` renders without throwing.
- Representative annotation, form XObject, pattern, shading, and transparency fixtures render comparable output to Java.
- Known unsupported cases are explicit and narrower than the current TODO-level gaps.

#### References

- `src/PdfBox.Net/Rendering/PageDrawer.cs`
- `src/PdfBox.Net/Rendering/TilingPaint.cs`
- `src/PdfBox.Net/Rendering/SoftMask.cs`
- `src/PdfBox.Net/PDModel/Graphics/Color/PDPattern.cs`

---

### Issue 11: Align encrypted PDF load, merge, diagnostics, and save behavior with Java

Labels: `parity`, `encryption`, `bug`

#### Problem

Java and .NET fail the same encrypted/public-key load and merge fixtures, but .NET often reports `InvalidDataException:The archive entry was compressed using an unsupported compression method.` instead of Java-like decryption diagnostics. Save behavior also differs for `orimi-test.pdf`: Java rejects saving because the document still contains an encryption dictionary, while .NET saves.

#### Scope

- Audit public-key and standard security handler behavior against Java.
- Ensure encrypted PDFs fail at the correct semantic layer.
- Align exception categories/messages where practical.
- Decide and implement Java-compatible save behavior for documents retaining encryption dictionaries.
- Add parity tests for encrypted load, text, render, save, and merge cases.

#### Acceptance Criteria

- Encrypted/public-key fixtures produce Java-compatible pass/fail outcomes and diagnostics.
- `orimi-test.pdf` save behavior is aligned with Java or explicitly documented if intentionally different.
- Lower-level compression exceptions no longer mask decryption failures.

#### References

- `src/PdfBox.Net/PDModel/Encryption/PublicKeySecurityHandler.cs`
- `src/PdfBox.Net/PDModel/Encryption/StandardSecurityHandler.cs`
- `src/PdfBox.Net/PDModel/PDDocument.cs`

---

### Issue 12: Port missing parser, graphics-state, font mapper, annotation, and XMPBox classes

Labels: `parity`, `source-coverage`, `porting`

#### Problem

Filename-level source coverage is high but not complete. The scan found 11 upstream Java classes without same-stem C# files.

#### Scope

Port or intentionally replace these classes:

- `org/apache/pdfbox/pdfparser/BaseParser.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineMiterLimit.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineJoinStyle.java`
- `org/apache/pdfbox/contentstream/operator/state/SetLineCapStyle.java`
- `org/apache/pdfbox/pdmodel/font/FontMapperImpl.java`
- `org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationStrikeout.java`
- `org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationRubberStamp.java`
- `org/apache/xmpbox/type/StructuredType.java`
- `org/apache/xmpbox/type/PropertyType.java`
- `org/apache/xmpbox/type/Attribute.java`

`CCITTFaxFilter.java` is tracked separately under the CCITT issue.

#### Acceptance Criteria

- Missing-source report no longer lists these classes unless an intentional replacement is documented.
- Each port follows repository conversion rules, including provenance headers and JavaDoc-to-XML documentation comments.
- Focused unit or parity tests cover the behavior introduced by each class.

#### References

- Apache PDFBox upstream checkout from the scan: `2589dc979`
- PdfBox.Net checkout from the scan: `a257b22`

---

### Issue 13: Implement font mapping and TrueType/CID font subsetting parity

Labels: `parity`, `fonts`, `create-edit`

#### Problem

Font subsetting is missing or stubbed, and upstream `FontMapperImpl.java` is missing by same-stem source coverage. This affects create/edit workflows and faithful save output.

#### Scope

- Port or implement Java-compatible font mapping.
- Implement TrueType font subsetting.
- Implement `PDTrueTypeFont` subsetting.
- Implement `PDCIDFontType2` subsetting.
- Add create/save/readback tests comparing Java and .NET behavior.

#### Acceptance Criteria

- Newly created PDFs with embedded TrueType fonts can subset fonts like Java PDFBox.
- CID TrueType subset behavior works for representative Unicode text.
- Saved files can be loaded and text-extracted by both Java and .NET.
- `FontMapperImpl` coverage gap is closed or intentionally replaced.

#### References

- `src/PdfBox.Net/PDModel/Font/TrueTypeEmbedder.cs`
- `src/PdfBox.Net/PDModel/Font/PDTrueTypeFontEmbedder.cs`
- `src/PdfBox.Net/PDModel/Font/PDCIDFontType2Embedder.cs`
- upstream `org/apache/pdfbox/pdmodel/font/FontMapperImpl.java`

---

### Issue 14: Complete image factory and sampled image reader parity

Labels: `parity`, `images`, `create-edit`

#### Problem

Several image factory paths and sampled-image reader behavior are missing or stubbed. This affects image extraction, image creation, rendering, and PDF authoring workflows.

#### Scope

- Implement `SampledImageReader.GetRGBImage`.
- Implement `CCITTFactory.CreateFromFile`.
- Implement `CCITTFactory.CreateFromStream`.
- Implement `LosslessFactory.CreateFromRawData`.
- Implement `PNGConverter.Convert`.
- Implement `CustomFactory.CreateFromRaw`.
- Add Java-vs-.NET tests for image creation, save, reload, render, and extract.

#### Acceptance Criteria

- Supported image factory APIs create PDFs readable by Java PDFBox and PdfBox.Net.
- Created image XObjects render correctly.
- Extracted image metadata and pixels match Java behavior within documented tolerances.
- Unsupported image inputs fail with Java-compatible diagnostics.

#### References

- `src/PdfBox.Net/PDModel/Graphics/Image/SampledImageReader.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/CCITTFactory.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/LosslessFactory.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/PNGConverter.cs`
- `src/PdfBox.Net/PDModel/Graphics/Image/CustomFactory.cs`

---

### Issue 15: Expand the parity corpus and introduce a no-regression ratchet

Labels: `parity`, `testing`, `ci`

#### Problem

The current corpus is useful but not sufficient to claim 100% feature parity. It should become a growing regression suite that covers fixed bugs and major PDFBox capabilities.

#### Scope

- Add corpus categories:
  - filters/images
  - text extraction
  - rendering
  - encryption
  - forms
  - annotations
  - fonts and subsetting
  - merge/split/save
  - malformed PDFs
- Add every fixed bug as a fixture.
- Track known failures with explicit metadata.
- Add CI ratcheting so the known-failure count cannot increase unintentionally.

#### Acceptance Criteria

- Corpus fixtures are categorized and documented.
- Parity summary includes per-category pass/fail counts.
- CI fails on new regressions unless the known-failures file is intentionally updated.
- The parity report can show trend over time.

#### References

- `/tmp/pdfbox-gap-scan/manifest.txt`
- `/tmp/pdfbox-gap-scan/pdfbox-net-gap-analysis-report.md`

