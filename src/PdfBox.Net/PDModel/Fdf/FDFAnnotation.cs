/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotation.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using System.Globalization;
using System.Text;
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Fdf;

public abstract class FDFAnnotation : COSObjectable
{
    private const int FlagInvisible = 1;
    private const int FlagHidden = 1 << 1;
    private const int FlagPrinted = 1 << 2;
    private const int FlagNoZoom = 1 << 3;
    private const int FlagNoRotate = 1 << 4;
    private const int FlagNoView = 1 << 5;
    private const int FlagReadOnly = 1 << 6;
    private const int FlagLocked = 1 << 7;
    private const int FlagToggleNoView = 1 << 8;
    private const int FlagLockedContents = 1 << 9;
    private static readonly COSName SubjName = COSName.GetPDFName("Subj");
    private static readonly COSName ItName = COSName.GetPDFName("IT");

    protected readonly COSDictionary Annot;

    protected FDFAnnotation()
    {
        Annot = new COSDictionary();
        Annot.SetItem(COSName.TYPE, COSName.ANNOT);
    }

    protected FDFAnnotation(COSDictionary annotation)
    {
        Annot = annotation ?? throw new ArgumentNullException(nameof(annotation));
    }

    protected FDFAnnotation(XmlElement element)
        : this()
    {
        ArgumentNullException.ThrowIfNull(element);

        string page = element.GetAttribute("page");
        if (string.IsNullOrEmpty(page))
        {
            throw new IOException("Error: missing required attribute 'page'");
        }

        SetPage(int.Parse(page, CultureInfo.InvariantCulture));

        float[]? color = ParseColor(element.GetAttribute("color"));
        if (color is not null)
        {
            SetColor(color);
        }

        SetDate(element.GetAttribute("date"));
        ApplyFlags(element.GetAttribute("flags"));
        SetName(element.GetAttribute("name"));

        string rect = element.GetAttribute("rect");
        if (string.IsNullOrEmpty(rect))
        {
            throw new IOException("Error: missing attribute 'rect'");
        }

        SetRectangle(new PDRectangle(COSArray.Of(ParseRectangleAttributes(
            rect, "Error: wrong amount of numbers in attribute 'rect'"))));

        SetTitle(element.GetAttribute("title"));

        string creationDate = element.GetAttribute("creationdate");
        Annot.SetString(COSName.CREATION_DATE, string.IsNullOrEmpty(creationDate) ? null : creationDate);

        string opacity = element.GetAttribute("opacity");
        if (!string.IsNullOrEmpty(opacity))
        {
            SetOpacity(float.Parse(opacity, CultureInfo.InvariantCulture));
        }

        SetSubject(element.GetAttribute("subject"));

        string intent = element.GetAttribute("intent");
        if (string.IsNullOrEmpty(intent))
        {
            intent = element.GetAttribute("IT");
        }

        if (!string.IsNullOrEmpty(intent))
        {
            SetIntent(intent);
        }

        XmlElement? contents = FirstChildElement(element, "contents");
        if (contents is not null)
        {
            SetContents(contents.InnerText);
        }

        XmlElement? richContents = FirstChildElement(element, "contents-richtext");
        if (richContents is not null)
        {
            SetRichContents(RichContentsToString(richContents, root: true));
            SetContents(richContents.InnerText.Trim());
        }

        SetBorderStyle(CreateBorderStyle(element, out PDBorderEffectDictionary? borderEffect));
        if (borderEffect is not null)
        {
            SetBorderEffect(borderEffect);
        }
    }

    public static FDFAnnotation? Create(COSDictionary? dictionary)
    {
        return dictionary?.GetNameAsString(COSName.SUBTYPE) switch
        {
            FDFAnnotationText.Subtype => new FDFAnnotationText(dictionary),
            FDFAnnotationCaret.Subtype => new FDFAnnotationCaret(dictionary),
            FDFAnnotationFreeText.Subtype => new FDFAnnotationFreeText(dictionary),
            FDFAnnotationFileAttachment.Subtype => new FDFAnnotationFileAttachment(dictionary),
            FDFAnnotationHighlight.Subtype => new FDFAnnotationHighlight(dictionary),
            FDFAnnotationInk.Subtype => new FDFAnnotationInk(dictionary),
            FDFAnnotationLine.Subtype => new FDFAnnotationLine(dictionary),
            FDFAnnotationLink.Subtype => new FDFAnnotationLink(dictionary),
            FDFAnnotationCircle.Subtype => new FDFAnnotationCircle(dictionary),
            FDFAnnotationSquare.Subtype => new FDFAnnotationSquare(dictionary),
            FDFAnnotationPolygon.Subtype => new FDFAnnotationPolygon(dictionary),
            FDFAnnotationPolyline.Subtype => new FDFAnnotationPolyline(dictionary),
            FDFAnnotationSound.Subtype => new FDFAnnotationSound(dictionary),
            FDFAnnotationSquiggly.Subtype => new FDFAnnotationSquiggly(dictionary),
            FDFAnnotationStamp.Subtype => new FDFAnnotationStamp(dictionary),
            FDFAnnotationStrikeOut.Subtype => new FDFAnnotationStrikeOut(dictionary),
            FDFAnnotationUnderline.Subtype => new FDFAnnotationUnderline(dictionary),
            _ => null
        };
    }

