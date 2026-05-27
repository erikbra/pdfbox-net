/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDThreadBead.java
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
/// This a single bead in a thread in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDThreadBead</c>.</remarks>
public class PDThreadBead : COSObjectable
{
    private readonly COSDictionary _bead;

    /// <summary>
    /// Constructor that is used for a preexisting dictionary.
    /// </summary>
    /// <param name="bead">The underlying dictionary.</param>
    public PDThreadBead(COSDictionary bead)
    {
        _bead = bead ?? throw new ArgumentNullException(nameof(bead));
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDThreadBead()
    {
        _bead = new COSDictionary();
        _bead.SetItem(COSName.TYPE, COSName.GetPDFName("Bead"));
        SetNextBead(this);
        SetPreviousBead(this);
    }

    /// <summary>
    /// This will get the underlying dictionary that this object wraps.
    /// </summary>
    /// <returns>The underlying info dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _bead;
    }

    /// <summary>
    /// This will get the thread that this bead is part of.
    /// </summary>
    /// <returns>The thread that this bead is part of.</returns>
    public PDThread? GetThread()
    {
        COSDictionary? dic = _bead.GetCOSDictionary(COSName.T);
        return dic != null ? new PDThread(dic) : null;
    }

    /// <summary>
    /// Set the thread that this bead is part of.
    /// </summary>
    /// <param name="thread">The thread that this bead is part of.</param>
    public void SetThread(PDThread? thread)
    {
        _bead.SetItem(COSName.T, thread);
    }

    /// <summary>
    /// This will get the next bead.
    /// </summary>
    /// <returns>The next bead in the list.</returns>
    public PDThreadBead? GetNextBead()
    {
        COSDictionary? next = _bead.GetCOSDictionary(COSName.N);
        return next != null ? new PDThreadBead(next) : null;
    }

    /// <summary>
    /// Set the next bead in the thread.
    /// </summary>
    /// <param name="next">The next bead.</param>
    protected internal void SetNextBead(PDThreadBead? next)
    {
        _bead.SetItem(COSName.N, next);
    }

    /// <summary>
    /// This will get the previous bead.
    /// </summary>
    /// <returns>The previous bead in the list.</returns>
    public PDThreadBead? GetPreviousBead()
    {
        COSDictionary? previous = _bead.GetCOSDictionary(COSName.V);
        return previous != null ? new PDThreadBead(previous) : null;
    }

    /// <summary>
    /// Set the previous bead in the thread.
    /// </summary>
    /// <param name="previous">The previous bead.</param>
    protected internal void SetPreviousBead(PDThreadBead? previous)
    {
        _bead.SetItem(COSName.V, previous);
    }

    /// <summary>
    /// Append a bead after this bead.
    /// </summary>
    /// <param name="append">The bead to insert.</param>
    public void AppendBead(PDThreadBead append)
    {
        ArgumentNullException.ThrowIfNull(append);
        PDThreadBead nextBead = GetNextBead() ?? this;
        nextBead.SetPreviousBead(append);
        append.SetNextBead(nextBead);
        SetNextBead(append);
        append.SetPreviousBead(this);
    }

    /// <summary>
    /// Get the page that this bead is part of.
    /// </summary>
    /// <returns>The page that this bead is part of.</returns>
    public PDPage? GetPage()
    {
        COSDictionary? dic = _bead.GetCOSDictionary(COSName.P);
        return dic != null ? new PDPage(dic) : null;
    }

    /// <summary>
    /// Set the page that this bead is part of.
    /// </summary>
    /// <param name="page">The page that this bead is on.</param>
    public void SetPage(PDPage? page)
    {
        _bead.SetItem(COSName.P, page);
    }

    /// <summary>
    /// The rectangle on the page that this bead is part of.
    /// </summary>
    /// <returns>The part of the page that this bead covers.</returns>
    public virtual PDRectangle? GetRectangle()
    {
        COSArray? array = _bead.GetCOSArray(COSName.GetPDFName("R"));
        return array != null ? new PDRectangle(array) : null;
    }

    /// <summary>
    /// Set the rectangle on the page that this bead covers.
    /// </summary>
    /// <param name="rect">The portion of the page that this bead covers.</param>
    public void SetRectangle(PDRectangle? rect)
    {
        _bead.SetItem(COSName.GetPDFName("R"), rect);
    }
}
