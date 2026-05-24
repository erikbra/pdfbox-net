using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

public class CharStringParserTest
{
    // ── Type1CharStringParser ──────────────────────────────────────────────

    [Fact]
    public void Type1Parser_SimpleEndchar_ReturnsEndcharCommand()
    {
        // byte 14 → ENDCHAR command
        byte[] bytes = [14];
        var parser = new Type1CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, [], "space");
        Assert.Single(seq);
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[0]));
    }

    [Fact]
    public void Type1Parser_NumberEncoding_SmallPositive()
    {
        // byte 139 encodes 0, byte 140 encodes 1
        byte[] bytes = [140, 14]; // 1, endchar
        var parser = new Type1CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, [], "A");
        Assert.Equal(2, seq.Count);
        Assert.Equal(1, Assert.IsType<int>(seq[0]));
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[1]));
    }

    [Fact]
    public void Type1Parser_CallSubr_InlinesSubroutine()
    {
        // subr 0: [11] → RET
        byte[] subr0 = [11];
        // main: [139 (=0), 10 (CALLSUBR), 14 (ENDCHAR)]
        byte[] bytes = [139, 10, 14];
        var parser = new Type1CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, [subr0], "glyph");
        // The RET at the end of the subr should be stripped; ENDCHAR remains
        Assert.Single(seq);
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[0]));
    }

    [Fact]
    public void Type1Parser_CallSubr_OutOfRange_RemovesParam()
    {
        // subr index 99 does not exist (list is empty)
        byte[] bytes = [238, 10, 14]; // 238-139=99, CALLSUBR, ENDCHAR
        var parser = new Type1CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, [], "glyph");
        // The integer 99 should be removed; only ENDCHAR remains
        Assert.Single(seq);
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[0]));
    }

    // ── Type2CharStringParser ─────────────────────────────────────────────

    [Fact]
    public void Type2Parser_SimpleEndchar_ReturnsEndcharCommand()
    {
        byte[] bytes = [14]; // ENDCHAR
        var parser = new Type2CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, null, null);
        Assert.Single(seq);
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[0]));
    }

    [Fact]
    public void Type2Parser_ShortInt_DecodesCorrectly()
    {
        // b0=28 → read 2-byte signed short; 0x01 0x00 = 256
        byte[] bytes = [28, 1, 0, 14];
        var parser = new Type2CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, null, null);
        Assert.Equal(2, seq.Count);
        Assert.Equal(256d, Assert.IsType<double>(seq[0]));
    }

    [Fact]
    public void Type2Parser_Hmoveto_AppearsInSequence()
    {
        // byte 22 → HMOVETO
        byte[] bytes = [139, 22, 14]; // 0, HMOVETO, ENDCHAR
        var parser = new Type2CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, null, null);
        Assert.Equal(3, seq.Count);
        Assert.Equal(CharStringCommand.HMOVETO, Assert.IsType<CharStringCommand>(seq[1]));
    }

    [Fact]
    public void Type2Parser_CallGSubr_InlinesGlobalSubroutine()
    {
        // global subr 0 (bias=107 for length<1240, so operand=-107 encodes index 0)
        // subr: [11] → RET
        byte[] subr0 = [11]; // RET
        byte[][] globalSubrs = [subr0];
        // main: 32 (=32-139=-107) CALLGSUBR ENDCHAR
        // 32 = 32 → number: 32-139 = -107, so subrNumber = 107 + (-107) = 0 ✓
        byte[] bytes = [32, 29, 14]; // -107, CALLGSUBR, ENDCHAR
        var parser = new Type2CharStringParser("TestFont");
        List<object> seq = parser.Parse(bytes, globalSubrs, null);
        // RET is stripped, only ENDCHAR remains
        Assert.Single(seq);
        Assert.Equal(CharStringCommand.ENDCHAR, Assert.IsType<CharStringCommand>(seq[0]));
    }

    // ── DataInput implementations ─────────────────────────────────────────

    [Fact]
    public void DataInputByteArray_ReadUnsignedByte_ReturnsCorrectValue()
    {
        DataInputByteArray input = new([0xFF, 0x00]);
        Assert.Equal(0xFF, input.ReadUnsignedByte());
        Assert.Equal(0x00, input.ReadUnsignedByte());
        Assert.False(input.HasRemaining());
    }

    [Fact]
    public void DataInputByteArray_PeekUnsignedByte_DoesNotAdvance()
    {
        DataInputByteArray input = new([0xAB, 0xCD]);
        Assert.Equal(0xCD, input.PeekUnsignedByte(1));
        Assert.Equal(0, input.GetPosition()); // position unchanged
    }

    [Fact]
    public void DataInputByteArray_ReadBytes_ReturnsSlice()
    {
        DataInputByteArray input = new([1, 2, 3, 4]);
        byte[] slice = input.ReadBytes(2);
        Assert.Equal([1, 2], slice);
        Assert.Equal(2, input.GetPosition());
    }

    [Fact]
    public void DataInputByteArray_SetPosition_ThrowsOnOutOfRange()
    {
        DataInputByteArray input = new([1, 2]);
        Assert.Throws<IOException>(() => input.SetPosition(5));
    }

    [Fact]
    public void DataInputRandomAccessRead_ReadUnsignedByte_Works()
    {
        using RandomAccessReadBuffer buf = new([0x42, 0x00]);
        DataInputRandomAccessRead input = new(buf);
        Assert.Equal(0x42, input.ReadUnsignedByte());
    }

    // ── CFFOperator ────────────────────────────────────────────────────────

    [Fact]
    public void CFFOperator_SingleByte_ReturnsKnownName()
    {
        Assert.Equal("CharStrings", CFFOperator.GetOperator(17));
        Assert.Equal("Private", CFFOperator.GetOperator(18));
        Assert.Equal("charset", CFFOperator.GetOperator(15));
    }

    [Fact]
    public void CFFOperator_TwoByte_ReturnsKnownName()
    {
        Assert.Equal("FontMatrix", CFFOperator.GetOperator(12, 7));
        Assert.Equal("ROS", CFFOperator.GetOperator(12, 30));
        Assert.Equal("FDArray", CFFOperator.GetOperator(12, 36));
    }

    [Fact]
    public void CFFOperator_UnknownOpcode_ReturnsNull()
    {
        Assert.Null(CFFOperator.GetOperator(200));
    }

    // ── CharStringCommand ─────────────────────────────────────────────────

    [Fact]
    public void CharStringCommand_GetInstance_KnownValue()
    {
        Assert.Equal(CharStringCommand.ENDCHAR, CharStringCommandExtensions.GetInstance(14));
        Assert.Equal(CharStringCommand.HMOVETO, CharStringCommandExtensions.GetInstance(22));
        Assert.Equal(CharStringCommand.CALLSUBR, CharStringCommandExtensions.GetInstance(10));
    }

    [Fact]
    public void CharStringCommand_GetInstance_UnknownValue_ReturnsUnknown()
    {
        Assert.Equal(CharStringCommand.UNKNOWN, CharStringCommandExtensions.GetInstance(250));
    }

    [Fact]
    public void CharStringCommand_Type1KeyWord_MapsCorrectly()
    {
        Assert.Equal(Type1KeyWord.ENDCHAR, CharStringCommand.ENDCHAR.GetType1KeyWord());
        Assert.Equal(Type1KeyWord.HMOVETO, CharStringCommand.HMOVETO.GetType1KeyWord());
        Assert.Null(CharStringCommand.HINTMASK.GetType1KeyWord());
    }

    [Fact]
    public void CharStringCommand_Type2KeyWord_MapsCorrectly()
    {
        Assert.Equal(Type2KeyWord.ENDCHAR, CharStringCommand.ENDCHAR.GetType2KeyWord());
        Assert.Equal(Type2KeyWord.HMOVETO, CharStringCommand.HMOVETO.GetType2KeyWord());
        Assert.Null(CharStringCommand.CLOSEPATH.GetType2KeyWord());
    }

    // ── CFFCharsetCID ─────────────────────────────────────────────────────

    [Fact]
    public void CFFCharsetCID_IsCIDFont_ReturnsTrue()
    {
        Assert.True(new CFFCharsetCID().IsCIDFont());
    }

    [Fact]
    public void CFFCharsetCID_AddCid_RoundTrips()
    {
        CFFCharsetCID cs = new();
        cs.AddCID(0, 0);
        cs.AddCID(1, 42);
        Assert.Equal(1, cs.GetGIDForCID(42));
        Assert.Equal(42, cs.GetCIDForGID(1));
    }

    [Fact]
    public void CFFCharsetCID_AddSID_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new CFFCharsetCID().AddSID(0, 0, ".notdef"));
    }

    // ── FDSelect ──────────────────────────────────────────────────────────

    [Fact]
    public void FDSelect_LambdaImplementation_Works()
    {
        FDSelect fds = new SimpleFDSelect();
        Assert.Equal(0, fds.GetFDIndex(0));
        Assert.Equal(1, fds.GetFDIndex(100));
    }

    private sealed class SimpleFDSelect : FDSelect
    {
        public int GetFDIndex(int gid) => gid > 50 ? 1 : 0;
    }

    // ── CIDKeyedType2CharString ───────────────────────────────────────────

    [Fact]
    public void CIDKeyedType2CharString_CIDAndGlyphName_AreCorrect()
    {
        byte[] bytes = [14];
        CIDKeyedType2CharString cs = new("MyFont", 0x0041, bytes);
        Assert.Equal(0x0041, cs.CID);
        Assert.Equal("0041", cs.GlyphName);
        Assert.Equal("MyFont", cs.FontName);
    }

    // ── EmbeddedCharset ───────────────────────────────────────────────────

    [Fact]
    public void EmbeddedCharset_Type1_DelegatesCorrectly()
    {
        EmbeddedCharset ec = new(isCIDFont: false);
        Assert.False(ec.IsCIDFont());
        ec.AddSID(0, 0, ".notdef");
        ec.AddSID(1, 1, "space");
        Assert.Equal("space", ec.GetNameForGID(1));
        Assert.Equal(1, ec.GetSIDForGID(1));
    }

    [Fact]
    public void EmbeddedCharset_CID_DelegatesCorrectly()
    {
        EmbeddedCharset ec = new(isCIDFont: true);
        Assert.True(ec.IsCIDFont());
        ec.AddCID(0, 0);
        ec.AddCID(3, 200);
        Assert.Equal(3, ec.GetGIDForCID(200));
        Assert.Equal(200, ec.GetCIDForGID(3));
    }

    // ── Expert charsets/encoding ──────────────────────────────────────────

    [Fact]
    public void CFFISOAdobeCharset_GID0_IsNotdef()
    {
        Assert.Equal(".notdef", CFFISOAdobeCharset.INSTANCE.GetNameForGID(0));
    }

    [Fact]
    public void CFFISOAdobeCharset_GID1_IsSpace()
    {
        Assert.Equal("space", CFFISOAdobeCharset.INSTANCE.GetNameForGID(1));
    }

    [Fact]
    public void CFFISOAdobeCharset_LastEntry_IsZcaron()
    {
        // 229 entries → last GID is 228
        Assert.Equal("zcaron", CFFISOAdobeCharset.INSTANCE.GetNameForGID(228));
    }

    [Fact]
    public void CFFExpertCharset_GID0_IsNotdef()
    {
        Assert.Equal(".notdef", CFFExpertCharset.INSTANCE.GetNameForGID(0));
    }

    [Fact]
    public void CFFExpertCharset_GID1_IsSpace()
    {
        Assert.Equal("space", CFFExpertCharset.INSTANCE.GetNameForGID(1));
    }

    [Fact]
    public void CFFExpertSubsetCharset_GID0_IsNotdef()
    {
        Assert.Equal(".notdef", CFFExpertSubsetCharset.INSTANCE.GetNameForGID(0));
    }

    [Fact]
    public void CFFExpertEncoding_Code32_IsSpace()
    {
        // SID 1 → "space"
        Assert.Equal("space", CFFExpertEncoding.INSTANCE.GetName(32));
    }

    [Fact]
    public void CFFExpertEncoding_Code255_IsMapped()
    {
        // SID 378 is beyond the current CFFStandardString table (57 entries),
        // so it resolves to the "sid<n>" fallback name.
        Assert.Equal("sid378", CFFExpertEncoding.INSTANCE.GetName(255));
    }
}
