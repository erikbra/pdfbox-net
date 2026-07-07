import java.awt.font.GlyphVector;
import java.awt.geom.Point2D;
import java.io.FileInputStream;
import java.util.Locale;

import org.apache.pdfbox.glyphlayout.awt.GlyphLayoutFontLoaderAwt;
import org.apache.pdfbox.glyphlayout.awt.GlyphLayoutProcessorAwt;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.font.PDType0Font;

public final class JavaGlyphLayoutProbe
{
    private JavaGlyphLayoutProbe()
    {
    }

    public static void main(String[] args) throws Exception
    {
        if (args.length < 4)
        {
            System.err.println("usage: JavaGlyphLayoutProbe <font-file> <font-size> <bidi-level> <text> [--kerning] [--ligatures]");
            System.exit(2);
        }

        String fontFile = args[0];
        float fontSize = Float.parseFloat(args[1]);
        int bidiLevel = Integer.parseInt(args[2]);
        String text = args[3];
        GlyphLayoutFontLoaderAwt.FontOptions options = new GlyphLayoutFontLoaderAwt.FontOptions();
        for (int i = 4; i < args.length; i++)
        {
            if ("--kerning".equals(args[i]))
            {
                options.setKerningOn();
            }
            else if ("--ligatures".equals(args[i]))
            {
                options.setLigaturesOn();
            }
            else
            {
                throw new IllegalArgumentException("unknown option: " + args[i]);
            }
        }

        ExposedGlyphLayoutProcessor processor = new ExposedGlyphLayoutProcessor();
        try (PDDocument document = new PDDocument();
             FileInputStream input = new FileInputStream(fontFile))
        {
            PDType0Font font = processor.loadFont(document, input, options);
            GlyphVector vector = processor.compute(font, fontSize, text, bidiLevel);
            printGlyphVector(vector);
        }
    }

    private static void printGlyphVector(GlyphVector vector)
    {
        for (int i = 0; i < vector.getNumGlyphs(); i++)
        {
            Point2D position = vector.getGlyphPosition(i);
            System.out.println("{\"index\":" + i +
                ",\"glyph\":" + vector.getGlyphCode(i) +
                ",\"x\":" + format(position.getX()) +
                ",\"y\":" + format(position.getY()) +
                ",\"advanceX\":" + format(vector.getGlyphMetrics(i).getAdvanceX()) +
                "}");
        }

        Point2D end = vector.getGlyphPosition(vector.getNumGlyphs());
        System.out.println("{\"endX\":" + format(end.getX()) + ",\"endY\":" + format(end.getY()) + "}");
    }

    private static String format(double value)
    {
        return String.format(Locale.ROOT, "%.6f", value);
    }

    private static final class ExposedGlyphLayoutProcessor extends GlyphLayoutProcessorAwt
    {
        GlyphVector compute(PDType0Font font, float fontSize, String text, int bidiLevel)
        {
            return computeGlyphVector(font, fontSize, text, bidiLevel);
        }
    }
}
