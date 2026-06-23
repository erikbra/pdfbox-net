using System.Security.Cryptography;
using System.Text;
using PdfBox.Net;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Tools.ImageIO;
using SkiaSharp;

internal static class DotnetPdfProbe
{
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
                    Emit(name, "render", true, pages, $"{image.Width}x{image.Height}:{FileSignature(png)}:{ImageMetrics(image)}", Elapsed(started));
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
        SKBitmap bitmap = image.Bitmap;
        int total = bitmap.Width * bitmap.Height;
        SKColor background = bitmap.GetPixel(0, 0);
        Dictionary<uint, int> histogram = [];
        int nonBackground = 0;
        int transparent = 0;
        int dominant = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                if (color.Alpha < 8)
                {
                    transparent++;
                }

                if (ColorDistance(color, background) > 8)
                {
                    nonBackground++;
                }

                uint argb = ((uint)color.Alpha << 24) | ((uint)color.Red << 16) | ((uint)color.Green << 8) | color.Blue;
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

    private static int ColorDistance(SKColor a, SKColor b)
    {
        return Math.Abs(a.Alpha - b.Alpha) + Math.Abs(a.Red - b.Red) + Math.Abs(a.Green - b.Green) + Math.Abs(a.Blue - b.Blue);
    }

    private static string Message(Exception ex)
    {
        Exception root = ex;
        while (root.InnerException is not null)
        {
            root = root.InnerException;
        }

        return $"{root.GetType().Name}:{root.Message}".Replace('\n', ' ').Replace('\r', ' ');
    }

    private static void Emit(string file, string op, bool ok, int pages, string detail, long ms)
    {
        Console.WriteLine("{\"file\":\"" + Escape(file) + "\",\"op\":\"" + op + "\",\"ok\":" + ok.ToString().ToLowerInvariant()
            + ",\"pages\":" + pages + ",\"ms\":" + ms + ",\"detail\":\"" + Escape(detail) + "\"}");
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
