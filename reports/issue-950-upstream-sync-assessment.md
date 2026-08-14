# Issue 950 Upstream 3.0 Sync Assessment

## Scope

- Apache PDFBox branch: `3.0`
- Previous tracked commit: `e5cbdeeb4adf3d2b3f7578bac953ddff5c3d4330`
- Assessed through commit: `2902dd4e5fcca22bda75327a5570c0ea9936a904`
- Upstream commits reviewed: 11
- Changed upstream paths reviewed: 24
- Explicitly excluded upstream modules: `pdfbox-layout-fop`, `preflight`, and `preflight-app`

## Commit audit

| Commit | Upstream change | .NET disposition |
|---|---|---|
| `ee6ce775a0eb83fff6ce4458fe0b986740a3c87f` | PDFBOX-6231 composite-glyph point-index overflow protection | Converted literally in `GlyfCompositeDescript`. |
| `1ad67bab1c2b405a00068623168964c1acd3e336` | Bouncy Castle 1.85.2 BOM and Maven dependency cleanup | Maven-only; no .NET package change. `BouncyCastle.Cryptography` uses its independent NuGet version line. |
| `90813af0b681b8ea7592a8ad05be470641bec13d` | PDFBOX-4951 Bidi refactor and glyph-layout string widths | Converted the new abstract base and interface member; synchronized the AWT-shaped fallback and the real Skia/HarfBuzz implementation. |
| `92768da1200d329901f97e08f30ce60b0c767ee0` | Kerning width assertions | Converted into deterministic HarfBuzz/xUnit coverage. |
| `b5e85483d002c05d2d809517ea4b36c072795a09` | Moves the Isartor fixture download to the Open Preservation Foundation corpus | Maven Preflight-only. Local CI/tests contain no reference to the old URL, and Preflight remains an explicit release-branch exclusion. |
| `caa0d72db74802f45bd457cf585ab789810a4d5d` | Adds a custom-CMYK JPEG fixture | Deferred with the absent custom-CMYK `BufferedImage` creation path. |
| `cc19f8b934f9469c1e2d7f69f57b76bfc50e979e` | PDFBOX-6235 CMYK `CreateFromImage` decode array and absent-JFIF guard | Reviewed; not applicable to the port's stream-only JPEG factory and ARGB-only core image proxy. |
| `9513f9011eb9e54f8c2d71797f2e08ae28bda8d5` | Forces the JDK JPEG reader in the CMYK test | Java ImageIO test infrastructure only; deferred with PDFBOX-6235. |
| `63771867cd6a81446a5bcc6943a6552408506d61` | Uses stream-decoded output for weak CMYK pixel comparison | Java ImageIO re-encoding coverage remains deferred; existing .NET encoded-stream tests remain applicable. |
| `762b7e02d22be40c8d8ecab98474444b8676a8f3` | Reuses JPEG resource bytes in Java tests | Test-only Java resource-lifetime cleanup; no .NET production change. |
| `2902dd4e5fcca22bda75327a5570c0ea9936a904` | Rejects non-HTTP protocols in `SigUtils.openURL` | Converted as Java-shaped `OpenURL` with URI scheme validation, automatic redirects, and deterministic rejection coverage. The accidentally duplicated, malformed upstream conditional was normalized to one valid protocol guard rather than copied. |

## Per-file sync log

