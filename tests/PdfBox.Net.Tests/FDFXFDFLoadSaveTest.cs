/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * XFDF load/save regression coverage for issue #451.
 */

using System.Text;
using PdfBox.Net;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Fdf;

namespace PdfBox.Net.Tests;

public class FDFXFDFLoadSaveTest
{
    [Fact]
    public void LoaderCanReadXFDFFieldsAndSaveXFDF()
    {
        string xfdf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xfdf xmlns="http://ns.adobe.com/xfdf/" xml:space="preserve">
              <f href="form.pdf" />
              <ids original="0102" modified="0A0B" />
              <fields>
                <field name="Parent">
                  <value>A&amp;B</value>
                  <value-richtext>rich &amp; text</value-richtext>
                  <field name="Child">
                    <value>Oslo</value>
                  </field>
                </field>
              </fields>
            </xfdf>
            """;

        using FDFDocument document = Loader.LoadXFDF(ToStream(xfdf));
        FDFDictionary dictionary = document.GetCatalog().GetFDF();

        Assert.Equal("form.pdf", dictionary.GetFile()?.GetFile());
        Assert.Equal("0102", ((COSString)dictionary.GetID()!.GetObject(0)!).ToHexString());
        Assert.Equal("0A0B", ((COSString)dictionary.GetID()!.GetObject(1)!).ToHexString());

        FDFField parent = Assert.Single(dictionary.GetFields()!);
        Assert.Equal("Parent", parent.GetPartialFieldName());
        Assert.Equal("A&B", parent.GetValue());
        Assert.Equal("rich & text", parent.GetRichText());

        FDFField child = Assert.Single(parent.GetKids()!);
        Assert.Equal("Child", child.GetPartialFieldName());
        Assert.Equal("Oslo", child.GetValue());

        using MemoryStream saved = new();
        document.SaveXFDF(saved);
        string savedXml = Encoding.UTF8.GetString(saved.ToArray());

        Assert.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", savedXml);
        Assert.Contains("<f href=\"form.pdf\" />", savedXml);
        Assert.Contains("<ids original=\"0102\" modified=\"0A0B\" />", savedXml);
        Assert.Contains("<field name=\"Parent\">", savedXml);
        Assert.Contains("<value>A&amp;B</value>", savedXml);
        Assert.Contains("<value-richtext>rich &amp; text</value-richtext>", savedXml);

        using FDFDocument reloaded = Loader.LoadXFDF(ToStream(savedXml));
        Assert.Equal("A&B", reloaded.GetCatalog().GetFDF().GetFields()![0].GetValue());
    }

    [Fact]
    public void LoaderCanReadXFDFAnnotations()
    {
        string xfdf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xfdf xmlns="http://ns.adobe.com/xfdf/" xml:space="preserve">
              <annots>
                <highlight page="0" rect="1,2,3,4" flags="print" coords="1,2,3,4,5,6,7,8," />
                <underline page="0" rect="1,2,3,4" flags="print" coords="1,2,3,4,5,6,7,8," />
                <strikeout page="0" rect="1,2,3,4" flags="print" coords="1,2,3,4,5,6,7,8," />
                <freetext page="0" rect="1,2,3,4" flags="print" width="0" justification="left">
                  <contents-richtext><body xmlns="http://www.w3.org/1999/xhtml" xmlns:xfa="http://www.xfa.org/schema/xfa-data/1.0/" style="font:12pt Helvetica; color:#D66C00;" xfa:APIVersion="Acrobat:7.0.8" xfa:spec="2.0.2"><p dir="ltr">P&amp;1 <span style="text-decoration:word;font-family:Helvetica">P&amp;2</span> P&amp;3</p></body></contents-richtext>
                  <defaultappearance>/Helvetica 12 Tf</defaultappearance>
                </freetext>
                <freetext page="0" rect="1,2,3,4" flags="print" width="2" callout="1,2,3,4,5,6" fringe="1,2,3,4" head="OpenArrow" />
                <text page="0" rect="1,2,3,4" flags="print" icon="Comment" />
                <ink page="0" rect="1,2,3,4" flags="print"><inklist><gesture>1,2;3,4;5,6</gesture></inklist></ink>
                <line page="0" rect="1,2,3,4" flags="print" start="1,2" end="3,4" head="Square" tail="Slash" interior-color="#1E59FF" />
                <link width="0" page="2" rect="72,454,188,467" />
                <link width="0" page="2" rect="283,418,418,431" />
                <link width="0" page="2" rect="72,400,207,413" />
                <link width="0" page="3" rect="271,655,415,669" />
                <link width="0" page="3" rect="383,565,517,579" />
                <link width="0" page="3" rect="72,547,97,561" />
                <link width="0" page="3" rect="345,457,499,471" />
                <link width="0" page="3" rect="158,205,273,219" />
                <link width="0" page="3" rect="105,187,217,201" />
                <freetext page="0" rect="1,2,3,4" flags="print" width="2" />
              </annots>
            </xfdf>
            """;

        using FDFDocument document = Loader.LoadXFDF(ToStream(xfdf));
        List<FDFAnnotation> annotations = document.GetCatalog().GetFDF().GetAnnotations()!;

        Assert.Equal(18, annotations.Count);
        Assert.IsType<FDFAnnotationHighlight>(annotations[0]);
        Assert.True(annotations[0].IsPrinted());
        Assert.IsType<FDFAnnotationUnderline>(annotations[1]);
        Assert.IsType<FDFAnnotationStrikeOut>(annotations[2]);
        Assert.IsType<FDFAnnotationInk>(annotations[6]);
        Assert.IsType<FDFAnnotationLine>(annotations[7]);

        FDFAnnotationFreeText richText = annotations.OfType<FDFAnnotationFreeText>()
            .First(annotation => annotation.GetContents() == "P&1 P&2 P&3");
        Assert.Contains("P&amp;1", richText.GetRichContents());
        Assert.Contains("<span style=\"text-decoration:word;font-family:Helvetica\">P&amp;2</span>", richText.GetRichContents());
    }

    [Fact]
    public void LoaderPreservesXFDFAnnotationZeroWidth()
    {
        string xfdf =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<xfdf xmlns=\"http://ns.adobe.com/xfdf/\" xml:space=\"preserve\">" +
            "<annots>" +
            "<freetext width=\"0.00\" justification=\"left\" page=\"0\" flags=\"print\" " +
            "rect=\"372.339325,722.633545,531.075317,736.673523\">" +
            "<defaultappearance>&#x20;/Helv 12 Tf 0.415686 0.756863 0.690196 rg</defaultappearance>" +
            "<contents>Your text is here.</contents>" +
            "</freetext>" +
            "</annots>" +
            "<f href=\".xfdf\"/>" +
            "</xfdf>";

        using FDFDocument document = Loader.LoadXFDF(ToStream(xfdf));
        FDFAnnotation annotation = Assert.Single(document.GetCatalog().GetFDF().GetAnnotations()!);

        Assert.NotNull(annotation.GetBorderStyle());
        Assert.Equal(0f, annotation.GetBorderStyle()!.GetWidth(), precision: 2);
    }

    private static MemoryStream ToStream(string text)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(text));
    }
}
