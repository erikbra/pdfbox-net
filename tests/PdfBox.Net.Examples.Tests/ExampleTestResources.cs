using System.Reflection;
using PdfBox.Net.Examples.PDModel;

namespace PdfBox.Net.Examples.Tests;

internal static class ExampleTestResources
{
    private const string LiberationSansResource =
        "PdfBox.Net.Examples.Resources.ttf.LiberationSans-Regular.ttf";

    public static string CreateTempDirectory(string name)
    {
        string path = Path.Combine(Path.GetTempPath(), $"pdfbox-net-{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    public static string WriteLiberationSansRegular(string directory)
    {
        Assembly assembly = typeof(CreatePDFA).Assembly;
        using Stream? input = assembly.GetManifestResourceStream(LiberationSansResource);
        Assert.NotNull(input);

        string path = Path.Combine(directory, "LiberationSans-Regular.ttf");
        using FileStream output = File.Create(path);
        input!.CopyTo(output);
        return path;
    }
}
