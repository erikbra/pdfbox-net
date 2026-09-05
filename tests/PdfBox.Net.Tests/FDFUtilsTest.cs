/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/fdf/FDFUtilsTest.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
 */

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

using System.Xml.Linq;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Fdf;

namespace PdfBox.Net.Tests;

public class FDFUtilsTest
{
    [Theory]
    [InlineData("Hello World 123", "Hello World 123")]
    [InlineData("", "")]
    [InlineData("<", "&lt;")]
    [InlineData(">", "&gt;")]
    [InlineData("&", "&amp;")]
    [InlineData("\"", "&quot;")]
    [InlineData("'", "&apos;")]
    [InlineData("<tag attr=\"value\" other='x'>&</tag>", "&lt;tag attr=&quot;value&quot; other=&apos;x&apos;&gt;&amp;&lt;/tag&gt;")]
    [InlineData("line1\tline2\nline3\rline4", "line1\tline2\nline3\rline4")]
    [InlineData("caf\u00e9", "caf&#233;")]
    [InlineData("\u00e9\u00e8", "&#233;&#232;")]
    [InlineData("a\u000bb", "a\ufffdb")]
    [InlineData("\U0001F600", "&#128512;")]
    public void EscapeXML10PreservesValidCodePoints(string input, string expected)
    {
        Assert.Equal(expected, FDFUtils.EscapeXML10(input));
    }

    [Fact]
    public void EscapeXML10ReplacesUnpairedSurrogatesAndForbiddenCodePoints()
    {
        string input = new(['\0', '\u001F', '\uD800', 'x', '\uDC00', '\uFFFE', '\uFFFF']);
        string escaped = FDFUtils.EscapeXML10(input);

        Assert.Equal("\uFFFD\uFFFD\uFFFDx\uFFFD\uFFFD\uFFFD", escaped);
        Assert.Equal(escaped, XElement.Parse("<value>" + escaped + "</value>").Value);
    }

    [Fact]
    public void EscapeXML10RetainsBoundaryCodePoints()
    {
        Assert.Equal("&#55295;&#57344;&#65533;&#65536;&#1114111;",
            FDFUtils.EscapeXML10("\uD7FF\uE000\uFFFD\U00010000\U0010FFFF"));
    }

    [Fact]
    public void FieldWriterEscapesFieldNameValuesRichTextAndChildNames()
    {
        FDFField field = new();
        const string name = "name\"<&'\U0001F600";
        field.SetPartialFieldName(name);
        field.SetValue((object)new List<string> { "first <&", "second \U0001F600" });
        field.SetRichText(new COSString("<body>\U0001F600 & text</body>"));
        FDFField child = new();
        child.SetPartialFieldName("child<&\"");
        child.SetValue("single <& \U0001F600");
        field.SetKids([child]);

        using StringWriter output = new();
        field.WriteXml(output);
        XElement xml = XElement.Parse(output.ToString());

        Assert.Equal(name, xml.Attribute("name")!.Value);
        Assert.Equal(["first <&", "second \U0001F600"], xml.Elements("value").Select(e => e.Value));
        Assert.Equal("<body>\U0001F600 & text</body>", xml.Element("value-richtext")!.Value);
        Assert.Equal("child<&\"", xml.Element("field")!.Attribute("name")!.Value);
        Assert.Equal("single <& \U0001F600", xml.Element("field")!.Element("value")!.Value);
    }

    [Fact]
    public void FieldWriterRejectsMissingNameBeforeWriting()
    {
        using StringWriter output = new();
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => new FDFField().WriteXml(output));

        Assert.Equal("Field name is missing", error.Message);
        Assert.Empty(output.ToString());
    }

    [Fact]
    public void DictionaryWriterEscapesFileNameAndOmitsMissingFileName()
    {
        FDFDictionary dictionary = new();
        PDComplexFileSpecification file = new();
        dictionary.SetFile(file);
        using StringWriter missingOutput = new();
        dictionary.WriteXml(missingOutput);
        Assert.DoesNotContain("<f ", missingOutput.ToString());

        const string filename = "<report>&\"'\U0001F600.pdf";
        file.SetFile(filename);
        using StringWriter output = new();
        dictionary.WriteXml(output);

        Assert.Equal(filename, XElement.Parse(output.ToString()).Attribute("href")!.Value);
    }
}