    public static FDFAnnotation? CreateFromXFDF(XmlElement element)
    {
        return element.LocalName switch
        {
            "text" => new FDFAnnotationText(element),
            "freetext" => new FDFAnnotationFreeText(element),
            "highlight" => new FDFAnnotationHighlight(element),
            "ink" => new FDFAnnotationInk(element),
            "line" => new FDFAnnotationLine(element),
            "link" => new FDFAnnotationLink(element),
            "squiggly" => new FDFAnnotationSquiggly(element),
            "strikeout" => new FDFAnnotationStrikeOut(element),
            "underline" => new FDFAnnotationUnderline(element),
            _ => null
        };
    }

    public COSBase GetCOSObject() => Annot;

    public int? GetPage() => Annot.GetDictionaryObject(COSName.PAGE) is COSNumber page ? page.IntValue() : null;
    public void SetPage(int page) => Annot.SetInt(COSName.PAGE, page);

    public float[]? GetColor() => GetColor(COSName.C);

    protected float[]? GetColor(COSName colorName)
    {
        return Annot.GetCOSArray(colorName)?.ToFloatArray();
    }

    public void SetColor(float[]? color)
    {
        Annot.SetItem(COSName.C, color is null ? null : COSArray.Of(color));
    }

    public string? GetDate() => Annot.GetString(COSName.M);
    public void SetDate(string? date) => Annot.SetString(COSName.M, date);

    public bool IsInvisible() => Annot.GetFlag(COSName.F, FlagInvisible);
    public void SetInvisible(bool invisible) => Annot.SetFlag(COSName.F, FlagInvisible, invisible);

    public bool IsHidden() => Annot.GetFlag(COSName.F, FlagHidden);
    public void SetHidden(bool hidden) => Annot.SetFlag(COSName.F, FlagHidden, hidden);

    public bool IsPrinted() => Annot.GetFlag(COSName.F, FlagPrinted);
    public void SetPrinted(bool printed) => Annot.SetFlag(COSName.F, FlagPrinted, printed);

    public bool IsNoZoom() => Annot.GetFlag(COSName.F, FlagNoZoom);
    public void SetNoZoom(bool noZoom) => Annot.SetFlag(COSName.F, FlagNoZoom, noZoom);

    public bool IsNoRotate() => Annot.GetFlag(COSName.F, FlagNoRotate);
    public void SetNoRotate(bool noRotate) => Annot.SetFlag(COSName.F, FlagNoRotate, noRotate);

    public bool IsNoView() => Annot.GetFlag(COSName.F, FlagNoView);
    public void SetNoView(bool noView) => Annot.SetFlag(COSName.F, FlagNoView, noView);

    public bool IsReadOnly() => Annot.GetFlag(COSName.F, FlagReadOnly);
    public void SetReadOnly(bool readOnly) => Annot.SetFlag(COSName.F, FlagReadOnly, readOnly);

    public bool IsLocked() => Annot.GetFlag(COSName.F, FlagLocked);
    public void SetLocked(bool locked) => Annot.SetFlag(COSName.F, FlagLocked, locked);

    public bool IsToggleNoView() => Annot.GetFlag(COSName.F, FlagToggleNoView);
    public void SetToggleNoView(bool toggleNoView) => Annot.SetFlag(COSName.F, FlagToggleNoView, toggleNoView);

    public bool IsLockedContents() => Annot.GetFlag(COSName.F, FlagLockedContents);
    public void SetLockedContents(bool lockedContents) => Annot.SetFlag(COSName.F, FlagLockedContents, lockedContents);

