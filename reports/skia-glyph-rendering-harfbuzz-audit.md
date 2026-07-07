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

Decision: use the optional `Unicode.Bidi` NuGet package in
`PdfBox.Net.SkiaSharp`. The package is a .NET port of Rust `unicode-bidi`
0.3.18, targets .NET 10, has no transitive dependencies, tracks Unicode data
16.0.0, and exposes UTF-16 code-unit ranges and resolved levels. That API shape
fits `SkiaGlyphLayoutProcessor`: it can split visual runs while keeping
HarfBuzz input as logical Unicode text plus a direction.

Current implementation status: `SkiaGlyphLayoutProcessor` now uses an internal
`BidiTextRunResolver` adapter over `Unicode.Bidi` before HarfBuzz shaping.
The resolver covers the Java `Bidi` cases seen in Apache's
`GlyphLayoutBidiTest` style samples: Arabic/Hebrew RTL runs, mixed LTR/RTL
runs, European numbers inside RTL text as even-level runs, neutral punctuation
such as parentheses resolving back to the base direction when surrounded by
opposite strong directions, and isolate controls in representative mixed text.

A follow-up Java comparison pass tightened RTL paragraph behavior: when the
paragraph base level is RTL, embedded Latin runs and European digits now use
Java-like even level `2` instead of being treated as root-level LTR text. The
tests pin Java-observed visual run order for leading-number RTL paragraphs,
RTL paragraphs with trailing Latin text, hyphenated RTL-plus-number text, and
Arabic text containing embedded Latin plus European digits.

This replaces the previous strong-character splitter with a conformance-focused
UAX #9 implementation while keeping the dependency isolated to the SkiaSharp
backend. Java `Bidi` and Rust `unicode-bidi` can associate explicit formatting
controls with adjacent runs differently; those controls are non-rendering, but
visible Java comparison tests remain focused on glyph-bearing text and isolate
controls.

## Test Coverage Added

New tests cover:

- Type 0 font registration through the Skia glyph layout processor.
- Latin glyph mapping and kerning with `LiberationSans-Regular.ttf`.
- Complex-script substitution with `Lohit-Bengali.ttf`, where the Bengali
  cluster `U+0995 U+09CD U+09B0` shapes to one glyph.
- Content stream output using shaped glyph IDs rather than raw Unicode text.
- Java `java.text.Bidi` representative visual run outputs for Arabic numbers,
  mixed Arabic/LTR text, Hebrew plus European digits, and neutral parentheses.
- Java `java.text.Bidi` RTL paragraph outputs where embedded Latin and
  European numbers are level `2` runs.
- Java `java.text.Bidi` isolate-control output for mixed Hebrew/European-digit
  text.

## Remaining Work For #618

- Monitor `Unicode.Bidi` preview package upgrades and conformance ratchets.
- Decide whether fallback Unicode rendering should buffer whole text runs so it
  can shape fallback text with HarfBuzz before drawing/filling/stroking/clipping.
- Promote the local Java PDFBox/AWT comparison probe into CI fixtures if the
  required extra fonts are accepted as test assets.
- If fallback run shaping is implemented, verify fill, stroke, fill+stroke,
  invisible, and clipping rendering modes use identical shaped paths.
