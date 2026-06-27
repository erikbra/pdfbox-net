/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionNamed.java
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

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a named action in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionNamed</c>.</remarks>
public partial class PDActionNamed : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "Named";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionNamed()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionNamed(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the name of the action to be performed.
    /// </summary>
    /// <returns>The name of the action to be performed.</returns>
    public string? GetN()
    {
        return action.GetNameAsString("N");
    }

    /// <summary>
    /// This will set the name of the action to be performed.
    /// </summary>
    /// <param name="name">The name of the action to be performed.</param>
    public void SetN(string name)
    {
        action.SetName("N", name);
    }
}
