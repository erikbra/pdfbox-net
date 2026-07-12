// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestHelloWorld.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Resources;
using System.Text;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test of HelloWorld and HelloWorldTTF examples.
/// Ported from TestHelloWorld.java.
/// </summary>
public class TestHelloWorld
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-helloworld");

    public TestHelloWorld()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestHelloWorldCreatesFile()
    {
        string outputFile = Path.Combine(OutputDir, "HelloWorld.pdf");
        File.Delete(outputFile);
        // HelloWorld expects exactly 2 args: <output-file> <message>
        string[] args = { outputFile, "Hello World!" };
        HelloWorld.Main(args);
        Assert.True(File.Exists(outputFile), "HelloWorld should have created the PDF");
    }

    [Fact]
    public void TestHelloWorldTTFCreatesFile()
    {
        const string ttfFont = "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf";
        if (!File.Exists(ttfFont))
            Assert.Skip("LiberationSans-Regular.ttf not available on this system");

        string outputFile = Path.Combine(OutputDir, "HelloWorldTTF.pdf");
        File.Delete(outputFile);
        // HelloWorldTTF expects 3 args: <output-file> <message> <ttf-file>
        string[] args = { outputFile, "Hello World TTF!", ttfFont };
        HelloWorldTTF.Main(args);
        Assert.True(File.Exists(outputFile), "HelloWorldTTF should have created the PDF");
    }

    [Fact]
    public void TestHelloWorldType1CreatesFileWithEmbeddedFont()
    {
        string outputFile = Path.Combine(OutputDir, "HelloWorldType1.pdf");
        string pfbFile = Path.Combine(OutputDir, "HelloWorldType1.pfb");
        File.Delete(outputFile);
        File.WriteAllBytes(pfbFile, CreateMinimalType1Pfb());

        try
        {
            // The synthetic PFB contains only .notdef and A; use the glyph it actually encodes.
            string[] args = { outputFile, "A", pfbFile };
            HelloWorldType1.Main(args);

            Assert.True(File.Exists(outputFile), "HelloWorldType1 should have created the PDF");

            using PDDocument document = PDDocument.Load(outputFile);
            PDPage page = document.GetPage(0);
            PDResources? resources = page.GetResources();
            Assert.NotNull(resources);

            COSName fontName = Assert.Single(resources.GetFontNames());
            var font = resources.GetFont(fontName);
            Assert.NotNull(font);
            COSDictionary fontDictionary = Assert.IsType<COSDictionary>(font.GetCOSObject());
            COSDictionary fontDescriptor = Assert.IsType<COSDictionary>(
                fontDictionary.GetDictionaryObject(COSName.GetPDFName("FontDescriptor")));
            COSDictionary fontFile = Assert.IsAssignableFrom<COSDictionary>(
                fontDescriptor.GetDictionaryObject(COSName.GetPDFName("FontFile")));
            Assert.True(fontFile.GetInt(COSName.GetPDFName("Length1"), 0) > 0);
            Assert.True(fontFile.GetInt(COSName.GetPDFName("Length2"), 0) > 0);
        }
        finally
        {
            File.Delete(pfbFile);
        }
    }

    private static byte[] CreateMinimalType1Pfb()
    {
        string segment1Text = string.Join('\n',
        [
            "%!FontType1-1.0: TestFont 1.0",
            "10 dict begin",
            "/FontName /TestFont def",
            "/FontInfo 5 dict dup begin",
            "/version (1.0) readonly def",
            "/Notice (Test notice) readonly def",
            "/FullName (Test Font) readonly def",
            "/FamilyName (Test Family) readonly def",
            "/Weight (Regular) readonly def",
            "end readonly def",
            "/isFixedPitch false def",
            "/ItalicAngle 0 def",
            "/UnderlinePosition -100 def",
            "/UnderlineThickness 50 def",
            "/Encoding 256 array",
            "dup 0 /.notdef put",
            "dup 65 /A put",
            "readonly def",
            "/FontBBox [0 0 500 700] readonly def",
            "/FontMatrix [0.001 0 0 0.001 0 0] readonly def",
            "currentdict end",
            "currentfile eexec",
        ]) + "\n";

        byte[] notdef = EncryptType1CharString([14]);
        byte[] glyphA = EncryptType1CharString([139, 14]);
        byte[] subr0 = EncryptType1CharString([11]);

        using MemoryStream clear = new();
        WriteAscii(clear, "/Private 8 dict dup begin\n");
        WriteAscii(clear, "/lenIV 4 def\n");
        WriteAscii(clear, "/BlueValues [0 10] def\n");
        WriteAscii(clear, "/ForceBold false def\n");
        WriteAscii(clear, "/Subrs 1 array\n");
        WriteAscii(clear, $"dup 0 {subr0.Length} RD ");
        clear.Write(subr0);
        WriteAscii(clear, " NP\n");
        WriteAscii(clear, "/CharStrings 2 dict dup begin\n");
        WriteAscii(clear, $"/.notdef {notdef.Length} RD ");
        clear.Write(notdef);
        WriteAscii(clear, " ND\n");
        WriteAscii(clear, $"/A {glyphA.Length} RD ");
        clear.Write(glyphA);
        WriteAscii(clear, " ND\n");
        WriteAscii(clear, "end\nend\ncleartomark\n");

        byte[] segment1 = Encoding.ASCII.GetBytes(segment1Text);
        byte[] segment2 = Encrypt(clear.ToArray(), 55665, 4);
        return BuildPfb(segment1, segment2);
    }

    private static byte[] BuildPfb(byte[] segment1, byte[] segment2)
    {
        using MemoryStream stream = new();
        WritePfbRecord(stream, 0x01, segment1);
        WritePfbRecord(stream, 0x02, segment2);
        stream.WriteByte(0x80);
        stream.WriteByte(0x03);
        return stream.ToArray();
    }

    private static void WritePfbRecord(Stream stream, byte type, byte[] data)
    {
        stream.WriteByte(0x80);
        stream.WriteByte(type);
        WriteInt32LittleEndian(stream, data.Length);
        stream.Write(data);
    }

    private static byte[] EncryptType1CharString(byte[] clear) => Encrypt(clear, 4330, 4);

    private static byte[] Encrypt(byte[] clear, int seed, int discard)
    {
        byte[] plain = new byte[discard + clear.Length];
        Array.Copy(clear, 0, plain, discard, clear.Length);
        byte[] cipher = new byte[plain.Length];
        int r = seed;
        for (int i = 0; i < plain.Length; i++)
        {
            int plainByte = plain[i] & 0xFF;
            int cipherByte = plainByte ^ (r >> 8);
            cipher[i] = (byte)cipherByte;
            r = ((cipherByte + r) * 52845 + 22719) & 0xFFFF;
        }

        return cipher;
    }

    private static void WriteAscii(Stream stream, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes);
    }

    private static void WriteInt32LittleEndian(Stream stream, int value)
    {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
    }
}
