// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdfa/CreatePDFATest.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Copyright 2015 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.Examples.Tests.PDFA;

/// <summary>
/// Test of CreatePDFA example.
/// Ported from CreatePDFATest.java. Preflight/VeraPDF compliance validation remains an
/// accepted external-validation adaptation for the 3.0 branch; these tests cover deterministic
/// structure that can be verified offline in .NET.
/// </summary>
public class CreatePDFATest
{
    [Fact]
    public void TestCreatePDFA()
    {
        string outDir = ExampleTestResources.CreateTempDirectory("examples-tests-pdfa");
        string fontPath = ExampleTestResources.WriteLiberationSansRegular(outDir);
        string pdfaFile = Path.Combine(outDir, "PDFA.pdf");

        CreatePDFA.Main(new string[] { pdfaFile, "The quick brown fox", fontPath });

        Assert.True(File.Exists(pdfaFile), "CreatePDFA should have created the PDF");

        using PDDocument document = PDDocument.Load(pdfaFile);
        Assert.Equal(1, document.GetNumberOfPages());

        PDFont? font = Assert.Single(document.GetPage(0).GetResources()!.GetFontNames()
            .Select(name => document.GetPage(0).GetResources()!.GetFont(name)));
        Assert.NotNull(font);
        Assert.True(font.IsEmbedded(), "CreatePDFA should embed the TrueType font it uses.");

        PDMetadata metadata = Assert.IsType<PDMetadata>(
            document.GetDocumentCatalog().GetMetadata());
        XmlDocument xmp = LoadXmp(metadata);

        Assert.Equal(pdfaFile, SelectText(xmp, "title"));
        Assert.Equal("1", SelectText(xmp, "part"));
        Assert.Equal("B", SelectText(xmp, "conformance"));
    }

    [Fact]
    public void TestCreatePDFAAddsOutputIntentWhenProfileIsProvided()
    {
        string outDir = ExampleTestResources.CreateTempDirectory("examples-tests-pdfa-intent");
        string fontPath = ExampleTestResources.WriteLiberationSansRegular(outDir);
        string iccPath = Path.Combine(outDir, "srgb.icc");
        byte[] profileBytes = Encoding.ASCII.GetBytes("deterministic-test-profile");
        File.WriteAllBytes(iccPath, profileBytes);

        string pdfaFile = Path.Combine(outDir, "PDFA-with-output-intent.pdf");
        CreatePDFA.Main(new string[] { pdfaFile, "The quick brown fox", fontPath, iccPath });

        using PDDocument document = PDDocument.Load(pdfaFile);
        PDOutputIntent intent = Assert.Single(document.GetDocumentCatalog().GetOutputIntents());
        Assert.Equal("sRGB IEC61966-2.1", intent.GetInfo());
        Assert.Equal("sRGB IEC61966-2.1", intent.GetOutputCondition());
        Assert.Equal("sRGB IEC61966-2.1", intent.GetOutputConditionIdentifier());
        Assert.Equal("http://www.color.org", intent.GetRegistryName());

        COSStream profileStream = Assert.IsType<COSStream>(intent.GetDestOutputIntent());
        using Stream decodedProfile = profileStream.CreateInputStream();
        using MemoryStream buffer = new();
        decodedProfile.CopyTo(buffer);
        Assert.Equal(profileBytes, buffer.ToArray());
    }

    private static XmlDocument LoadXmp(PDMetadata metadata)
    {
        XmlDocument document = new();
        using Stream xmp = metadata.ExportXMPMetadata();
        document.Load(xmp);
        return document;
    }

    private static string SelectText(XmlDocument document, string localName)
    {
        XmlNode? direct = document.SelectSingleNode($"//*[local-name()='{localName}']");
        Assert.NotNull(direct);

        XmlNode? listValue = direct!.SelectSingleNode(".//*[local-name()='li']");
        return (listValue ?? direct).InnerText;
    }
}