    public string? GetName() => Annot.GetString(COSName.NM);
    public void SetName(string? name) => Annot.SetString(COSName.NM, name);

    public PDRectangle? GetRectangle()
    {
        COSArray? rect = Annot.GetCOSArray(COSName.RECT);
        return rect is null ? null : new PDRectangle(rect);
    }

    public void SetRectangle(PDRectangle? rectangle) => Annot.SetItem(COSName.RECT, rectangle);

    public string? GetContents() => Annot.GetString(COSName.CONTENTS);
    public void SetContents(string? contents) => Annot.SetString(COSName.CONTENTS, contents);

    public string? GetTitle() => Annot.GetString(COSName.T);
    public void SetTitle(string? title) => Annot.SetString(COSName.T, title);

    public DateTimeOffset? GetCreationDate() => Annot.GetDate(COSName.CREATION_DATE);
    public void SetCreationDate(DateTimeOffset? date) => Annot.SetDate(COSName.CREATION_DATE, date);

    public float GetOpacity() => Annot.GetFloat(COSName.CA, 1f);
    public void SetOpacity(float opacity) => Annot.SetFloat(COSName.CA, opacity);

    public string? GetSubject() => Annot.GetString(SubjName);
    public void SetSubject(string? subject) => Annot.SetString(SubjName, subject);

    public string? GetIntent() => Annot.GetNameAsString(ItName);
    public void SetIntent(string? intent) => Annot.SetName(ItName, intent);

    public string? GetRichContents() => GetStringOrStream(Annot.GetDictionaryObject(COSName.RC));
    public void SetRichContents(string? richContents)
    {
        Annot.SetItem(COSName.RC, richContents is null ? null : new COSString(richContents));
    }

    public PDBorderStyleDictionary? GetBorderStyle()
    {
        COSDictionary? dictionary = Annot.GetCOSDictionary(COSName.BS);
        return dictionary is null ? null : new PDBorderStyleDictionary(dictionary);
    }

    public void SetBorderStyle(PDBorderStyleDictionary? borderStyle) => Annot.SetItem(COSName.BS, borderStyle);

    public PDBorderEffectDictionary? GetBorderEffect()
    {
        COSDictionary? dictionary = Annot.GetCOSDictionary(COSName.BE);
        return dictionary is null ? null : new PDBorderEffectDictionary(dictionary);
    }

    public void SetBorderEffect(PDBorderEffectDictionary? borderEffect) => Annot.SetItem(COSName.BE, borderEffect);

    protected string? GetStringOrStream(COSBase? baseValue)
    {
        return baseValue switch
        {
            null => null,
            COSString cosString => cosString.GetString(),
            COSStream stream => stream.ToTextString(),
            _ => null
        };
    }

    protected static string ElementText(XmlElement element, string localName)
    {
        return FirstChildElement(element, localName)?.InnerText ?? string.Empty;
    }

