/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDThread.java
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

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

/// <summary>
/// This a single thread in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDThread</c>.</remarks>
public partial class PDThread : COSObjectable
{
    private readonly COSDictionary _thread;

    /// <summary>
    /// Constructor that is used for a preexisting dictionary.
    /// </summary>
    /// <param name="thread">The underlying dictionary.</param>
    public PDThread(COSDictionary thread)
    {
        _thread = thread ?? throw new ArgumentNullException(nameof(thread));
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDThread()
    {
        _thread = new COSDictionary();
        _thread.SetItem(COSName.TYPE, COSName.GetPDFName("Thread"));
    }

    /// <summary>
    /// This will get the underlying dictionary that this object wraps.
    /// </summary>
    /// <returns>The underlying info dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _thread;
    }

    /// <summary>
    /// Get info about the thread, or null if there is nothing.
    /// </summary>
    /// <returns>The thread information.</returns>
    public PDDocumentInformation? GetThreadInfo()
    {
        COSDictionary? info = _thread.GetCOSDictionary(COSName.GetPDFName("I"));
        return info != null ? new PDDocumentInformation(info) : null;
    }

    /// <summary>
    /// Set the thread info, can be null.
    /// </summary>
    /// <param name="info">The info dictionary about this thread.</param>
    public void SetThreadInfo(PDDocumentInformation? info)
    {
        _thread.SetItem(COSName.GetPDFName("I"), info);
    }

    /// <summary>
    /// Get the first bead in the thread, or null if it has not been set yet.
    /// </summary>
    /// <returns>The first bead in the thread.</returns>
    public PDThreadBead? GetFirstBead()
    {
        COSDictionary? bead = _thread.GetCOSDictionary(COSName.F);
        return bead != null ? new PDThreadBead(bead) : null;
    }

    /// <summary>
    /// This will set the first bead in the thread.
    /// </summary>
    /// <param name="bead">The first bead in the thread.</param>
    public void SetFirstBead(PDThreadBead? bead)
    {
        bead?.SetThread(this);
        _thread.SetItem(COSName.F, bead);
    }
}
