# Issue 949 Upstream Sync Assessment

## Scope

- Apache PDFBox branch: `trunk`
- Previous tracked commit: `bf37c60dfa43cb9fb21497b44a667d091d809084`
- Assessed through commit: `1187c45f9dcee38ed5ac12bc15df04913b348875`
- Upstream commits reviewed: 12
- Changed upstream paths reviewed: 22
- Explicitly excluded upstream module: `pdfbox-layout-fop`

## Commit audit

| Commit | Upstream change | .NET disposition |
|---|---|---|
| `8423f12992d6489dc78d7e2f2967a8321493866e` | PDFBOX-6231 composite-glyph point-index overflow protection | Converted literally in `GlyfCompositeDescript`. |
| `bbea338208b7712bc151e2ff507764552eff03ad` | Bouncy Castle 1.85.2 BOM and Maven dependency cleanup | Maven-only; no .NET package change. `BouncyCastle.Cryptography` uses its independent NuGet version line. |
| `29924536e079f20086049ade35adb5bfd6f29677` | PDFBOX-4951 Bidi refactor and glyph-layout string widths | Converted the new abstract base and interface member; synchronized the AWT-shaped fallback and the real Skia/HarfBuzz implementation. |
| `bb678648ac6099e3a42e67954ff3ee4646a1f4e3` | Kerning width assertions | Converted into deterministic HarfBuzz/xUnit coverage. |
| `0fdfcf5c1456b5846320ce3d4176db09d24efc74` | Adds a custom-CMYK JPEG fixture | Deferred with the absent custom-CMYK `BufferedImage` creation path. |
| `2f3e569aec5de0b19369b7c09c1df6f4c0b40b14` | PDFBOX-6235 CMYK `CreateFromImage` decode array and absent-JFIF guard | Reviewed; not applicable to the port's stream-only JPEG factory and ARGB-only core image proxy. |
| `7ac2c6c0d1abb8d6f5115591e09707db7dd8f90b` | Corrects the Java JFIF metadata node name | Same deferred Java ImageIO-only region. |
| `90f94a9320c5c94b4b594356a9dda8d982b0fc94` | Forces the JDK JPEG reader in the CMYK test | Java ImageIO test infrastructure only; deferred with PDFBOX-6235. |
| `7ada890fa56eff988ea306f434461a6ec6812964` | Weak pixel comparisons and test variable cleanup | Existing .NET stream tests remain applicable; image-reencoding comparisons are deferred. |
| `ec3555619c2e733afa48986f0985b7dce9fca7ff` | Optimizes Java JPEG tests by reusing resource bytes | Test-only Java resource-lifetime cleanup; no .NET production change. |
| `3ec9455b0c4f1d730ee0553f7ce99e9b645c8504` | Rejects non-HTTP protocols in `SigUtils.openURL` | Converted as Java-shaped `OpenURL` with automatic redirects and deterministic rejection coverage. |
| `1187c45f9dcee38ed5ac12bc15df04913b348875` | Improves signature verification and self-signed output | Converted to `SignedCms`/`X509Certificate2` output, including signature algorithm and certificate subject. |

## Per-file sync log

