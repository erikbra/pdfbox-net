/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java
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
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed partial class PDAcroForm : COSObjectable
{
    private readonly PDDocument _document;
    private readonly COSDictionary _dictionary;

    public PDAcroForm(PDDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.GetPDFName("Fields"), new COSArray());
    }

    public PDAcroForm(PDDocument document, COSDictionary dictionary)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject()
    {
        return _dictionary;
    }

    public IList<PDField> GetFields()
    {
        COSArray? array = _dictionary.GetCOSArray(COSName.GetPDFName("Fields"));
        if (array == null)
        {
            return new COSArrayList<PDField>(_dictionary, COSName.GetPDFName("Fields"));
        }

        List<PDField> fields = new(array.Size());
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary fieldDict)
            {
                fields.Add(PDField.FromDictionary(this, fieldDict));
            }
        }
        return new COSArrayList<PDField>(fields, array);
    }

    public void SetFields(IList<PDField>? fields)
    {
        _dictionary.SetItem(COSName.GetPDFName("Fields"), COSArrayList<object>.ConverterToCOSArray(fields?.Cast<object>().ToList()));
    }

    public IEnumerator<PDField> GetFieldIterator()
    {
        return new PDFieldTree(this).GetEnumerator();
    }

    public PDFieldTree GetFieldTree()
    {
        return new PDFieldTree(this);
    }

    public string GetDefaultAppearance()
    {
        return _dictionary.GetString(COSName.GetPDFName("DA"), string.Empty) ?? string.Empty;
    }

    public void SetDefaultAppearance(string? daValue)
    {
        _dictionary.SetString(COSName.GetPDFName("DA"), daValue);
    }

    public bool GetNeedAppearances()
    {
        return _dictionary.GetBoolean(COSName.GetPDFName("NeedAppearances"), false);
    }

    public void SetNeedAppearances(bool? value)
    {
        if (value.HasValue)
        {
            _dictionary.SetBoolean(COSName.GetPDFName("NeedAppearances"), value.Value);
        }
        else
        {
            _dictionary.RemoveItem(COSName.GetPDFName("NeedAppearances"));
        }
    }

    public PDResources? GetDefaultResources()
    {
        COSDictionary? dr = _dictionary.GetCOSDictionary(COSName.GetPDFName("DR"));
        return dr == null ? null : new PDResources(dr);
    }

    public void SetDefaultResources(PDResources? resources)
    {
        _dictionary.SetItem(COSName.GetPDFName("DR"), resources?.GetCOSObject());
    }

    public bool IsSignaturesExist()
    {
        int flags = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        return (flags & 1) != 0;
    }

    public void SetSignaturesExist(bool value)
    {
        int current = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        _dictionary.SetInt(COSName.GetPDFName("SigFlags"), value ? (current | 1) : (current & ~1));
    }

    public bool IsAppendOnly()
    {
        int flags = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        return (flags & 2) != 0;
    }

    public void SetAppendOnly(bool value)
    {
        int current = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        _dictionary.SetInt(COSName.GetPDFName("SigFlags"), value ? (current | 2) : (current & ~2));
    }

    public PDXFAResource? GetXFA()
    {
        COSBase? baseValue = _dictionary.GetDictionaryObject(COSName.GetPDFName("XFA"));
        return baseValue != null ? new PDXFAResource(baseValue) : null;
    }

    public void SetXFA(PDXFAResource? xfa)
    {
        _dictionary.SetItem(COSName.GetPDFName("XFA"), xfa?.GetCOSObject());
    }

    internal PDDocument GetDocument()
    {
        return _document;
    }

    public void Flatten()
    {
        if (XfaIsDynamic())
        {
            return;
        }

        RefreshAppearances();

        HashSet<COSDictionary> widgetsToFlatten = [];
        foreach (PDField field in GetFieldTree())
        {
            foreach (PDAnnotationWidget widget in field.GetWidgets())
            {
                widgetsToFlatten.Add(widget.GetCOSDictionary());
            }
        }

        foreach (PDPage page in _document.GetPages())
        {
            IList<PDAnnotation> annotations = page.GetAnnotations();
            if (annotations.Count == 0)
            {
                continue;
            }

            List<PDAnnotation> remainingAnnotations = [];
            PDPageContentStream? contentStream = null;
            try
            {
                foreach (PDAnnotation annotation in annotations)
                {
                    if (!widgetsToFlatten.Contains(annotation.GetCOSDictionary()))
                    {
                        remainingAnnotations.Add(annotation);
                        continue;
                    }

                    PDAppearanceStream? appearanceStream = annotation.GetNormalAppearanceStream();
                    if (appearanceStream == null || !IsVisibleAnnotation(annotation, appearanceStream))
                    {
                        continue;
                    }

                    COSStream appearanceCosStream = appearanceStream.GetCOSObject()
                        ?? throw new InvalidOperationException("Widget appearance stream is missing a COS stream.");
                    contentStream ??= new PDPageContentStream(_document, page, PDPageContentStream.AppendMode.APPEND, false);
                    contentStream.SaveGraphicsState();
                    contentStream.Transform(ResolveTransformationMatrix(annotation, appearanceStream));
                    contentStream.DrawForm(new PDFormXObject(appearanceCosStream));
                    contentStream.RestoreGraphicsState();
                }
            }
            finally
            {
                contentStream?.Dispose();
            }

            page.SetAnnotations(remainingAnnotations);
        }

        SetFields([]);
        _document.GetDocumentCatalog().SetAcroForm(null);
    }

    public void RefreshAppearances()
    {
        foreach (PDField field in GetFieldTree())
        {
            if (field is PDVariableText variableText)
            {
                variableText.ConstructAppearances();
            }
        }
    }

    private bool HasXFA()
    {
        return _dictionary.ContainsKey(COSName.GetPDFName("XFA"));
    }

    private bool XfaIsDynamic()
    {
        return HasXFA() && GetFields().Count == 0;
    }

    private static bool IsVisibleAnnotation(PDAnnotation annotation, PDAppearanceStream? normalAppearanceStream)
    {
        if (annotation.IsInvisible() || annotation.IsHidden())
        {
            return false;
        }

        PDRectangle? bbox = normalAppearanceStream?.GetBBox();
        return bbox != null && bbox.GetWidth() > 0 && bbox.GetHeight() > 0;
    }

    private static Matrix ResolveTransformationMatrix(PDAnnotation annotation, PDAppearanceStream appearanceStream)
    {
        PDRectangle annotationRectangle = annotation.GetRectangle()
            ?? throw new InvalidOperationException("Widget annotation is missing a rectangle.");

        (float x, float y, float width, float height) = GetTransformedAppearanceBounds(appearanceStream);
        return new Matrix(
            annotationRectangle.GetWidth() / width,
            0,
            0,
            annotationRectangle.GetHeight() / height,
            annotationRectangle.GetLowerLeftX() - x,
            annotationRectangle.GetLowerLeftY() - y);
    }

    private static (float x, float y, float width, float height) GetTransformedAppearanceBounds(PDAppearanceStream appearanceStream)
    {
        PDRectangle bbox = appearanceStream.GetBBox()
            ?? throw new InvalidOperationException("Widget appearance stream is missing a bounding box.");

        Matrix matrix = appearanceStream.GetMatrix();
        Vector[] corners =
        [
            matrix.TransformPoint(bbox.GetLowerLeftX(), bbox.GetLowerLeftY()),
            matrix.TransformPoint(bbox.GetUpperRightX(), bbox.GetLowerLeftY()),
            matrix.TransformPoint(bbox.GetUpperRightX(), bbox.GetUpperRightY()),
            matrix.TransformPoint(bbox.GetLowerLeftX(), bbox.GetUpperRightY())
        ];

        float minX = corners.Min(corner => corner.GetX());
        float minY = corners.Min(corner => corner.GetY());
        float maxX = corners.Max(corner => corner.GetX());
        float maxY = corners.Max(corner => corner.GetY());
        float width = maxX - minX;
        float height = maxY - minY;

        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException("Widget appearance stream has a non-positive transformed bounding box.");
        }

        return (minX, minY, width, height);
    }
}
