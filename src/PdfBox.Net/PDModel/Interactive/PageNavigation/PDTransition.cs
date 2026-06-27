/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDTransition.java
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

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

/// <summary>
/// Represents a page transition.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDTransition</c>.</remarks>
public sealed partial class PDTransition : PDDictionaryWrapper
{
    /// <summary>
    /// Creates a new transition with default replace style.
    /// </summary>
    public PDTransition()
        : this(PDTransitionStyle.R)
    {
    }

    /// <summary>
    /// Creates a new transition with the given style.
    /// </summary>
    /// <param name="style">The style to use.</param>
    public PDTransition(PDTransitionStyle style)
        : base()
    {
        GetCOSObject().SetName(COSName.TYPE, "Trans");
        GetCOSObject().SetName(COSName.S, style.ToString());
    }

    /// <summary>
    /// Creates a transition for an existing dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    public PDTransition(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Gets the transition style.
    /// </summary>
    public string GetStyle()
    {
        return GetCOSObject().GetNameAsString(COSName.S, PDTransitionStyle.R.ToString())!;
    }

    /// <summary>
    /// Gets the transition dimension.
    /// </summary>
    public string GetDimension()
    {
        return GetCOSObject().GetNameAsString(COSName.GetPDFName("DM"), PDTransitionDimension.H.ToString())!;
    }

    /// <summary>
    /// Sets the transition dimension.
    /// </summary>
    public void SetDimension(PDTransitionDimension dimension)
    {
        GetCOSObject().SetName(COSName.GetPDFName("DM"), dimension.ToString());
    }

    /// <summary>
    /// Gets the transition motion.
    /// </summary>
    public string GetMotion()
    {
        return GetCOSObject().GetNameAsString(COSName.M, PDTransitionMotion.I.ToString())!;
    }

    /// <summary>
    /// Sets the transition motion.
    /// </summary>
    public void SetMotion(PDTransitionMotion motion)
    {
        GetCOSObject().SetName(COSName.M, motion.ToString());
    }

    /// <summary>
    /// Gets the transition direction.
    /// </summary>
    public COSBase GetDirection()
    {
        return GetCOSObject().GetItem(COSName.GetPDFName("Di")) ?? COSInteger.ZERO;
    }

    /// <summary>
    /// Sets the transition direction.
    /// </summary>
    public void SetDirection(PDTransitionDirection direction)
    {
        GetCOSObject().SetItem(COSName.GetPDFName("Di"), direction.GetCOSBase());
    }

    /// <summary>
    /// Gets transition duration in seconds.
    /// </summary>
    public float GetDuration()
    {
        return GetCOSObject().GetFloat(COSName.D, 1);
    }

    /// <summary>
    /// Sets transition duration in seconds.
    /// </summary>
    public void SetDuration(float duration)
    {
        GetCOSObject().SetItem(COSName.D, new COSFloat(duration));
    }

    /// <summary>
    /// Gets fly transition scale.
    /// </summary>
    public float GetFlyScale()
    {
        return GetCOSObject().GetFloat(COSName.GetPDFName("SS"), 1);
    }

    /// <summary>
    /// Sets fly transition scale.
    /// </summary>
    public void SetFlyScale(float scale)
    {
        GetCOSObject().SetItem(COSName.GetPDFName("SS"), new COSFloat(scale));
    }

    /// <summary>
    /// Gets whether the fly area is opaque.
    /// </summary>
    public bool IsFlyAreaOpaque()
    {
        return GetCOSObject().GetBoolean(COSName.B, false);
    }

    /// <summary>
    /// Sets whether the fly area is opaque.
    /// </summary>
    public void SetFlyAreaOpaque(bool opaque)
    {
        GetCOSObject().SetItem(COSName.B, COSBoolean.GetBoolean(opaque));
    }
}
