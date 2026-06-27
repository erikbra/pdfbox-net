/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.FontBox.Tests;

public class TTFNameRecordTest
{
    [Fact]
    public void PropertiesProxyJavaStyleAccessors()
    {
        NameRecord record = new();

        record.SetPlatformId(NameRecord.PLATFORM_WINDOWS);
        record.SetPlatformEncodingId(NameRecord.ENCODING_WINDOWS_UNICODE_BMP);
        record.SetLanguageId(NameRecord.LANGUAGE_WINDOWS_EN_US);
        record.SetNameId(NameRecord.NAME_FULL_FONT_NAME);
        record.SetStringLength(12);
        record.SetStringOffset(34);
        record.SetString("font name");

        Assert.Equal(NameRecord.PLATFORM_WINDOWS, record.PlatformId);
        Assert.Equal(NameRecord.ENCODING_WINDOWS_UNICODE_BMP, record.PlatformEncodingId);
        Assert.Equal(NameRecord.LANGUAGE_WINDOWS_EN_US, record.LanguageId);
        Assert.Equal(NameRecord.NAME_FULL_FONT_NAME, record.NameId);
        Assert.Equal(12, record.StringLength);
        Assert.Equal(34, record.StringOffset);
        Assert.Equal("font name", record.String);

        record.PlatformId = NameRecord.PLATFORM_MACINTOSH;
        record.String = "mac name";

        Assert.Equal(NameRecord.PLATFORM_MACINTOSH, record.GetPlatformId());
        Assert.Equal("mac name", record.GetString());
        Assert.Equal("mac name", record.Value);
    }
}
