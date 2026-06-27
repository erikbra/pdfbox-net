using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using PdfBox.Net;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Tools.ImageIO;
using PdfBox.Net.Util;
using GlyphList = PdfBox.Net.PDModel.Font.Encoding.GlyphList;

internal static class DotnetPdfProbe
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private static readonly HashSet<string> GlyphProbeFiles =
    [
        "AlignmentTests.pdf",
        "ControlCharacters.pdf",
        "PDFBOX-3038-001033-p2.pdf",
        "PDFBOX-3044-010197-p5-ligatures.pdf",
        "PDFBOX-3062-002207-p1.pdf",
        "PDFBOX-3656-SF1199AEG (Complete).pdf",
        "PDFBOX-4417-054080.pdf",
        "PDFBOX-5784.pdf",
        "PDFBOX-5811-362972.pdf",
        "arxiv-sample.pdf",
    ];

    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: DotnetPdfProbe <out-dir> <pdf> [<pdf>...] | --merge <out-dir> <pdf-a> <pdf-b>");
            return 2;
        }

        if (args[0] == "--merge")
        {
            if (args.Length != 4)
            {
                Console.Error.WriteLine("usage: DotnetPdfProbe --merge <out-dir> <pdf-a> <pdf-b>");
                return 2;
            }

            Merge(args[1], args[2], args[3]);
            return 0;
        }

        Directory.CreateDirectory(args[0]);
        for (int i = 1; i < args.Length; i++)
        {
            Probe(args[0], args[i]);
        }

        return 0;
    }

    private static void Probe(string outDir, string input)
    {
        string name = Path.GetFileName(input);
        int pages = -1;
        long started = Environment.TickCount64;
        try
        {
            using PDDocument document = Loader.LoadPDF(input);
            pages = document.GetNumberOfPages();
            Emit(name, "load", true, pages, "", Elapsed(started));

            started = Environment.TickCount64;
            try
            {
                string text = new PDFTextStripper().GetText(document);
                File.WriteAllText(Path.Combine(outDir, StripExt(name) + "-dotnet-text.txt"), text, Utf8NoBom);
                Emit(name, "text", true, pages, Hash(text), Elapsed(started));
            }
            catch (Exception ex)
            {
                Emit(name, "text", false, pages, Message(ex), Elapsed(started));
            }

            started = Environment.TickCount64;
            try
            {
                string saved = Path.Combine(outDir, StripExt(name) + "-dotnet-copy.pdf");
                document.Save(saved);
                Emit(name, "save", true, pages, FileSignature(saved), Elapsed(started));
            }
            catch (Exception ex)
            {
                Emit(name, "save", false, pages, Message(ex), Elapsed(started));
            }

            started = Environment.TickCount64;
            try
            {
                if (pages > 0)
                {
                    using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 36);
                    string png = Path.Combine(outDir, StripExt(name) + "-dotnet-p1.png");
                    ImageIOUtil.WriteImage(image, png, 36);
                    Emit(name, "render", true, pages, $"{image.Width}x{image.Height}:{ImagePixelHash(image)}:{ImageMetrics(image)}", Elapsed(started));
                }
                else
                {
                    Emit(name, "render", true, pages, "no-pages", Elapsed(started));
                }
            }
            catch (Exception ex)
            {
                Emit(name, "render", false, pages, Message(ex), Elapsed(started));
            }

            WriteGlyphProbe(outDir, name, document);
        }
        catch (Exception ex)
        {
            Emit(name, "load", false, pages, Message(ex), Elapsed(started));
        }
    }

    private static void Merge(string outDir, string a, string b)
    {
        Directory.CreateDirectory(outDir);
        string key = Path.GetFileName(a) + "+" + Path.GetFileName(b);
        long started = Environment.TickCount64;
        try
        {
            string dest = Path.Combine(outDir, $"{StripExt(Path.GetFileName(a))}__{StripExt(Path.GetFileName(b))}-dotnet-merged.pdf");
            PDFMergerUtility merger = new() { DestinationFileName = dest };
            merger.AddSource(a);
            merger.AddSource(b);
            merger.MergeDocuments();
            Emit(key, "merge", true, -1, FileSignature(dest), Elapsed(started));
        }
        catch (Exception ex)
        {
            Emit(key, "merge", false, -1, Message(ex), Elapsed(started));
        }
    }

    private static string StripExt(string name) => Path.GetFileNameWithoutExtension(name);

    private static long Elapsed(long started) => Environment.TickCount64 - started;

    private static string Hash(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] digest = SHA256.HashData(bytes);
        return $"{bytes.Length}:{Convert.ToHexString(digest)[..16].ToLowerInvariant()}";
    }

    private static string FileSignature(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] digest = SHA256.HashData(stream);
        return $"{new FileInfo(path).Length}:{Convert.ToHexString(digest)[..16].ToLowerInvariant()}";
    }

    private static string ImageMetrics(BufferedImage image)
    {
        int total = image.Width * image.Height;
        int background = image.GetRgb(0, 0);
        Dictionary<uint, int> histogram = [];
        int nonBackground = 0;
        int transparent = 0;
        int dominant = 0;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int color = image.GetRgb(x, y);
                if (Alpha(color) < 8)
                {
                    transparent++;
                }

                if (ColorDistance(color, background) > 8)
                {
                    nonBackground++;
                }

                uint argb = unchecked((uint)color);
                histogram.TryGetValue(argb, out int count);
                count++;
                histogram[argb] = count;
                if (count > dominant)
                {
                    dominant = count;
                }
            }
        }

        bool nearBlank = transparent == total || nonBackground <= Math.Max(10, total / 1000) || dominant >= (int)Math.Ceiling(total * 0.995);
        return $"nonBg={nonBackground}:unique={histogram.Count}:dominant={dominant}:transparent={transparent}:nearBlank={nearBlank.ToString().ToLowerInvariant()}";
    }

    private static string ImagePixelHash(BufferedImage image)
    {
        using SHA256 digest = SHA256.Create();
        byte[] argb = new byte[4];
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int color = image.GetRgb(x, y);
                argb[0] = (byte)Alpha(color);
                argb[1] = (byte)Red(color);
                argb[2] = (byte)Green(color);
                argb[3] = (byte)Blue(color);
                digest.TransformBlock(argb, 0, argb.Length, null, 0);
            }
        }

        digest.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(digest.Hash!)[..16].ToLowerInvariant();
    }

    private static int ColorDistance(int a, int b)
    {
        return Math.Abs(Alpha(a) - Alpha(b)) + Math.Abs(Red(a) - Red(b)) + Math.Abs(Green(a) - Green(b)) + Math.Abs(Blue(a) - Blue(b));
    }

    private static int Alpha(int argb) => (argb >> 24) & 0xFF;

    private static int Red(int argb) => (argb >> 16) & 0xFF;

    private static int Green(int argb) => (argb >> 8) & 0xFF;

    private static int Blue(int argb) => argb & 0xFF;

    private static void WriteGlyphProbe(string outDir, string name, PDDocument document)
    {
        if (!GlyphProbeFiles.Contains(name))
        {
            return;
        }

        try
        {
            GlyphProbeRenderer renderer = new(document);
            if (document.GetNumberOfPages() > 0)
            {
                renderer.RenderImageWithDPI(0, 36);
            }

            File.WriteAllText(Path.Combine(outDir, StripExt(name) + "-dotnet-glyphs.jsonl"), renderer.ToJsonLines(), Utf8NoBom);
        }
        catch
        {
            // Glyph probes are diagnostic artifacts; text extraction remains authoritative.
        }
    }

    private sealed class GlyphProbeRenderer(PDDocument document) : PDFRenderer(document)
    {
        private readonly GlyphRecorder _recorder = new();

        protected override PageDrawer CreatePageDrawer(PageDrawerParameters parameters)
        {
            return new GlyphProbePageDrawer(parameters, _recorder);
        }

        public string ToJsonLines() => _recorder.ToJsonLines();
    }

    private sealed class GlyphProbePageDrawer(PageDrawerParameters parameters, GlyphRecorder recorder) : PageDrawer(parameters)
    {
        protected override void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
        {
            recorder.Record(1, textRenderingMatrix, font, code, displacement);
            base.ShowGlyph(textRenderingMatrix, font, code, displacement);
        }
    }

    private sealed class GlyphRecorder
    {
        private readonly StringBuilder _lines = new();
        private int _index;

        public void Record(int page, Matrix textRenderingMatrix, PDFont? font, int code, Vector displacement)
        {
            string unicode = ToUnicode(font, code);
            string fontName = font?.GetName() ?? string.Empty;
            string fontType = font?.GetType().Name ?? string.Empty;
            bool embedded = font?.IsEmbedded() ?? false;
            bool standard14 = font?.IsStandard14() ?? false;
            float advance = displacement.GetX() * textRenderingMatrix.GetScalingFactorX();

            _lines.Append("{\"page\":").Append(page)
                .Append(",\"index\":").Append(_index++)
                .Append(",\"unicode\":\"").Append(EscapeJson(unicode)).Append('"')
                .Append(",\"codes\":\"").Append(code).Append('"')
                .Append(",\"x\":").Append(F(textRenderingMatrix.GetTranslateX()))
                .Append(",\"y\":").Append(F(textRenderingMatrix.GetTranslateY()))
                .Append(",\"w\":").Append(F(advance))
                .Append(",\"h\":").Append(F(textRenderingMatrix.GetScalingFactorY()))
                .Append(",\"space\":").Append(F(0))
                .Append(",\"fontSize\":").Append(F(textRenderingMatrix.GetScalingFactorY()))
                .Append(",\"fontSizePt\":").Append(F(textRenderingMatrix.GetScalingFactorY()))
                .Append(",\"xScale\":").Append(F(textRenderingMatrix.GetScalingFactorX()))
                .Append(",\"yScale\":").Append(F(textRenderingMatrix.GetScalingFactorY()))
                .Append(",\"font\":\"").Append(EscapeJson(fontName)).Append('"')
                .Append(",\"fontType\":\"").Append(EscapeJson(fontType)).Append('"')
                .Append(",\"embedded\":").Append(embedded.ToString().ToLowerInvariant())
                .Append(",\"standard14\":").Append(standard14.ToString().ToLowerInvariant())
                .AppendLine("}");
        }

        public string ToJsonLines() => _lines.ToString();

        private static string ToUnicode(PDFont? font, int code)
        {
            if (font is null)
            {
                return string.Empty;
            }

            try
            {
                return font.ToUnicode(code, GlyphList.GetAdobeGlyphList()) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    private static string F(float value) => value.ToString("F3", CultureInfo.InvariantCulture);

    private static string Message(Exception ex)
    {
        Exception root = ex;
        while (root.InnerException is not null)
        {
            root = root.InnerException;
        }

        string typeName = root is InvalidOperationException ? "IllegalStateException" : root.GetType().Name;
        return $"{typeName}:{root.Message}".Replace('\n', ' ').Replace('\r', ' ');
    }

    private static void Emit(string file, string op, bool ok, int pages, string detail, long ms)
    {
        Console.WriteLine("{\"file\":\"" + Escape(file) + "\",\"op\":\"" + op + "\",\"ok\":" + ok.ToString().ToLowerInvariant()
            + ",\"pages\":" + pages + ",\"ms\":" + ms + ",\"detail\":\"" + Escape(detail) + "\"}");
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapeJson(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char ch in value)
        {
            switch (ch)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(ch))
                    {
                        builder.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}
