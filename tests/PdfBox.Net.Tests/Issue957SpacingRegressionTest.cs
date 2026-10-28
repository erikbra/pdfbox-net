/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/resources/input/PDFBOX-5920-4MQTG6ZXOYSMTQ444KGQOVC6ZFQHWFNY-spaces-reduced.pdf
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: adapted
 * Apache PDFBox corpus regression; original fixture bytes retained unchanged.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class Issue957SpacingRegressionTest
{
    [Fact]
    public void Type1CFontWithCustomEncodingPreservesInferredWordSpaces()
    {
        using PDDocument document = PDDocument.Load(Path.Combine("Fixtures", "Rendering", "PDFBOX-5920-4MQTG6ZXOYSMTQ444KGQOVC6ZFQHWFNY-spaces-reduced.pdf"));
        PDType1CFont font = Assert.IsType<PDType1CFont>(document.GetPage(0).GetResources()!.GetFont(COSName.GetPDFName("T1_1")));
        // Java PDFBox returns 139 for this CFF's space: nominalWidthX 650 + delta -511.
        Assert.Equal(139f, font.GetStringWidth(" "));
        Assert.Equal(139f, font.GetSpaceWidth());
        Assert.Equal("Ce guide va vous permettre de découvrir", new PDFTextStripper().GetText(document).Trim());
    }
}