| Source path | Target path | Previous sync commit | New sync commit | Conflict type | Result status | Local regions | Sync note |
|---|---|---|---|---|---|---:|---|
| `app/pom.xml` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Maven-only Bouncy Castle BOM consumption; no .NET build mapping. |
| `debugger/pom.xml` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Maven-only dependency version removal. |
| `examples/pom.xml` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Maven-only dependency version removal. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/ShowSignature.java` | `src/PdfBox.Net.Examples/Signature/ShowSignature.cs` | `ddef86f` | `1187c45` | semantic-divergence | in-sync | 0 | Mapped Bouncy Castle verification output to `SignedCms` and `X509Certificate2`; signature-only verification matches upstream semantics. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java` | `src/PdfBox.Net.Examples/Signature/SigUtils.cs` | `eeb5d61` | `1187c45` | semantic-divergence | in-sync | 0 | Added `OpenURL`; a shared `HttpClient` follows redirects and only HTTP(S) is accepted. |
| `fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeDescript.java` | `src/PdfBox.Net.FontBox/FontBox/TTF/GlyfCompositeDescript.cs` | `7e9effe` | `1187c45` | none | in-sync | 0 | Literal PDFBOX-6231 guard with `short.MaxValue` and structured MEL diagnostic. |
| `parent/pom.xml` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Java BOM/version management has no direct .NET package mapping. |
| `pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutProcessorAwt.java` | `src/PdfBox.Net/GlyphLayout/Awt/GlyphLayoutProcessorAwt.cs` | `56575fd` | `1187c45` | semantic-divergence | in-sync | 0 | Inherits the new base; the core fallback computes widths for its conservative Identity-H glyph codes. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutDin91379Test.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | `replaceAll` to `replace` is a Java test implementation cleanup; no mapped test contains that helper. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutLigaturesAndKerningTest.java` | `tests/PdfBox.Net.Tests/SkiaGlyphLayoutProcessorTest.cs` | `56575fd` | `1187c45` | semantic-divergence | in-sync | 0 | Ported the width assertions to the actual HarfBuzz backend; local Type 0 ordinary width is simplified, so the stable parity assertion compares kerned and unkerned shaped advances. |
| `pdfbox-layout-awt/src/test/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutSMPTest.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Java-only regex-to-literal replacement in a helper absent locally. |
| `pdfbox-layout-awt/src/test/resources/pdf/GlyphLayoutLigaturesAndKerning.pdf` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Generated Java visual reference changed with the new width guide lines; it is not a .NET golden fixture. |
| `pdfbox-layout-fop/src/main/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutProcessorFop.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Explicitly excluded Java-only FOP module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutDin91379Test.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutLigaturesAndKerningTest.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox-layout-fop/src/test/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutSMPTest.java` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Explicitly excluded with its module. |
| `pdfbox/pom.xml` | — | `bf37c60` | `1187c45` | none | in-sync | 0 | Maven-only dependency version removal. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/AbstractGlyphLayoutProcessor.java` | `src/PdfBox.Net/PDModel/AbstractGlyphLayoutProcessor.cs` | — | `1187c45` | none | in-sync | 0 | New file converted with upstream-shaped hooks and Bidi splitting; `Unicode.Bidi` stays internal to core. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/GlyphLayoutProcessorInterface.java` | `src/PdfBox.Net/PDModel/GlyphLayoutProcessorInterface.cs` | `56575fd` | `1187c45` | none | in-sync | 0 | Added `GetStringWidth`. Both known implementers were updated. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactory.java` | `src/PdfBox.Net/PDModel/Graphics/Image/JPEGFactory.cs` | `7e9effe` | `1187c45` | semantic-divergence | in-sync | 0 | The working Gray/RGB/CMYK stream factory was audited. The changed custom-CMYK image-encoding region is absent by design and explicitly deferred. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactoryTest.java` | `tests/PdfBox.Net.Tests/ImageFactoryTest.cs` | `7e9effe` | `1187c45` | semantic-divergence | in-sync | 0 | Existing stream metadata/raw-byte tests remain; Java ImageIO reencoding coverage is deferred. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/graphics/image/PDFBOX-6235-cmyk.jpg` | — | — | `1187c45` | semantic-divergence | in-sync | 0 | Not copied because the only new test requiring it targets the absent CMYK `CreateFromImage` path. |

## Upstream-test parity by production class

| Production class | Status | Evidence or reason |
|---|---|---|
| `GlyfCompositeDescript` | deferred | Upstream added no regression test or malformed-font fixture for PDFBOX-6231. The guard is a literal control-flow port and is covered by the full FontBox suite; a synthetic 32,768-point nested composite fixture is not introduced in this sync. |
| `AbstractGlyphLayoutProcessor` | converted | `AbstractGlyphLayoutProcessorTest.GetStringWidth_SplitsAndVisuallyReordersBidiRuns` directly verifies mixed-direction visual runs and width aggregation. |
| `GlyphLayoutProcessorInterface` | converted | Both repository implementers compile against `GetStringWidth`; targeted AWT-shaped/core and Skia tests execute the contract. |
| `GlyphLayoutProcessorAwt` | converted | The AWT-shaped fallback inherits the new base and compiles; real shaping parity is exercised through the optional Skia/HarfBuzz implementation used by .NET. |
| `JPEGFactory` | deferred | The new upstream test is specifically for Java `BufferedImage.TYPE_CUSTOM` CMYK reencoding. Core `BufferedImage` exposes ARGB pixels and `JPEGFactory` intentionally accepts encoded streams; stream Gray/RGB/CMYK coverage remains converted. |
| `SigUtils` | converted | `TestSigUtils.OpenURL_RejectsNonHttpProtocol` is deterministic and checks the exact FTP rejection message. Redirect behavior is delegated to `HttpClientHandler.AllowAutoRedirect`. |
| `ShowSignature` | deferred | No upstream regression test changed or was added for the two output strings. The adapted `SignedCms` path compiles and existing signature creation/verification coverage remains; this sync does not add global console-capture coupling. |

## Similarity and normalization

- The new abstract processor preserves upstream method boundaries: Bidi splitting is centralized,
  `ShowText` and `GetStringWidth` iterate the same visual runs, and subclasses implement only
  unidirectional work.
- `Unicode.Bidi` is a private implementation dependency of `PdfBox.Net.Core`; no dependency type
  appears in the public or protected PDFBox API.
- Skia/HarfBuzz width is the sum of shaped horizontal advances converted from the existing
  1/1000-text-unit representation to the requested font size. This is the closest .NET analogue
  to AWT `GlyphVector.getLogicalBounds().getWidth()`.
- The upstream Java test also compares shaped width to `PDType0Font.getStringWidth`. The current
  .NET base method is documented as a simplified character-code calculation, so the stable local
  port retains the option-independent ordinary-width assertion and the key kerning-vs-unshaped
  shaped-width assertion.
- `GlyfCompositeDescript`, previously missing from conversion and traceability ledgers, now has
  complete provenance, conversion, normalization, traceability, and comparison evidence.
- The former JPEGFactory “adapted stub” metadata was corrected: stream embedding is implemented;
  only custom-raster encoding remains deferred.
- Report-row gaps: none for changed or newly converted production sources. The CMYK image creation
  test/resource deferral is recorded explicitly rather than represented as implemented.

## Validation

- Targeted glyph-layout tests: 8 passed, 0 failed, 0 skipped.
- Targeted `SigUtils.OpenURL` test: 1 passed, 0 failed, 0 skipped.
- Release solution build: succeeded with 0 warnings and 0 errors.
- Full Release solution suite: 1,536 passed, 0 failed, 14 skipped.
- Canonical upstream inventory: 1,074 mapped / 1,074 in-scope Java production files, 0 missing,
  and 0 non-`in-sync` traceability rows.
- API-surface generator and ratchet: passed with 0 unreviewed deltas and 0 invalid dispositions.
- Changed CI workflow lint: passed with `actionlint`. A repository-wide lint also reports the
  pre-existing shellcheck findings in untouched `publish-nuget.yml` lines 103 (`SC2006` and
  `SC2091`); no changed workflow finding exists.
- Runtime parity harness compile/help smoke: passed.
- All JSON artifacts under `reports/` parse successfully; changed traceability and normalization
  rows satisfy their required schemas and all coverage/API gates passed.
- `git diff --check`: passed.
- Final upstream fetch confirmed Apache `trunk` still points to
  `1187c45f9dcee38ed5ac12bc15df04913b348875`.
