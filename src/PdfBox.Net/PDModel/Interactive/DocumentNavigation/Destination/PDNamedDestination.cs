/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDNamedDestination.java
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

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

/// <summary>
/// This represents a destination to a page by referencing it with a name.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDNamedDestination</c>.</remarks>
public class PDNamedDestination : PDDestination
{
    private COSBase? _namedDestination;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dest">The named destination.</param>
    public PDNamedDestination(COSString dest)
    {
        _namedDestination = dest;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dest">The named destination.</param>
    public PDNamedDestination(COSName dest)
    {
        _namedDestination = dest;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDNamedDestination()
    {
        //default, so do nothing
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dest">The named destination string.</param>
    public PDNamedDestination(string dest)
    {
        _namedDestination = new COSString(dest);
    }

    /// <summary>
    /// Convert this standard java object to a COS object.
    /// </summary>
    /// <returns>The cos object that matches this Java object.</returns>
    public override COSBase GetCOSObject()
    {
        return _namedDestination ?? COSNull.NULL;
    }

    /// <summary>
    /// This will get the name of the destination.
    /// </summary>
    /// <returns>The name of the destination.</returns>
    public string? GetNamedDestination()
    {
        if (_namedDestination is COSString cosString)
        {
            return cosString.GetString();
        }
        else if (_namedDestination is COSName cosName)
        {
            return cosName.GetName();
        }
        return null;
    }

    /// <summary>
    /// Set the named destination.
    /// </summary>
    /// <param name="dest">The new named destination.</param>
    public void SetNamedDestination(string? dest)
    {
        if (dest == null)
        {
            _namedDestination = null;
        }
        else
        {
            _namedDestination = new COSString(dest);
        }
    }
}
