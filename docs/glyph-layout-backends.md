# Glyph Layout Backends

Status: 2026-08-01

## Upstream PDFBox functionality

Apache PDFBox added an opt-in glyph-layout pipeline under PDFBOX-4951 in July
2026. The work is present on both the Apache `trunk` and `3.0` branches and is
currently part of their `4.0.0-SNAPSHOT` and `3.0.9-SNAPSHOT` development
lines.

The upstream work has three parts:

1. Core interfaces and content-stream integration in the `pdfbox` module.
   `PDPageContentStream`, appearance streams, and AcroForm appearance
   generation can delegate Unicode text to a registered glyph-layout
   processor.
2. The optional `pdfbox-layout-awt` module, which shapes text using Java AWT
   font APIs.
3. The optional `pdfbox-layout-fop` module, which shapes text using Apache FOP
   2.11 font classes.

The FOP module does not add XSL-FO document layout, paragraph flow, line
breaking, or page composition to PDFBox. It uses FOP's font machinery to map a
Unicode string to positioned glyph IDs before PDFBox writes that string into a
PDF content stream. This supports capabilities such as OpenType substitutions
and positioning, ligatures, kerning, combining marks, complex scripts,
bidirectional text, and supplementary Unicode-plane characters. The same
processor can be propagated into AcroForm appearance generation.

The upstream FOP implementation is opt-in. Existing `showText` behavior is
used when no processor is registered or when the processor does not support
the selected font. It currently targets Type 0 fonts backed by TrueType
outlines; CFF-based OpenType fonts are outside its supported path.

References:

- [PDFBOX-4951](https://issues.apache.org/jira/browse/PDFBOX-4951)
- [Apache PDFBox `pdfbox-layout-fop` module](https://github.com/apache/pdfbox/tree/3.0/pdfbox-layout-fop)
- [Commit adding the FOP backend to the Apache 3.0 branch](https://github.com/apache/pdfbox/commit/bb497fe1543c0747f92b4e613788e3297c29c3a7)

## PDFBox.Net mapping

Apache FOP is a Java library, so `pdfbox-layout-fop` is intentionally not
ported as a literal .NET module and remains an explicit exclusion in the
source-parity inventory. This is a runtime-specific adaptation, not a decision
to omit glyph shaping.

The .NET functional counterpart is `SkiaGlyphLayoutProcessor` in the optional
`PdfBox.Net.SkiaSharp` package. It uses HarfBuzzSharp for shaping and SkiaSharp
for font loading while implementing the upstream core
`GlyphLayoutProcessorInterface`. It:

- registers the same TrueType font with PDFBox.Net and HarfBuzz;
- resolves bidirectional text into visual runs;
- emits shaped glyph IDs instead of raw Unicode values;
- writes kerning, advances, and horizontal/vertical offsets as PDF text
  positioning operations; and
- keeps SkiaSharp and HarfBuzzSharp types out of the core `PdfBox.Net` API.

This backend shapes Unicode text while creating new content streams and form
appearances. It does not reshape glyph codes already present in an existing PDF
during rendering. Existing PDF content already records the selected glyphs and
positions; reshaping it would risk changing the author's content.

## Branch support matrix

| Capability | `main` | `release/3.0` |
|---|---:|---:|
| Core glyph-layout interfaces and content-stream hooks | Yes | Yes |
| Upstream-compatible AWT-facing API adaptation | Yes | Yes |
| SkiaSharp/HarfBuzz shaping backend | Yes | Yes |
| Literal Apache FOP runtime dependency | No, intentionally replaced | No, intentionally replaced |

The `release/3.0` branch includes the same optional SkiaSharp/HarfBuzz shaping
backend as `main`. Applications can therefore opt into complex-script shaping
without adding Apache FOP or exposing SkiaSharp/HarfBuzzSharp types through the
core PDFBox.Net API.

## `release/3.0` backport status

The backend was backported in [issue #929](https://github.com/erikbra/pdfbox-net/issues/929)
and merged through [PR #931](https://github.com/erikbra/pdfbox-net/pull/931).
The backport includes:

1. `SkiaGlyphLayoutProcessor` and `BidiTextRunResolver`.
2. HarfBuzzSharp native assets for Linux, macOS, and Windows plus
   `Unicode.Bidi` references in `PdfBox.Net.SkiaSharp`.
3. Generated-content tests for Latin kerning, Bengali
   substitution, Thai mark positioning, missing glyphs, and representative
   LTR/RTL visual runs, including font-stream lifetime coverage.

Before merge, all eight focused glyph-layout tests passed, the full solution
reported 1,462 passing and 7 skipped tests, package inspection confirmed the
native runtime dependencies, and the API-surface gate passed. The Java/.NET
runtime-parity suite reported 1,027 matches across 161 PDFs with no unexpected
divergences; the same build, test, package, API, and runtime gates passed in CI.
