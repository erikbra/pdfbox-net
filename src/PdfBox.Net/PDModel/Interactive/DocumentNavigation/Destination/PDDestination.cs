/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDDestination.java
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

/// <summary>
/// This represents a destination in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDDestination</c>.</remarks>
public abstract class PDDestination : PDDestinationOrAction
{
    /// <summary>
    /// Convert this standard java object to a COS object.
    /// </summary>
    public abstract COSBase GetCOSObject();

    /// <summary>
    /// This will create a new destination depending on the type of <see cref="COSBase"/> that is passed in.
    /// </summary>
    /// <param name="base">The base level object.</param>
    /// <returns>A new destination.</returns>
    /// <exception cref="IOException">If the base cannot be converted to a Destination.</exception>
    public static PDDestination? Create(COSBase? @base)
    {
        if (@base == null)
        {
            //this is ok, just return null.
            return null;
        }
        else if (@base is COSArray array
            && array.Size() > 1
            && array.GetObject(1) is COSName type)
        {
            string typeString = type.GetName();
            return typeString switch
            {
                PDPageFitDestination.Type or PDPageFitDestination.TypeBounded =>
                    new PDPageFitDestination(array),
                PDPageFitHeightDestination.Type or PDPageFitHeightDestination.TypeBounded =>
                    new PDPageFitHeightDestination(array),
                PDPageFitRectangleDestination.Type =>
                    new PDPageFitRectangleDestination(array),
                PDPageFitWidthDestination.Type or PDPageFitWidthDestination.TypeBounded =>
                    new PDPageFitWidthDestination(array),
                PDPageXYZDestination.Type =>
                    new PDPageXYZDestination(array),
                _ => throw new IOException("Unknown destination type: " + type.GetName())
            };
        }
        else if (@base is COSString cosString)
        {
            return new PDNamedDestination(cosString);
        }
        else if (@base is COSName cosName)
        {
            return new PDNamedDestination(cosName);
        }
        else
        {
            throw new IOException("Error: can't convert to Destination " + @base);
        }
    }
}
