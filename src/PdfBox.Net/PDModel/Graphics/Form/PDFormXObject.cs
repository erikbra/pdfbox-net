/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/form/PDFormXObject.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Form;

public partial class PDFormXObject : PDXObject, PDContentStream
{
    private static readonly COSName FormTypeName = COSName.GetPDFName("FormType");
    private static readonly COSName BboxName = COSName.GetPDFName("BBox");
    private static readonly COSName MatrixName = COSName.GetPDFName("Matrix");
    private static readonly COSName GroupName = COSName.GetPDFName("Group");
    private static readonly COSName OptionalContentName = COSName.GetPDFName("OC");

    private PDTransparencyGroupAttributes? _group;
    private readonly ResourceCache? _resourceCache;
    private PDResources? _resources;

    public PDFormXObject(PDStream stream)
        : base(stream)
    {
        InitializeSubtype();
    }

    public PDFormXObject(COSStream stream)
        : this(stream, null)
    {
    }

    /// <summary>Creates a Form XObject for reading with a cache for its resources.</summary>
    /// <param name="stream">The XObject stream.</param>
    /// <param name="resourceCache">The cache to use for the form resources.</param>
    public PDFormXObject(COSStream stream, ResourceCache? resourceCache)
        : base(stream)
    {
        _resourceCache = resourceCache;
        InitializeSubtype();
    }

    private void InitializeSubtype()
    {
        SetXObjectSubtype("Form");
    }

    public int GetFormType() => GetCOSObject()?.GetInt(FormTypeName, 1) ?? 1;

    public void SetFormType(int formType) => GetCOSObject()?.SetInt(FormTypeName, formType);

    public PDTransparencyGroupAttributes? GetGroup()
    {
        if (_group is null && GetCOSObject()?.GetCOSDictionary(GroupName) is COSDictionary dictionary)
        {
            _group = new PDTransparencyGroupAttributes(dictionary);
        }

        return _group;
    }

    public void SetGroup(PDTransparencyGroupAttributes? group)
    {
        _group = group;
        GetCOSObject()?.SetItem(GroupName, group);
    }

    public PDStream GetContentStream() => new(GetCOSObject()!);

    public Stream GetContents() => GetContentStream().CreateInputStream();

    public RandomAccessRead GetContentsForRandomAccess() => new RandomAccessReadBuffer(GetContents());

    public PDResources? GetResources()
    {
        COSDictionary? resources = GetCOSObject()?.GetCOSDictionary(COSName.RESOURCES);
        if (resources is not null)
        {
            if (_resources is null || !ReferenceEquals(_resources.GetCOSObject(), resources))
            {
                _resources = new PDResources(resources, _resourceCache);
            }

            return _resources;
        }

        _resources = null;
        return GetCOSObject()?.ContainsKey(COSName.RESOURCES) == true ? new PDResources() : null;
    }

    public void SetResources(PDResources? resources)
    {
        _resources = null;
        GetCOSObject()?.SetItem(COSName.RESOURCES, resources?.GetCOSObject());
    }

    public PDRectangle? GetBBox()
    {
        COSArray? array = GetCOSObject()?.GetCOSArray(BboxName);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetBBox(PDRectangle? bbox)
    {
        if (bbox is null)
        {
            GetCOSObject()?.RemoveItem(BboxName);
        }
        else
        {
            GetCOSObject()?.SetItem(BboxName, bbox.GetCOSArray());
        }
    }

    public Matrix GetMatrix()
    {
        COSArray? m = GetCOSObject()?.GetCOSArray(MatrixName);
        if (m is null || m.Size() < 6)
        {
            return new Matrix();
        }

        float[] values = m.ToFloatArray();
        return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
    }

    public virtual void SetMatrix(Matrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        COSArray m = new();
        m.Add(new COSFloat(matrix.GetScaleX()));
        m.Add(new COSFloat(matrix.GetShearY()));
        m.Add(new COSFloat(matrix.GetShearX()));
        m.Add(new COSFloat(matrix.GetScaleY()));
        m.Add(new COSFloat(matrix.GetTranslateX()));
        m.Add(new COSFloat(matrix.GetTranslateY()));
        GetCOSObject()?.SetItem(MatrixName, m);
    }

    public void SetMatrix(AffineTransform at)
    {
        ArgumentNullException.ThrowIfNull(at);
        SetMatrix(new Matrix(at));
    }

    public int GetStructParents() => GetCOSObject()?.GetInt(COSName.STRUCT_PARENTS) ?? -1;

    public void SetStructParents(int structParent) => GetCOSObject()?.SetInt(COSName.STRUCT_PARENTS, structParent);

    public PDPropertyList? GetOptionalContent()
    {
        COSDictionary? optionalContent = GetCOSObject()?.GetCOSDictionary(OptionalContentName);
        return optionalContent is null ? null : PDPropertyList.Create(optionalContent);
    }

    public void SetOptionalContent(PDPropertyList? optionalContent)
    {
        GetCOSObject()?.SetItem(OptionalContentName, optionalContent);
    }
}
