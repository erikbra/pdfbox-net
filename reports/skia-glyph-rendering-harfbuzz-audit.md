# SkiaSharp Glyph Rendering HarfBuzz Audit

Issue: #618

## Summary

`PdfBox.Net.SkiaSharp` has three different text-related responsibilities, and
they should not all use HarfBuzz in the same way:

1. Rendering existing PDF glyph streams.
2. Drawing fallback Unicode text when the PDF font cannot provide a usable path.
3. Creating new PDF content streams from Unicode text.

The safe first implementation slice is to add HarfBuzz shaping to generated
content streams, where the input is still Unicode text and the output is a Type
0 font glyph stream. Rendering existing PDFs should continue to prefer the PDF
glyph codes, positions, and font outlines that are already present in the
content stream.

## Rendering Path Inventory

### `ShowFontGlyph(...)`

Location: `src/PdfBox.Net.SkiaSharp/Rendering/SkiaPageDrawerPeer.cs`

This is the main renderer entry point for glyphs received from the PDF content
stream. At this point the PDF stream engine has already decoded a character
code and has already applied text positioning. For vector fonts, the renderer
uses `GlyphCache` and `PDVectorFont.GetPath(...)`/`GetNormalizedPath(...)` to
draw the exact glyph outline for that PDF code.

Decision: do not run HarfBuzz over this path by default. HarfBuzz shapes
Unicode text into glyph clusters; this path already has PDF glyph codes and
positions. Reshaping here could reorder or substitute glyphs that the PDF
author already selected.

### `DrawMappedTrueTypeGlyph(...)`

This path is used for non-embedded TrueType dictionary fonts when a substitute
system font can be mapped and parsed by FontBox. It still draws by glyph code
through `GlyphCache`, then stretches to the explicit PDF width when needed.

Decision: keep this as a glyph-code outline path. HarfBuzz is not needed unless
the renderer is redesigned to batch glyph runs before `ShowGlyph`, which would
be a larger PDF stream-engine change and still must preserve existing PDF glyph
codes.

### `DrawUnicodeGlyphFallback(...)`

This is the only current rendering path that draws Unicode text with Skia
directly (`SKFont.MeasureText`, `SKFont.GetTextPath`, `SKCanvas.DrawText`).
It is reached only when a real PDF/vector glyph path cannot be obtained.

Decision: this is the main future rendering-side candidate for HarfBuzz, but it
currently receives one PDF code at a time. HarfBuzz benefits from run-level
context, so useful improvement would require buffering fallback Unicode runs,
not merely replacing a one-glyph `DrawText` call. Until that buffering exists,
the fallback path should remain conservative.

### Text Clipping And Rendering Modes

Text clipping is buffered from the same glyph paths used for fill/stroke
rendering. The current implementation correctly keeps clipping tied to the
actual path that was rendered or would have been rendered. Any future fallback
run shaper must generate matching fill, stroke, and clip paths from the same
shaped glyph run.

## Implemented In This Slice

`PdfBox.Net.SkiaSharp` now provides `SkiaGlyphLayoutProcessor`, an optional
HarfBuzz-backed implementation of the upstream glyph-layout interface:

- loads/registers Type 0 fonts through the SkiaSharp package,
- shapes Unicode text with HarfBuzzSharp,
- emits Identity-H glyph IDs into `Tj`/`TJ` content stream operators,
- applies HarfBuzz advances and offsets as PDF text-positioning adjustments,
- keeps SkiaSharp/HarfBuzzSharp types out of core `PdfBox.Net` APIs.

This addresses generated content streams and form appearances that opt into a
glyph layout processor. It does not reshape existing PDF page content during
rasterization.

## Bidi Decision

Full Java parity uses `java.text.Bidi`, which implements the Unicode
Bidirectional Algorithm. .NET does not provide an equivalent BCL API.

NuGet candidates reviewed:

- `Bidi` 0.9.0: exposes `System.Text.Bidi`, but its NuGet metadata has no
  license expression or license URL. Do not add it.
- `BidiSharp` 0.2.1: MIT licensed, but targets Unicode 6.3 and exposes only a
  low-level `LogicalToVisual(string, int[])` API that requires caller-provided
  embedding levels. Do not use it as a complete Java `Bidi` replacement.
- `Harara.Bidi` 1.0.3: MIT licensed and managed, but its public model returns
  visual text and performs Arabic/Persian presentation-form shaping. That is
  useful for naive text renderers, but it is the wrong input for HarfBuzz,
  which should receive logical Unicode plus a run direction.
- `FriBidiSharp`: wraps native FriBidi, which would add another native runtime
  dependency family. This may be valid later, but should be evaluated as a
  separate backend dependency decision.

Current implementation status: `SkiaGlyphLayoutProcessor` now uses an internal
`BidiTextRunResolver` for Java-like visual run ordering before HarfBuzz
shaping. The resolver covers the Java `Bidi` cases seen in Apache's
`GlyphLayoutBidiTest` style samples: Arabic/Hebrew RTL runs, mixed LTR/RTL
runs, European numbers inside RTL text as even-level runs, and neutral
punctuation such as parentheses resolving back to the base direction when
surrounded by opposite strong directions.

This is closer to Java than the previous strong-character splitter, but it is
still not a complete modern UAX #9 implementation. Full Bidi parity remains
open under issue #618 and should be solved either by a well-maintained licensed
dependency that exposes run levels without reshaping text, or by a focused
port/adaptation with explicit Unicode-version coverage.

## Test Coverage Added

New tests cover:

- Type 0 font registration through the Skia glyph layout processor.
- Latin glyph mapping and kerning with `LiberationSans-Regular.ttf`.
- Complex-script substitution with `Lohit-Bengali.ttf`, where the Bengali
  cluster `U+0995 U+09CD U+09B0` shapes to one glyph.
- Content stream output using shaped glyph IDs rather than raw Unicode text.
- Java `java.text.Bidi` representative visual run outputs for Arabic numbers,
  mixed Arabic/LTR text, Hebrew plus European digits, and neutral parentheses.

## Remaining Work For #618

- Add full UAX #9 Bidi support if a suitable dependency or focused internal
  port is chosen.
- Decide whether fallback Unicode rendering should buffer whole text runs so it
  can shape fallback text with HarfBuzz before drawing/filling/stroking/clipping.
- Promote the local Java PDFBox/AWT comparison probe into CI fixtures if the
  required extra fonts are accepted as test assets.
- If fallback run shaping is implemented, verify fill, stroke, fill+stroke,
  invisible, and clipping rendering modes use identical shaped paths.
