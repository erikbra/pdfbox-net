/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/markedcontent/PDMarkedContent.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;
using PdfBox.Net.Text;

namespace PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

/// <summary>
/// A marked content sequence in a PDF content stream.
/// Collects text positions, nested marked-content sequences, and XObject references
/// that fall within the sequence delimited by the BMC/BDC and EMC operators.
/// </summary>
public class PDMarkedContent
{
    private static readonly COSName McidName = COSName.GetPDFName("MCID");
    private static readonly COSName AltName = COSName.GetPDFName("Alt");
    private static readonly COSName ActualTextName = COSName.GetPDFName("ActualText");
    private static readonly COSName ExpandedFormName = COSName.GetPDFName("E");

    private readonly List<PDMarkedContent> _markedContents = new();
    private readonly List<TextPosition> _texts = new();
    private readonly List<PDXObject> _xobjects = new();
    private readonly List<object> _contents = new();

    protected PDMarkedContent(COSName tag, COSDictionary? properties)
    {
        Tag = tag;
        Properties = properties;
    }

    /// <summary>The tag name of this marked-content sequence.</summary>
    public COSName Tag { get; }

    /// <summary>The optional properties dictionary for this sequence.</summary>
    public COSDictionary? Properties { get; }

    /// <summary>The marked-content identifier, or -1 if none exists.</summary>
    public int GetMCID() => Properties is null ? -1 : Properties.GetInt(McidName);

    /// <summary>The language override (Lang), if present.</summary>
    public string? GetLanguage() => Properties?.GetNameAsString(COSName.LANG);

    /// <summary>Creates a new marked-content sequence with the given tag and properties.</summary>
    public static PDMarkedContent Create(COSName tag, COSDictionary? properties)
    {
        if (tag.Equals(COSName.GetPDFName("Artifact")))
        {
            return new PDArtifactMarkedContent(properties ?? new COSDictionary());
        }

        return new PDMarkedContent(tag, properties);
    }

    /// <summary>Adds a nested marked-content sequence.</summary>
    public void AddMarkedContent(PDMarkedContent markedContent)
    {
        _markedContents.Add(markedContent);
        _contents.Add(markedContent);
    }

    /// <summary>Adds a text position collected inside this sequence.</summary>
    public void AddText(TextPosition text)
    {
        _texts.Add(text);
        _contents.Add(text);
    }

    /// <summary>Adds an XObject reference collected inside this sequence.</summary>
    public void AddXObject(PDXObject xobject)
    {
        _xobjects.Add(xobject);
        _contents.Add(xobject);
    }

    /// <summary>Returns the ActualText override from the properties dictionary, if present.</summary>
    public string? GetActualText() => Properties?.GetString(ActualTextName);

    /// <summary>Returns the alternate description (Alt), if present.</summary>
    public string? GetAlternateDescription() => Properties?.GetString(AltName);

    /// <summary>Returns the expanded form (E), if present.</summary>
    public string? GetExpandedForm() => Properties?.GetString(ExpandedFormName);

    /// <summary>Returns all nested marked-content sequences, in order of appearance.</summary>
    public IReadOnlyList<PDMarkedContent> GetMarkedContents() => _markedContents;

    /// <summary>Returns all text positions collected within this sequence.</summary>
    public IReadOnlyList<TextPosition> GetTexts() => _texts;

    /// <summary>Returns all XObject references collected within this sequence.</summary>
    public IReadOnlyList<PDXObject> GetXObjects() => _xobjects;

    /// <summary>Returns all content objects collected within this sequence.</summary>
    public List<object> GetContents() => _contents;

    public override string ToString()
    {
        return $"tag={Tag}, properties={Properties}, contents={string.Join(", ", _contents)}";
    }
}
