/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionThread.java
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
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a thread action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionThread</c>.</remarks>
public class PDActionThread : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "Thread";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionThread()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionThread(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// Gets the D entry of the specific thread action dictionary.
    /// </summary>
    public COSBase? GetD()
    {
        return action.GetDictionaryObject(COSName.D);
    }

    /// <summary>
    /// Sets the destination.
    /// </summary>
    public void SetD(COSBase? d)
    {
        action.SetItem(COSName.D, d);
    }

    /// <summary>
    /// This will get the file in which the destination is located.
    /// </summary>
    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(action.GetDictionaryObject(COSName.F));
    }

    /// <summary>
    /// This will set the file in which the destination is located.
    /// </summary>
    public void SetFile(PDFileSpecification? fs)
    {
        action.SetItem(COSName.F, fs);
    }

    /// <summary>
    /// Gets the B entry of the specific thread action dictionary.
    /// </summary>
    public COSBase? GetB()
    {
        return action.GetDictionaryObject(COSName.B);
    }

    /// <summary>
    /// Sets the destination.
    /// </summary>
    public void SetB(COSBase? b)
    {
        action.SetItem(COSName.B, b);
    }
}

