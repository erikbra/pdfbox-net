/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDObjectReference.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// An object reference dictionary.
/// </summary>
public class PDObjectReference : COSObjectable
{
    public const string TYPE = "OBJR";

    private static readonly COSName ObjName = COSName.GetPDFName("OBJ");
    private static readonly COSName PgName = COSName.GetPDFName("Pg");

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDObjectReference()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, TYPE);
    }

    /// <summary>
    /// Constructor for an existing object reference.
    /// </summary>
    public PDObjectReference(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    /// <summary>
    /// Returns the underlying dictionary.
    /// </summary>
    public COSDictionary GetCOSObject() => _dictionary;

    COSBase COSObjectable.GetCOSObject() => _dictionary;

    /// <summary>
    /// Gets the higher-level referenced object, if it can be resolved.
    /// </summary>
    public COSObjectable? GetReferencedObject()
    {
        COSDictionary? objDictionary = GetCOSObject().GetCOSDictionary(ObjName);
        if (objDictionary is null)
        {
            return null;
        }

        try
        {
            if (objDictionary is COSStream stream)
            {
                PDXObject? xobject = PDXObject.CreateXObject(stream, null);
                if (xobject is not null)
                {
                    return xobject;
                }
            }

            PDAnnotation annotation = PDAnnotation.CreateAnnotation(objDictionary);
            if (annotation is not PDAnnotationUnknown ||
                COSName.ANNOT.Equals(objDictionary.GetCOSName(COSName.TYPE)))
            {
                return annotation;
            }
        }
        catch (IOException)
        {
            // Keep compatibility with upstream behavior: unresolved target returns null.
        }

        return null;
    }

    /// <summary>
    /// Sets the referenced annotation.
    /// </summary>
    public void SetReferencedObject(PDAnnotation? annotation) => GetCOSObject().SetItem(ObjName, annotation);

    /// <summary>
    /// Sets the referenced XObject.
    /// </summary>
    public void SetReferencedObject(PDXObject? xobject) => GetCOSObject().SetItem(ObjName, xobject?.GetCOSObject());

    /// <summary>
    /// Gets the page.
    /// </summary>
    public PDPage? GetPage()
    {
        COSDictionary? page = GetCOSObject().GetCOSDictionary(PgName);
        return page is null ? null : new PDPage(page);
    }

    /// <summary>
    /// Sets the page.
    /// </summary>
    public void SetPage(PDPage? page) => GetCOSObject().SetItem(PgName, page);

}
