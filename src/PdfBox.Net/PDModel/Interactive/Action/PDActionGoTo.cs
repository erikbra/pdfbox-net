/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionGoTo.java
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
/// This represents a go-to action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionGoTo</c>.</remarks>
public partial class PDActionGoTo : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "GoTo";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionGoTo()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionGoTo(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the destination to jump to.
    /// </summary>
    /// <returns>The D entry of the specific go-to action dictionary.</returns>
    public PDDestination? GetDestination()
    {
        return PDDestination.Create(GetCOSObject().GetDictionaryObject(COSName.D));
    }

    /// <summary>
    /// This will set the destination to jump to.
    /// </summary>
    /// <param name="d">The destination.</param>
    /// <exception cref="ArgumentException">If the destination is not a page dictionary object.</exception>
    public void SetDestination(PDDestination? d)
    {
        if (d is PDPageDestination pageDest)
        {
            COSArray destArray = (COSArray)pageDest.GetCOSObject();
            if (!destArray.IsEmpty())
            {
                COSBase? page = destArray.GetObject(0);
                if (page is not COSDictionary)
                {
                    throw new ArgumentException(
                        "Destination of a GoTo action must be a page dictionary object");
                }
            }
        }
        GetCOSObject().SetItem(COSName.D, d);
    }
}