| Source path | Target path | Previous sync commit | New sync commit | Conflict type | Result status | Local regions | Sync note |
|---|---|---|---|---|---|---:|---|
| `app/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only Bouncy Castle BOM consumption; no .NET build mapping. |
| `debugger-app/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only dependency version removal. |
| `debugger/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only dependency version removal. |
| `examples/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only dependency version removal. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java` | `src/PdfBox.Net.Examples/Signature/SigUtils.cs` | `eeb5d611` | `2902dd4e` | semantic-divergence | in-sync | 0 | Added `OpenURL`; a shared `HttpClient` follows redirects and only URI schemes HTTP(S) are accepted. Normalized the malformed duplicated upstream guard. |
| `fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeDescript.java` | `src/PdfBox.Net.FontBox/FontBox/TTF/GlyfCompositeDescript.cs` | `7e9effef` | `2902dd4e` | none | in-sync | 0 | Literal PDFBOX-6231 guard with `short.MaxValue` and structured MEL diagnostic. |
| `parent/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Java BOM/version management has no direct .NET package mapping. |
| `pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutProcessorAwt.java` | `src/PdfBox.Net/GlyphLayout/Awt/GlyphLayoutProcessorAwt.cs` | `ddb7e789` | `2902dd4e` | semantic-divergence | in-sync | 0 | Inherits the new base; the core fallback computes widths for its conservative Identity-H glyph codes. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutDin91379Test.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | `replaceAll` to `replace` is a Java test implementation cleanup; no mapped test contains that helper. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutLigaturesAndKerningTest.java` | `tests/PdfBox.Net.Tests/SkiaGlyphLayoutProcessorTest.cs` | `ddb7e789` | `2902dd4e` | semantic-divergence | in-sync | 0 | Ported the width assertions to the actual HarfBuzz backend; local Type 0 ordinary width is simplified, so the stable parity assertion compares kerned and unkerned shaped advances. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutSMPTest.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Java-only regex-to-literal replacement in a helper absent locally. |
| `pdfbox-layout-awt/src/test/resources/pdf/GlyphLayoutLigaturesAndKerning.pdf` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Generated Java visual reference changed with the new width guide lines; it is not a .NET golden fixture. |
| `pdfbox-layout-fop/src/main/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutProcessorFop.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Explicitly excluded Java-only FOP module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutDin91379Test.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutLigaturesAndKerningTest.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutSMPTest.java` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only dependency version removal. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/AbstractGlyphLayoutProcessor.java` | `src/PdfBox.Net/PDModel/AbstractGlyphLayoutProcessor.cs` | — | `2902dd4e` | none | in-sync | 0 | New file converted with upstream-shaped hooks and Bidi splitting; `Unicode.Bidi` stays internal to core. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/GlyphLayoutProcessorInterface.java` | `src/PdfBox.Net/PDModel/GlyphLayoutProcessorInterface.cs` | `ddb7e789` | `2902dd4e` | none | in-sync | 0 | Added `GetStringWidth`; both repository implementers were updated. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactory.java` | `src/PdfBox.Net/PDModel/Graphics/Image/JPEGFactory.cs` | `7e9effef` | `2902dd4e` | semantic-divergence | in-sync | 0 | The working Gray/RGB/CMYK stream factory was audited. The changed custom-CMYK image-encoding region is absent by design and explicitly deferred. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactoryTest.java` | `tests/PdfBox.Net.Tests/ImageFactoryTest.cs` | `7e9effef` | `2902dd4e` | semantic-divergence | in-sync | 0 | Existing stream metadata/raw-byte tests remain; Java ImageIO re-encoding coverage is deferred. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/graphics/image/PDFBOX-6235-cmyk.jpg` | — | — | `2902dd4e` | semantic-divergence | in-sync | 0 | Not copied because the only new test requiring it targets the absent CMYK `CreateFromImage` path. |
| `preflight-app/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Maven-only Bouncy Castle version removal in an explicitly excluded module. |
| `preflight/pom.xml` | — | `e5cbdeeb` | `2902dd4e` | none | in-sync | 0 | Isartor fixture URL change is Maven-only in an explicitly excluded module; no local CI/test uses the old URL. |

## Upstream-test parity by production class

| Production class | Status | Evidence or reason |
|---|---|---|
| `GlyfCompositeDescript` | deferred | Upstream added no regression test or malformed-font fixture for PDFBOX-6231. The guard is a literal control-flow port and is covered by the full FontBox suite; a synthetic 32,768-point nested composite fixture is not introduced in this sync. |
| `AbstractGlyphLayoutProcessor` | converted | `AbstractGlyphLayoutProcessorTest.GetStringWidth_SplitsAndVisuallyReordersBidiRuns` directly verifies mixed-direction visual runs and width aggregation. |
| `GlyphLayoutProcessorInterface` | converted | Both repository implementers compile against `GetStringWidth`; targeted core and Skia tests execute the contract. |
| `GlyphLayoutProcessorAwt` | converted | The AWT-shaped fallback inherits the new base and compiles; real shaping parity is exercised through the optional Skia/HarfBuzz implementation used by .NET. |
| `JPEGFactory` | deferred | The new upstream test is specifically for Java `BufferedImage.TYPE_CUSTOM` CMYK re-encoding. Core `BufferedImage` exposes ARGB pixels and `JPEGFactory` intentionally accepts encoded streams; stream Gray/RGB/CMYK coverage remains converted. |
| `SigUtils` | converted | `TestSigUtils.OpenURL_RejectsNonHttpProtocol` is deterministic and checks the exact FTP rejection message. Redirect behavior is delegated to `HttpClientHandler.AllowAutoRedirect`. |

## Similarity and normalization

- The new abstract processor preserves upstream method boundaries: Bidi splitting is centralized,
  `ShowText` and `GetStringWidth` iterate the same visual runs, and subclasses implement only
  unidirectional work.
- `Unicode.Bidi` is a private implementation dependency of `PdfBox.Net.Core`; no dependency type
  appears in the public or protected PDFBox API.
- Skia/HarfBuzz width is the sum of shaped horizontal advances converted from the existing
  1/1000-text-unit representation to the requested font size. This is the closest .NET analogue
  to AWT `GlyphVector.getLogicalBounds().getWidth()`.
- The upstream Java width test also compares shaped width to `PDType0Font.getStringWidth`. The
  current .NET base method is a simplified character-code calculation, so the stable local port
  retains the option-independent ordinary-width assertion and the key kerning-vs-unshaped shaped-width assertion.
- The upstream `SigUtils.java` head is malformed by a duplicated nested `if`. The port retains the
  intended scheme restriction with one valid `Uri.Scheme` guard and records that compile-oriented
  normalization explicitly in the normalization and traceability ledgers.
- `GlyfCompositeDescript`, previously missing from conversion and traceability ledgers, now has
  complete provenance, conversion, normalization, traceability, and comparison evidence.
- The former JPEGFactory "adapted stub" metadata was corrected: stream embedding is implemented;
  only custom-raster encoding remains deferred.
- The one pre-existing conversion-shaped row in the traceability ledger was normalized to the
  required traceability schema. No changed or newly converted production source has a report-row gap.

## Validation

- Targeted glyph-layout tests: 10 passed, 0 failed, 0 skipped.
- Targeted `SigUtils.OpenURL` test: 1 passed, 0 failed, 0 skipped.
- Release solution build: succeeded with 0 warnings and 0 errors.
- Full Release solution suite: 1,470 passed, 0 failed, 7 skipped.
- Canonical upstream inventory: 1,078 mapped / 1,078 in-scope Java production files, 0 missing.
- API-surface generator and ratchet: passed with 0 unreviewed deltas and 0 invalid dispositions.
- Changed CI workflow lint: passed with `actionlint`.
- Runtime parity harness compile/help smoke: passed.
- All 28 JSON artifacts under `reports/` parse successfully; traceability and normalization rows satisfy their required schemas.
- `git diff --check`: passed.
- Final upstream query confirmed Apache `3.0` still points to `2902dd4e5fcca22bda75327a5570c0ea9936a904`.
