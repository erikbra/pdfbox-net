using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;

namespace PdfBox.Net.Tests;

public class COSRegressionFixturesTest
{
    public static TheoryData<string, string> DeterministicRoundtripFixtures => new()
    {
        { "roundtrip-basic.cos", "roundtrip-basic.expected" },
        { "minimal-null.cos", "minimal-null.expected" },
        { "minimal-array.cos", "minimal-array.expected" },
        { "minimal-dictionary.cos", "minimal-dictionary.expected" }
    };

    [Theory]
    [MemberData(nameof(DeterministicRoundtripFixtures))]
    public void ParseSerializeRoundtrip_Fixture_MatchesExpected(string inputFixture, string expectedFixture)
    {
        string input = ReadFixtureText(inputFixture);
        string expected = ReadFixtureText(expectedFixture);

        COSBase parsed = COSParser.Parse(input);
        string serialized = COSWriter.SerializeToString(parsed);
        Assert.Equal(expected, serialized);

        COSBase reparsed = COSParser.Parse(serialized);
        string reserialized = COSWriter.SerializeToString(reparsed);
        Assert.Equal(serialized, reserialized);
    }

    public static TheoryData<string, Type> MalformedFixtures => new()
    {
        { "malformed-dictionary-key.cos", typeof(IOException) },
        { "malformed-unclosed-array.cos", typeof(EndOfStreamException) },
        { "malformed-name-escape.cos", typeof(IOException) },
        { "malformed-literal-string.cos", typeof(EndOfStreamException) }
    };

    [Theory]
    [MemberData(nameof(MalformedFixtures))]
    public void ParseObject_MalformedFixture_ThrowsExpectedException(string inputFixture, Type expectedException)
    {
        string input = ReadFixtureText(inputFixture);
        Exception exception = Assert.ThrowsAny<Exception>(() => COSParser.Parse(input));
        Assert.IsType(expectedException, exception);
    }

    private static string ReadFixtureText(string fixtureName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "COS", fixtureName);
        return NormalizeLineEndings(File.ReadAllText(path)).TrimEnd('\n');
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.ReplaceLineEndings("\n");
    }
}
