/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDictionary.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFDictionary : COSObjectable
{
    private static readonly COSName IdName = COSName.GetPDFName("ID");
    private static readonly COSName FieldsName = COSName.GetPDFName("Fields");
    private static readonly COSName PagesName = COSName.GetPDFName("Pages");
    private static readonly COSName StatusName = COSName.GetPDFName("Status");
    private static readonly COSName EncodingName = COSName.GetPDFName("Encoding");
    private static readonly COSName JavaScriptName = COSName.GetPDFName("JavaScript");
    private static readonly COSName AnnotsName = COSName.GetPDFName("Annots");

    private readonly COSDictionary _fdf;

    public FDFDictionary()
    {
        _fdf = new COSDictionary();
    }

    public FDFDictionary(COSDictionary fdfDictionary)
    {
        _fdf = fdfDictionary ?? throw new ArgumentNullException(nameof(fdfDictionary));
    }

    public FDFDictionary(XmlElement fdfXml)
        : this()
    {
        ArgumentNullException.ThrowIfNull(fdfXml);

        foreach (XmlElement child in ChildElements(fdfXml))
        {
            switch (child.LocalName)
            {
                case "f":
                    PDSimpleFileSpecification fs = new();
                    fs.SetFile(child.GetAttribute("href"));
                    SetFile(fs);
                    break;
                case "ids":
                    COSArray ids = new();
                    AddHexId(ids, child.GetAttribute("original"));
                    AddHexId(ids, child.GetAttribute("modified"));
                    SetID(ids);
                    break;
                case "fields":
                    List<FDFField> fieldList = [];
                    foreach (XmlElement field in ChildElements(child, "field"))
                    {
                        fieldList.Add(new FDFField(field));
                    }

                    SetFields(fieldList);
                    break;
                case "annots":
                    List<FDFAnnotation> annotationList = [];
                    foreach (XmlElement annotationElement in ChildElements(child))
                    {
                        FDFAnnotation? annotation = FDFAnnotation.CreateFromXFDF(annotationElement);
                        if (annotation is not null)
                        {
                            annotationList.Add(annotation);
                        }
                    }

                    SetAnnotations(annotationList);
                    break;
            }
        }
    }

    public void WriteXml(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);

        PDFileSpecification? fs = GetFile();
        if (fs is not null)
        {
            output.Write("<f href=\"");
            output.Write(fs.GetFile());
            output.Write("\" />\n");
        }

        COSArray? ids = GetID();
        if (ids is not null && ids.Size() >= 2
            && ids.GetObject(0) is COSString original
            && ids.GetObject(1) is COSString modified)
        {
            output.Write("<ids original=\"");
            output.Write(original.ToHexString());
            output.Write("\" ");
            output.Write("modified=\"");
            output.Write(modified.ToHexString());
            output.Write("\" />\n");
        }

        List<FDFField>? fields = GetFields();
        if (fields is { Count: > 0 })
        {
            output.Write("<fields>\n");
            foreach (FDFField field in fields)
            {
                field.WriteXml(output);
            }

            output.Write("</fields>\n");
        }
    }

    public COSBase GetCOSObject()
    {
        return _fdf;
    }

    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(_fdf.GetDictionaryObject(COSName.F));
    }

    public void SetFile(PDFileSpecification? fileSpecification)
    {
        _fdf.SetItem(COSName.F, fileSpecification);
    }

    public COSArray? GetID()
    {
        return _fdf.GetCOSArray(IdName);
    }

    public void SetID(COSArray? id)
    {
        _fdf.SetItem(IdName, id);
    }

    public List<FDFField>? GetFields()
    {
        COSArray? array = _fdf.GetCOSArray(FieldsName);
        if (array is null)
        {
            return null;
        }

        List<FDFField> fields = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dictionary)
            {
                fields.Add(new FDFField(dictionary));
            }
        }

        return fields;
    }

    public void SetFields(IList<FDFField>? fields)
    {
        _fdf.SetItem(FieldsName, fields is null ? null : new COSArray(fields));
    }

    public string? GetStatus()
    {
        return _fdf.GetString(StatusName);
    }

    public void SetStatus(string? status)
    {
        _fdf.SetString(StatusName, status);
    }

    public List<FDFPage>? GetPages()
    {
        COSArray? array = _fdf.GetCOSArray(PagesName);
        if (array is null)
        {
            return null;
        }

        List<FDFPage> pages = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dictionary)
            {
                pages.Add(new FDFPage(dictionary));
            }
        }

        return pages;
    }

    public void SetPages(IList<FDFPage>? pages)
    {
        _fdf.SetItem(PagesName, pages is null ? null : new COSArray(pages));
    }

    public string GetEncoding()
    {
        return _fdf.GetNameAsString(EncodingName) ?? "PDFDocEncoding";
    }

    public void SetEncoding(string? encoding)
    {
        _fdf.SetName(EncodingName, encoding);
    }

    public List<FDFAnnotation>? GetAnnotations()
    {
        COSArray? array = _fdf.GetCOSArray(AnnotsName);
        if (array is null)
        {
            return null;
        }

        List<FDFAnnotation> annotations = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dictionary)
            {
                FDFAnnotation? annotation = FDFAnnotation.Create(dictionary);
                if (annotation is not null)
                {
                    annotations.Add(annotation);
                }
            }
        }

        return annotations;
    }

    public void SetAnnotations(IList<FDFAnnotation>? annotations)
    {
        _fdf.SetItem(AnnotsName, annotations is null ? null : new COSArray(annotations));
    }

    public COSStream? GetDifferences()
    {
        return _fdf.GetCOSStream(COSName.DIFFERENCES);
    }

    public void SetDifferences(COSStream? differences)
    {
        _fdf.SetItem(COSName.DIFFERENCES, differences);
    }

    public string? GetTarget()
    {
        return _fdf.GetString(COSName.TARGET);
    }

    public void SetTarget(string? target)
    {
        _fdf.SetString(COSName.TARGET, target);
    }

    public IList<PDFileSpecification>? GetEmbeddedFDFs()
    {
        COSArray? embeddedArray = _fdf.GetCOSArray(COSName.EMBEDDED_FDFS);
        if (embeddedArray is null)
        {
            return null;
        }

        List<PDFileSpecification> embedded = new(embeddedArray.Size());
        for (int i = 0; i < embeddedArray.Size(); i++)
        {
            PDFileSpecification? fileSpecification = PDFileSpecification.CreateFS(embeddedArray.Get(i));
            if (fileSpecification is not null)
            {
                embedded.Add(fileSpecification);
            }
        }

        return new COSArrayList<PDFileSpecification>(embedded, embeddedArray);
    }

    public void SetEmbeddedFDFs(IList<PDFileSpecification>? embedded)
    {
        _fdf.SetItem(COSName.EMBEDDED_FDFS, embedded is null ? null : new COSArray(embedded));
    }

    public FDFJavaScript? GetJavaScript()
    {
        COSDictionary? dictionary = _fdf.GetCOSDictionary(JavaScriptName);
        return dictionary is null ? null : new FDFJavaScript(dictionary);
    }

    public void SetJavaScript(FDFJavaScript? javaScript)
    {
        _fdf.SetItem(JavaScriptName, javaScript);
    }

    private static void AddHexId(COSArray ids, string hex)
    {
        try
        {
            ids.Add(COSString.ParseHex(hex));
        }
        catch (IOException)
        {
            // Match upstream behavior: malformed XFDF ID entries are ignored.
        }
    }

    private static IEnumerable<XmlElement> ChildElements(XmlElement element, string? localName = null)
    {
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement child
                && (localName is null || string.Equals(child.LocalName, localName, StringComparison.Ordinal)))
            {
                yield return child;
            }
        }
    }
}
