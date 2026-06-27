/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDTargetDirectory.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// A target dictionary specifying path information to the target document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDTargetDirectory</c>.</remarks>
public partial class PDTargetDirectory : COSObjectable
{
    private static readonly COSName RName = COSName.GetPDFName("R");
    private readonly COSDictionary _dict;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDTargetDirectory()
    {
        _dict = new COSDictionary();
    }

    /// <summary>
    /// Constructor from an existing dictionary.
    /// </summary>
    public PDTargetDirectory(COSDictionary dictionary)
    {
        _dict = dictionary;
    }

    public COSBase GetCOSObject()
    {
        return _dict;
    }

    /// <summary>
    /// Gets the relationship between the current document and the target.
    /// </summary>
    public COSName? GetRelationship()
    {
        return _dict.GetCOSName(RName);
    }

    /// <summary>
    /// Sets the relationship between the current document and the target.
    /// </summary>
    public void SetRelationship(COSName relationship)
    {
        if (!COSName.P.Equals(relationship) && !COSName.C.Equals(relationship))
        {
            throw new ArgumentException("The only valid values are P or C, not " + relationship.GetName());
        }
        _dict.SetItem(RName, relationship);
    }

    /// <summary>
    /// Gets the filename in the EmbeddedFiles name tree.
    /// </summary>
    public string? GetFilename()
    {
        return _dict.GetString(COSName.N);
    }

    /// <summary>
    /// Sets the filename in the EmbeddedFiles name tree.
    /// </summary>
    public void SetFilename(string? filename)
    {
        _dict.SetString(COSName.N, filename);
    }

    /// <summary>
    /// Gets the nested target directory.
    /// </summary>
    public PDTargetDirectory? GetTargetDirectory()
    {
        COSDictionary? targetDict = _dict.GetCOSDictionary(COSName.T);
        return targetDict != null ? new PDTargetDirectory(targetDict) : null;
    }

    /// <summary>
    /// Sets the nested target directory.
    /// </summary>
    public void SetTargetDirectory(PDTargetDirectory? targetDirectory)
    {
        _dict.SetItem(COSName.T, targetDirectory);
    }

    /// <summary>
    /// Gets the zero-based page number from /P when it is numeric.
    /// </summary>
    public int GetPageNumber()
    {
        return _dict.GetInt(COSName.P, -1);
    }

    /// <summary>
    /// Sets the zero-based page number in /P.
    /// </summary>
    public void SetPageNumber(int pageNumber)
    {
        if (pageNumber < 0)
        {
            _dict.RemoveItem(COSName.P);
        }
        else
        {
            _dict.SetInt(COSName.P, pageNumber);
        }
    }

    /// <summary>
    /// Gets a named destination from /P when it is a string.
    /// </summary>
    public PDNamedDestination? GetNamedDestination()
    {
        COSBase? @base = _dict.GetDictionaryObject(COSName.P);
        if (@base is COSString cosString)
        {
            return new PDNamedDestination(cosString);
        }
        return null;
    }

    /// <summary>
    /// Sets a named destination in /P.
    /// </summary>
    public void SetNamedDestination(PDNamedDestination? dest)
    {
        if (dest == null)
        {
            _dict.RemoveItem(COSName.P);
        }
        else
        {
            _dict.SetItem(COSName.P, dest);
        }
    }

    /// <summary>
    /// Gets the zero-based annotation index from /A when it is numeric.
    /// </summary>
    public int GetAnnotationIndex()
    {
        return _dict.GetInt(COSName.A, -1);
    }

    /// <summary>
    /// Sets the zero-based annotation index in /A.
    /// </summary>
    public void SetAnnotationIndex(int index)
    {
        if (index < 0)
        {
            _dict.RemoveItem(COSName.A);
        }
        else
        {
            _dict.SetInt(COSName.A, index);
        }
    }

    /// <summary>
    /// Gets the annotation name from /A when it is a string.
    /// </summary>
    public string? GetAnnotationName()
    {
        return _dict.GetString(COSName.A);
    }

    /// <summary>
    /// Sets the annotation name in /A.
    /// </summary>
    public void SetAnnotationName(string? name)
    {
        _dict.SetString(COSName.A, name);
    }
}
