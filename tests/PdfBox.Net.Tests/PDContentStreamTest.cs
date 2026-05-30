/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for the shared PDContentStream abstraction introduced for parity mapping.
 */

using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Patterns;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDContentStreamTest
{
    [Fact]
    public void PDPageContentStreamConcatenatesArrayContents()
    {
        PDPage page = new();
        COSArray contents = new();
        contents.Add(CreateStream("BT "));
        contents.Add(CreateStream("/F1 12 Tf"));
        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.CONTENTS, contents);

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);

        Assert.Equal("BT \n/F1 12 Tf", reader.ReadToEnd());
    }

    [Fact]
    public void PDFormXObjectExposesRandomAccessContent()
    {
        PDFormXObject form = new(new COSStream());
        using (Stream output = form.GetCOSObject()!.CreateOutputStream())
        {
            byte[] bytes = Encoding.ASCII.GetBytes("q Q");
            output.Write(bytes, 0, bytes.Length);
        }

        using var randomAccess = form.GetContentsForRandomAccess();
        byte[] buffer = new byte[3];
        Assert.Equal(3, randomAccess.Read(buffer, 0, buffer.Length));
        Assert.Equal("q Q", Encoding.ASCII.GetString(buffer));
    }

    [Fact]
    public void PDTilingPatternImplementsContentStreamContract()
    {
        PDTilingPattern pattern = new();
        using (Stream output = ((COSStream)pattern.GetCOSObject()).CreateOutputStream())
        {
            byte[] bytes = Encoding.ASCII.GetBytes("0 0 m");
            output.Write(bytes, 0, bytes.Length);
        }

        Assert.NotNull(((PDContentStream)pattern).GetContentsForStreamParsing());
        Assert.NotNull(pattern.GetMatrix());
    }

    private static COSStream CreateStream(string text)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        output.Write(bytes, 0, bytes.Length);
        return stream;
    }
}