    protected static XmlElement? FirstChildElement(XmlElement element, string localName)
    {
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement child
                && string.Equals(child.LocalName, localName, StringComparison.Ordinal))
            {
                return child;
            }
        }

        return null;
    }

    protected static IEnumerable<XmlElement> ChildElements(XmlElement element, string localName)
    {
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement child
                && string.Equals(child.LocalName, localName, StringComparison.Ordinal))
            {
                yield return child;
            }
        }
    }

    protected static float[] ParseRectangleAttributes(string rect, string errorMessage)
    {
        string[] rectValues = SplitLikeJava(rect, ',');
        if (rectValues.Length != 4)
        {
            throw new IOException(errorMessage);
        }

        return ParseFloats(rectValues);
    }

    protected static float[] ParseFloats(string[] sourceValues)
    {
        float[] values = new float[sourceValues.Length];
        for (int i = 0; i < sourceValues.Length; i++)
        {
            values[i] = float.Parse(sourceValues[i], CultureInfo.InvariantCulture);
        }

        return values;
    }

    protected static string[] SplitLikeJava(string value, params char[] separators)
    {
        string[] parts = value.Split(separators);
        int length = parts.Length;
        while (length > 0 && parts[length - 1].Length == 0)
        {
            length--;
        }

        if (length == parts.Length)
        {
            return parts;
        }

        string[] trimmed = new string[length];
        Array.Copy(parts, trimmed, length);
        return trimmed;
    }

    protected static float[]? ParseColor(string color)
    {
        if (color.Length != 7 || color[0] != '#')
        {
            return null;
        }

        int colorValue = int.Parse(color[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return
        [
            ((colorValue >> 16) & 0xff) / 255f,
            ((colorValue >> 8) & 0xff) / 255f,
            (colorValue & 0xff) / 255f
        ];
    }

    private void ApplyFlags(string flags)
    {
        if (flags.Length == 0)
        {
            return;
        }

        foreach (string flagToken in flags.Split(','))
        {
            switch (flagToken)
            {
                case "invisible":
                    SetInvisible(true);
                    break;
                case "hidden":
                    SetHidden(true);
                    break;
                case "print":
                    SetPrinted(true);
                    break;
                case "nozoom":
                    SetNoZoom(true);
                    break;
                case "norotate":
                    SetNoRotate(true);
                    break;
                case "noview":
                    SetNoView(true);
                    break;
                case "readonly":
                    SetReadOnly(true);
                    break;
                case "locked":
                    SetLocked(true);
                    break;
                case "togglenoview":
                    SetToggleNoView(true);
                    break;
            }
        }
    }

    private static PDBorderStyleDictionary CreateBorderStyle(
        XmlElement element,
        out PDBorderEffectDictionary? borderEffect)
    {
        borderEffect = null;
        PDBorderStyleDictionary borderStyle = new();
        string width = element.GetAttribute("width");
        if (!string.IsNullOrEmpty(width))
        {
            borderStyle.SetWidth(float.Parse(width, CultureInfo.InvariantCulture));
        }

        if (borderStyle.GetWidth() > 0)
        {
            string style = element.GetAttribute("style");
            if (!string.IsNullOrEmpty(style))
            {
                switch (style)
                {
                    case "dash":
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_DASHED);
                        break;
                    case "bevelled":
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_BEVELED);
                        break;
                    case "inset":
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_INSET);
                        break;
                    case "underline":
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_SOLID);
                        break;
                    case "cloudy":
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_SOLID);
                        borderEffect = new PDBorderEffectDictionary();
                        borderEffect.SetStyle(PDBorderEffectDictionary.STYLE_CLOUDY);
                        string intensity = element.GetAttribute("intensity");
                        if (!string.IsNullOrEmpty(intensity))
                        {
                            borderEffect.SetIntensity(float.Parse(intensity, CultureInfo.InvariantCulture));
                        }

                        break;
                    default:
                        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_SOLID);
                        break;
                }
            }

            string dashes = element.GetAttribute("dashes");
            if (!string.IsNullOrEmpty(dashes))
            {
                COSArray dashPattern = new();
                foreach (string dashesValue in SplitLikeJava(dashes, ','))
                {
                    dashPattern.Add(COSNumber.Get(dashesValue));
                }

                borderStyle.SetDashStyle(dashPattern);
            }
        }

        return borderStyle;
    }

    private static string RichContentsToString(XmlNode node, bool root)
    {
        StringBuilder sb = new();
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child)
            {
                case XmlElement element:
                    sb.Append(RichContentsToString(element, root: false));
                    break;
                case XmlCDataSection cdata:
                    sb.Append("<![CDATA[").Append(cdata.Data).Append("]]>");
                    break;
                case XmlText text:
                    sb.Append(text.Data.Replace("&", "&amp;", StringComparison.Ordinal)
                        .Replace("<", "&lt;", StringComparison.Ordinal));
                    break;
            }
        }

        if (root)
        {
            return sb.ToString();
        }

        StringBuilder attributes = new();
        if (node.Attributes is not null)
        {
            foreach (XmlAttribute attribute in OrderedAttributes(node.Attributes))
            {
                string value = attribute.Value.Replace("\"", "&quot;", StringComparison.Ordinal);
                attributes.Append(' ')
                    .Append(attribute.Name)
                    .Append("=\"")
                    .Append(value)
                    .Append('"');
            }
        }

        return "<" + node.Name + attributes + ">" + sb + "</" + node.Name + ">";
    }

    private static IEnumerable<XmlAttribute> OrderedAttributes(XmlAttributeCollection attributes)
    {
        foreach (XmlAttribute attribute in attributes)
        {
            if (!attribute.Name.StartsWith("xmlns", StringComparison.Ordinal))
            {
                yield return attribute;
            }
        }

        foreach (XmlAttribute attribute in attributes)
        {
            if (attribute.Name.StartsWith("xmlns", StringComparison.Ordinal))
            {
                yield return attribute;
            }
        }
    }
}
