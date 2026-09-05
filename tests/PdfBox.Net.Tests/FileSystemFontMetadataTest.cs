using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

public class FileSystemFontMetadataTest
{
    [Fact]
    public void InventoryIsLazyReadsPostScriptNamesAndCachesParsedFonts()
    {
        string directory = Directory.CreateTempSubdirectory("pdfbox-font-metadata-").FullName;
        try
        {
            FileSystemFontProvider provider = new([directory]);
            string file = Path.Combine(directory, "different-filename.ttf");
            File.WriteAllBytes(file, FontBoxTestFixtures.CreateMinimalTrueType());

            FontInfo info = Assert.Single(provider.GetFontInfo());

            Assert.Equal("MiniTTF", info.GetPostScriptName());
            Assert.Equal(FontFormat.TTF, info.GetFormat());
            Assert.Equal(file, provider.FindFontFile("MiniTTF"));
            Assert.Equal(file, provider.FindFontFile("different-filename"));
            Assert.Same(provider.GetFontInfo(), provider.GetFontInfo());
            TrueTypeFont first = Assert.IsType<TrueTypeFont>(info.GetFont());
            Assert.Same(first, info.GetFont());
            int glyphId = first.GetUnicodeCmapLookup()!.GetGlyphId('A');
            Assert.Equal(1, glyphId);
            Assert.NotEmpty(first.GetGlyph()!.GetGlyph(glyphId)!.GetPath().Segments);

            CIDFontMapping mapping = Assert.IsType<CIDFontMapping>(new FontMapperImpl(provider).GetCIDFont("ABCDEF+MiniTTF", null, null));
            Assert.False(mapping.IsCIDFont());
            Assert.False(mapping.IsFallback());
            Assert.Same(first, mapping.GetTrueTypeFont());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void OpenTypeInventoryRetainsRosAndSuppliesCffFontToMapper()
    {
        string directory = Directory.CreateTempSubdirectory("pdfbox-font-cff-").FullName;
        try
        {
            File.WriteAllBytes(Path.Combine(directory, "font.otf"), FontBoxTestFixtures.CreateMinimalOpenTypeCff(cidKeyed: true));
            FileSystemFontProvider provider = new([directory]);

            FontInfo info = Assert.Single(provider.GetFontInfo());

            Assert.Equal(FontFormat.OTF, info.GetFormat());
            Assert.NotNull(info.GetCIDSystemInfo());
            CIDFontMapping mapping = Assert.IsType<CIDFontMapping>(new FontMapperImpl(provider).GetCIDFont("MiniCFF", null, null));
            Assert.True(mapping.IsCIDFont());
            Assert.False(mapping.IsFallback());
            Assert.IsType<OpenTypeFont>(mapping.GetFont());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Standard14AliasUsesInstalledSubstituteBeforeLastResort()
    {
        string directory = Directory.CreateTempSubdirectory("pdfbox-font-substitute-").FullName;
        try
        {
            using Stream input = typeof(FontMapperImpl).Assembly.GetManifestResourceStream(
                "PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
            using (FileStream output = File.Create(Path.Combine(directory, "font.ttf")))
            {
                input.CopyTo(output);
            }
            FileSystemFontProvider provider = new([directory]);
            FontMapperImpl mapper = new(provider);

            CIDFontMapping mapping = Assert.IsType<CIDFontMapping>(mapper.GetCIDFont("ArialMT", null, null));

            Assert.False(mapping.IsFallback());
            Assert.Equal("LiberationSans", Assert.IsType<TrueTypeFont>(mapping.GetTrueTypeFont()).GetName());
            Assert.Same(Assert.Single(provider.GetFontInfo()).GetFont(), mapping.GetTrueTypeFont());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void EmptyInventoryUsesBundledLastResortFont()
    {
        FontMapperImpl mapper = new(new FileSystemFontProvider([]));

        CIDFontMapping mapping = Assert.IsType<CIDFontMapping>(mapper.GetCIDFont("MissingFont", null, null));

        Assert.True(mapping.IsFallback());
        TrueTypeFont font = Assert.IsType<TrueTypeFont>(mapping.GetTrueTypeFont());
        Assert.Equal("LiberationSans", font.GetName());
        Assert.True(font.GetUnicodeCmapLookup()!.GetGlyphId('A') > 0);
    }
}
