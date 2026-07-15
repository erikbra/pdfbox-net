/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/PDXObject.java
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics;

/// <summary>
/// A PDF XObject (external object) that can be referenced from a content stream via the "Do" operator.
/// The base class provides access to the underlying COS stream dictionary.
/// Concrete subclasses (image XObjects, form XObjects) extend this with type-specific behavior.
/// </summary>
public class PDXObject : COSObjectable
{
    private static readonly COSName SubtypeName = COSName.GetPDFName("Subtype");
    private static readonly COSName GroupName = COSName.GetPDFName("Group");
    private static readonly COSName SName = COSName.GetPDFName("S");
    private static readonly COSName TransparencyName = COSName.GetPDFName("Transparency");
    private static readonly COSName FormName = COSName.GetPDFName("Form");
    private static readonly COSName ImageName = COSName.GetPDFName("Image");
    private static readonly COSName PsName = COSName.GetPDFName("PS");

    private readonly PDStream? _stream;

    /// <summary>Creates a PDXObject with no backing stream (used as a placeholder reference).</summary>
    public PDXObject()
    {
    }

    /// <summary>Creates a PDXObject backed by the given COS stream.</summary>
    public PDXObject(COSStream stream)
        : this(new PDStream(stream))
    {
    }

    public PDXObject(PDStream stream)
    {
        _stream = stream;
    }

    public static PDXObject? CreateXObject(COSBase? @base, PDResources? resources)
    {
        if (@base is null)
        {
            return null;
        }

        if (@base is not COSStream stream)
        {
            throw new IOException($"Unexpected XObject base type: {@base.GetType().Name}");
        }

        string? subtype = stream.GetNameAsString(SubtypeName);
        if (string.Equals(subtype, ImageName.GetName(), StringComparison.Ordinal))
        {
            return new PDImageXObject(new PDStream(stream), resources);
        }

        if (string.Equals(subtype, FormName.GetName(), StringComparison.Ordinal))
        {
            ResourceCache? resourceCache = resources?.GetResourceCache();
            COSDictionary? group = stream.GetCOSDictionary(GroupName);
            if (group is not null && group.GetCOSName(SName)?.Equals(TransparencyName) == true)
            {
                return new PDTransparencyGroup(stream, resourceCache);
            }

            return new PDFormXObject(stream, resourceCache);
        }

        if (string.Equals(subtype, PsName.GetName(), StringComparison.Ordinal))
        {
            return new PDPostScriptXObject(stream);
        }

        return new PDXObject(stream);
    }

    /// <summary>Returns the underlying stream wrapper, or null if this object has no backing stream.</summary>
    public PDStream? GetStream() => _stream;

    /// <summary>Returns the underlying COS stream, or null if this object has no backing stream.</summary>
    public COSStream? GetCOSObject() => _stream?.GetCOSObject();

    COSBase COSObjectable.GetCOSObject()
    {
        COSBase? cosObject = _stream?.GetCOSObject();
        return cosObject ?? COSNull.NULL;
    }

    /// <summary>Returns the subtype name from the stream dictionary, or null.</summary>
    public string? GetSubtype() => _stream?.GetCOSObject().GetNameAsString(SubtypeName);

    protected void SetXObjectSubtype(string subtype)
    {
        COSStream? cos = _stream?.GetCOSObject();
        if (cos is null)
        {
            return;
        }

        cos.SetName(COSName.TYPE, "XObject");
        cos.SetName(SubtypeName, subtype);
    }
}
