# Skia Glyph Layout Java Comparison - 2026-07-07

Issue: #618

## Scope

This comparison focused on generated Unicode text flowing through
`GlyphLayoutProcessorAwt` in Apache PDFBox trunk and
`SkiaGlyphLayoutProcessor` in `PdfBox.Net.SkiaSharp`.

Rendering existing PDF page content was not reshaped. Java `PageDrawer` renders
PDF glyph codes through `PDVectorFont` outlines, and the Skia backend should
continue to preserve already-decoded PDF glyph codes and positions.

## Java Reference Setup

Apache PDFBox was built from the local checkout:

```bash
PDFBOX_ROOT=/Users/erik/src/Repos/apache/pdfbox
JAVA_HOME=/opt/homebrew/opt/openjdk \
PATH=/opt/homebrew/opt/openjdk/bin:$PATH \
mvn -f "$PDFBOX_ROOT/pom.xml" -pl pdfbox-layout-awt -am -DskipTests package
```

`tools/parity/glyphlayout/JavaGlyphLayoutProbe.java` was then compiled against
the Apache `pdfbox-layout-awt`, `pdfbox`, `fontbox`, and `pdfbox-io` jars.

## Glyph Vector Results

### Latin Kerning

Java command:

```bash
JavaGlyphLayoutProbe \
  "$PDFBOX_ROOT/fontbox/src/test/resources/ttf/LiberationSans-Regular.ttf" \
  12 0 AV --kerning
```

Java AWT output:

```jsonl
{"index":0,"glyph":36,"x":0.000000,"y":0.000000,"advanceX":8.003906}
{"index":1,"glyph":57,"x":7.113281,"y":0.000000,"advanceX":8.003906}
{"endX":15.117188,"endY":0.000000}
```

`SkiaGlyphLayoutProcessorTest.ComputeGlyphs_UsesHarfBuzzGlyphMapping` already
asserts the same glyph ids, `[36, 57]`, and
`ShowText_EmitsShapedGlyphIdsAndPositioning` verifies kerning is represented as
PDF text-positioning adjustment rather than raw Unicode text.

### Bengali Cluster

Java command:

```bash
JavaGlyphLayoutProbe \
  "$PDFBOX_ROOT/fontbox/src/test/resources/ttf/Lohit-Bengali.ttf" \
  12 0 $'\u0995\u09CD\u09B0' --ligatures
```

Java AWT output:

```jsonl
{"index":0,"glyph":164,"x":0.000000,"y":0.000000,"advanceX":10.033844}
{"endX":10.033844,"endY":0.000000}
```

`SkiaGlyphLayoutProcessorTest.ComputeGlyphs_AppliesComplexScriptSubstitution`
asserts the same single glyph id, `164`.

### Arabic RTL Shape Sample

Java command:

```bash
JavaGlyphLayoutProbe \
  "$PDFBOX_ROOT/pdfbox-layout-awt/src/test/resources/ttf/NotoSansArabic-Regular.ttf" \
  12 1 $'\u0627\u0644\u0633\u0644\u0627\u0645 \u0639\u0644\u064A\u0643\u0645' --ligatures
```

Java AWT emits contextual Arabic glyph ids and positions for the RTL run. The
Noto Sans Arabic font is not currently a checked-in `PdfBox.Net` test fixture,
so this comparison remains a local probe result rather than a CI assertion.

## Java Bidi Run Results

Java `java.text.Bidi` was used as the run-order oracle. The following visual
run expectations are now pinned in
`SkiaGlyphLayoutProcessorTest.BidiTextRunResolver_MatchesJavaTextBidiForRepresentativeRuns`:

- Arabic with European year: visual order is trailing Arabic, `1447` at level
  2, then leading Arabic.
- Mixed LTR/Arabic/LTR: visual order remains LTR, RTL, LTR.
- Hebrew with European digits: visual order is leading LTR, digits at level 2,
  Hebrew at level 1, trailing LTR.
- Neutral parentheses around Hebrew in LTR text attach to the LTR runs, matching
  Java's base-direction neutral handling for this sample.

A later pass added `JavaBidiRunProbe.java` and compared additional RTL
paragraph cases. Those samples showed that Java assigns embedded Latin runs to
even level `2` when the paragraph base level is RTL. `BidiTextRunResolver` was
updated to match that behavior for:

- leading European numbers followed by Hebrew and Latin text,
- Hebrew plus European digits plus trailing Latin text,
- LTR text containing Hebrew plus hyphenated European digits,
- Arabic text with embedded Latin text and European digits.

## Code Changes

- Added `BidiTextRunResolver`, an internal SkiaSharp-package helper that
  resolves Java-like visual runs before HarfBuzz shaping.
- Replaced the previous strong-character-only splitter in
  `SkiaGlyphLayoutProcessor`.
- Added unit tests that encode the Java Bidi run outputs above.
- Added `JavaGlyphLayoutProbe.java`, `JavaBidiRunProbe.java`, and documentation
  for local Java/AWT glyph vector and Java `Bidi` visual-run comparisons.

## Remaining Differences

The new resolver is deliberately narrower than full UAX #9. It covers the
representative Java cases above, RTL paragraph embedding of Latin text and
European numbers, and common neutral punctuation, but it should not be
documented as full Bidi parity.

Fallback Unicode rendering in `SkiaPageDrawerPeer.DrawUnicodeGlyphFallback(...)`
still receives one decoded PDF code at a time. HarfBuzz-based fallback
rendering would need a whole-string or buffered-run rendering hook so fill,
stroke, and clipping all use the same shaped glyph path.

SkiaSharp 4 exposes `SKTextBlob` and low-level glyph APIs in this package, but
not a direct `SKShaper`-style API here. That reinforces keeping HarfBuzz
shaping in the generated-content processor until fallback rendering can supply
both whole text runs and font bytes/typefaces to HarfBuzz safely.
