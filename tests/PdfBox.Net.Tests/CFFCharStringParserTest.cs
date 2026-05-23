using PdfBox.Net.FontBox.CFF;

namespace PdfBox.Net.Tests;

public class CFFCharStringParserTest
{
    [Fact]
    public void Type1ParserExpandsSubrCall()
    {
        Type1CharStringParser parser = new("MiniType1");
        List<byte[]> subrs = [[139, 11]];
        List<object> sequence = parser.Parse([139, 10, 14], subrs, "A");

        Assert.Equal(2, sequence.Count);
        Assert.Equal(0, Assert.IsType<int>(sequence[0]));
        Assert.Equal(CharStringCommand.Type1KeyWord.ENDCHAR, Assert.IsType<CharStringCommand>(sequence[1]).Type1);
    }

    [Fact]
    public void Type2ParserExpandsGlobalSubrCall()
    {
        Type2CharStringParser parser = new("MiniType2");
        byte[][] globalSubrs = [[139, 11]];
        List<object> sequence = parser.Parse([32, 29, 14], globalSubrs, []);

        Assert.Equal(2, sequence.Count);
        Assert.Equal(0, Assert.IsType<int>(sequence[0]));
        Assert.Equal(CharStringCommand.Type2KeyWord.ENDCHAR, Assert.IsType<CharStringCommand>(sequence[1]).Type2);
    }

    [Fact]
    public void ExpertCharsetAndEncodingArePopulated()
    {
        Assert.Equal("space", CFFISOAdobeCharset.INSTANCE.GetNameForGID(1));
        Assert.Equal("space", CFFExpertCharset.INSTANCE.GetNameForGID(1));
        Assert.Equal("space", CFFExpertSubsetCharset.INSTANCE.GetNameForGID(1));
        Assert.Equal("space", CFFExpertEncoding.INSTANCE.GetName(32));
        Assert.Equal("dollaroldstyle", CFFExpertEncoding.INSTANCE.GetName(36));
    }

    [Fact]
    public void EmbeddedCharsetCidMapsBidirectionally()
    {
        EmbeddedCharset charset = new(isCidFont: true);
        charset.AddCID(0, 0);
        charset.AddCID(1, 88);

        Assert.True(charset.IsCIDFont());
        Assert.Equal(1, charset.GetGIDForCID(88));
        Assert.Equal(88, charset.GetCIDForGID(1));
    }
}
