# Glyph Layout Java Comparison Probe

`JavaGlyphLayoutProbe.java` is a small local oracle for issue #618. It exposes
Apache PDFBox's `GlyphLayoutProcessorAwt.computeGlyphVector(...)` result so the
SkiaSharp/HarfBuzz backend can be compared against Java AWT with the same
font, text, direction level, and kerning/ligature options.

`JavaBidiRunProbe.java` exposes the visual run order produced by
`java.text.Bidi`, which is the upstream `GlyphLayoutProcessorAwt` oracle before
each run is shaped.

Build Apache PDFBox first:

```bash
PDFBOX_ROOT=/path/to/apache/pdfbox
JAVA_HOME=/opt/homebrew/opt/openjdk \
PATH=/opt/homebrew/opt/openjdk/bin:$PATH \
mvn -f "$PDFBOX_ROOT/pom.xml" -pl pdfbox-layout-awt -am -DskipTests package
```

Compile and run the probe:

```bash
PDFBOX_ROOT=/path/to/apache/pdfbox
PROBE_CLASSES=/tmp/pdfbox-net-glyphlayout-probe
mkdir -p "$PROBE_CLASSES"
CP="$PDFBOX_ROOT/pdfbox-layout-awt/target/pdfbox-layout-awt-4.0.0-SNAPSHOT.jar"
CP="$CP:$PDFBOX_ROOT/pdfbox/target/pdfbox-4.0.0-SNAPSHOT.jar"
CP="$CP:$PDFBOX_ROOT/fontbox/target/fontbox-4.0.0-SNAPSHOT.jar"
CP="$CP:$PDFBOX_ROOT/io/target/pdfbox-io-4.0.0-SNAPSHOT.jar"
CP="$CP:$HOME/.m2/repository/org/apache/logging/log4j/log4j-api/2.26.1/log4j-api-2.26.1.jar"

javac -proc:none \
  -cp "$CP" \
  -d "$PROBE_CLASSES" \
  tools/parity/glyphlayout/JavaGlyphLayoutProbe.java \
  tools/parity/glyphlayout/JavaBidiRunProbe.java

java -Djava.awt.headless=true \
  -cp "$PROBE_CLASSES:$CP" \
  JavaGlyphLayoutProbe \
  "$PDFBOX_ROOT/fontbox/src/test/resources/ttf/LiberationSans-Regular.ttf" \
  12 0 AV --kerning

java -cp "$PROBE_CLASSES" \
  JavaBidiRunProbe \
  $'123 \u05D0\u05D1\u05D2 def'
```

The output is JSON Lines with glyph code, glyph position, and advance values.
Use the glyph and Bidi outputs as local references when changing
`PdfBox.Net.SkiaSharp/GlyphLayout/SkiaGlyphLayoutProcessor.cs` or
`PdfBox.Net.SkiaSharp/GlyphLayout/BidiTextRunResolver.cs`.
