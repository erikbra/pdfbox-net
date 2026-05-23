using System.Text;
using PdfBox.Net.FontBox.Type1;

namespace PdfBox.Net.Tests;

public class Type1LexerTest
{
    [Fact]
    public void TestRealNumbers()
    {
        string s = "/FontMatrix [1e-3 0e-3 0e-3 -1E-03 0 0 1.23 -1.23 ] readonly def";
        Type1Lexer lexer = new(Encoding.ASCII.GetBytes(s));
        List<Token> tokens = ReadTokens(lexer);
        Assert.Equal(Token.LITERAL, tokens[0].GetKind());
        Assert.Equal("FontMatrix", tokens[0].GetText());
        Assert.Equal(Token.START_ARRAY, tokens[1].GetKind());
        Assert.Equal(Token.REAL, tokens[2].GetKind());
        Assert.Equal(Token.REAL, tokens[3].GetKind());
        Assert.Equal(Token.REAL, tokens[4].GetKind());
        Assert.Equal(Token.REAL, tokens[5].GetKind());
        Assert.Equal(Token.INTEGER, tokens[6].GetKind());
        Assert.Equal(Token.INTEGER, tokens[7].GetKind());
        Assert.Equal(Token.REAL, tokens[8].GetKind());
        Assert.Equal(Token.REAL, tokens[9].GetKind());
        Assert.Equal(-1e-3f, tokens[5].FloatValue());
    }

    [Fact]
    public void TestEmptyName()
    {
        Type1Lexer lexer = new(Encoding.ASCII.GetBytes("dup 127 / put"));
        DamagedFontException ex = Assert.Throws<DamagedFontException>(() => ReadTokens(lexer));
        Assert.Equal("Could not read token at position 9", ex.Message);
    }

    [Fact]
    public void TestProcAndNameAndDictAndString()
    {
        string s = "/ND {noaccess def} executeonly def \n 8#173 +2#110 \n%comment \n<< (string \\n \\r \\t \\b \\f \\\\ \\( \\) \\123) >>";
        Type1Lexer lexer = new(Encoding.ASCII.GetBytes(s));
        List<Token> tokens = ReadTokens(lexer);
        Assert.Equal("ND", tokens[0].GetText());
        Assert.Equal(Token.START_PROC, tokens[1].GetKind());
        Assert.Equal("123", tokens[7].GetText());
        Assert.Equal("6", tokens[8].GetText());
        Assert.Equal(Token.START_DICT, tokens[9].GetKind());
        Assert.Equal("string \n \n \t \b \f \\ ( ) S", tokens[10].GetText());
        Assert.Equal(Token.END_DICT, tokens[11].GetKind());
    }

    [Fact]
    public void TestData()
    {
        Type1Lexer lexer = new(Encoding.ASCII.GetBytes("3 RD 123 ND"));
        List<Token> tokens = ReadTokens(lexer);
        Assert.Equal(3, tokens[0].IntValue());
        Assert.Equal(Token.CHARSTRING, tokens[1].GetKind());
        Assert.Equal(new byte[] { (byte)'1', (byte)'2', (byte)'3' }, tokens[1].GetData());
    }

    [Fact]
    public void TestOversizedData()
    {
        Type1Lexer lexer = new(Encoding.ASCII.GetBytes("999 RD"));
        IOException ex = Assert.Throws<IOException>(() => ReadTokens(lexer));
        Assert.Equal("String length 999 is larger than input", ex.Message);
    }

    private static List<Token> ReadTokens(Type1Lexer lexer)
    {
        List<Token> tokens = [];
        Token? token;
        while ((token = lexer.NextToken()) is not null)
        {
            tokens.Add(token);
        }

        return tokens;
    }
}
