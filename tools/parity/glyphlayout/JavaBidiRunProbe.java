/*
 * Local Java Bidi oracle for PdfBox.Net issue #618.
 *
 * Compile with:
 *   javac -d /tmp/pdfbox-net-glyphlayout tools/parity/glyphlayout/JavaBidiRunProbe.java
 *
 * Run with:
 *   java -cp /tmp/pdfbox-net-glyphlayout JavaBidiRunProbe "abc אבג 123 def"
 */

import java.text.Bidi;

public final class JavaBidiRunProbe
{
    private JavaBidiRunProbe()
    {
    }

    public static void main(String[] args)
    {
        if (args.length != 1)
        {
            System.err.println("Usage: JavaBidiRunProbe <text>");
            System.exit(2);
        }

        String text = args[0];
        Bidi bidi = new Bidi(text, Bidi.DIRECTION_DEFAULT_LEFT_TO_RIGHT);
        int runCount = bidi.getRunCount();
        byte[] levels = new byte[runCount];
        Integer[] runs = new Integer[runCount];

        for (int i = 0; i < runCount; i++)
        {
            levels[i] = (byte) bidi.getRunLevel(i);
            runs[i] = i;
        }

        Bidi.reorderVisually(levels, 0, runs, 0, runCount);
        System.out.printf("{\"baseLevel\":%d,\"mixed\":%s,\"runCount\":%d}%n",
                bidi.getBaseLevel(),
                bidi.isMixed() ? "true" : "false",
                runCount);

        for (int visualIndex = 0; visualIndex < runCount; visualIndex++)
        {
            int runIndex = runs[visualIndex];
            int start = bidi.getRunStart(runIndex);
            int limit = bidi.getRunLimit(runIndex);
            System.out.printf(
                    "{\"visualIndex\":%d,\"logicalRun\":%d,\"level\":%d,\"start\":%d,\"limit\":%d,\"text\":\"%s\"}%n",
                    visualIndex,
                    runIndex,
                    bidi.getRunLevel(runIndex),
                    start,
                    limit,
                    escapeJson(text.substring(start, limit)));
        }
    }

    private static String escapeJson(String text)
    {
        StringBuilder escaped = new StringBuilder(text.length());
        for (int i = 0; i < text.length(); i++)
        {
            char c = text.charAt(i);
            switch (c)
            {
                case '\\':
                    escaped.append("\\\\");
                    break;
                case '"':
                    escaped.append("\\\"");
                    break;
                case '\b':
                    escaped.append("\\b");
                    break;
                case '\f':
                    escaped.append("\\f");
                    break;
                case '\n':
                    escaped.append("\\n");
                    break;
                case '\r':
                    escaped.append("\\r");
                    break;
                case '\t':
                    escaped.append("\\t");
                    break;
                default:
                    if (c < 0x20)
                    {
                        escaped.append(String.format("\\u%04x", (int) c));
                    }
                    else
                    {
                        escaped.append(c);
                    }
                    break;
            }
        }

        return escaped.toString();
    }
}
