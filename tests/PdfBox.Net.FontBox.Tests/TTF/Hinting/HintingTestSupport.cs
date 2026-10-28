using System.Diagnostics.CodeAnalysis;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

public abstract class HintingTestSupport
{
    protected static TrueTypeFont Parse(string resource) => new TTFParser().Parse(
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "TTF", Path.GetFileName(resource))));

    protected static string Fixture(string file) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "TTF", file);

    protected static double[] Flatten(GeneralPath? path)
    {
        Assert.NotNull(path);
        return path.Segments.SelectMany(s => new[] { (double)s.Type, s.X1, s.Y1, s.X2, s.Y2, s.X3, s.Y3 }).ToArray();
    }

    protected static void Equal<T>(T expected, T actual, string? message = null) => Assert.Equal(expected, actual);
    protected static void IsNull(object? value, string? message = null) => Assert.Null(value);
    protected static void NotNull([NotNull] object? value, string? message = null) => Assert.NotNull(value);
}
