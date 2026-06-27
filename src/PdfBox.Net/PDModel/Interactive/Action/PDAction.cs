/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDAction.java
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
using System.Linq;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents an action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAction</c>.</remarks>
public abstract partial class PDAction : PDDestinationOrAction
{
    /// <summary>
    /// The type of PDF object.
    /// </summary>
    public const string TYPE = "Action";

    /// <summary>
    /// The action dictionary.
    /// </summary>
    protected readonly COSDictionary action;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected PDAction()
    {
        action = new COSDictionary();
        SetType(TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    protected PDAction(COSDictionary a)
    {
        action = a;
    }

    /// <summary>
    /// Convert this standard java object to a COS object.
    /// </summary>
    /// <returns>The cos object that matches this Java object.</returns>
    public COSDictionary GetCOSObject()
    {
        return action;
    }

    COSBase COSObjectable.GetCOSObject() => action;

    /// <summary>
    /// This will get the type of PDF object that the actions dictionary describes.
    /// If present must be Action for an action dictionary.
    /// </summary>
    /// <returns>The Type of PDF object.</returns>
    public string? GetActionType()
    {
        return action.GetNameAsString(COSName.TYPE);
    }

    /// <summary>
    /// This will set the type of PDF object that the actions dictionary describes.
    /// If present must be Action for an action dictionary.
    /// </summary>
    /// <param name="type">The new Type for the PDF object.</param>
    protected void SetType(string type)
    {
        action.SetName(COSName.TYPE, type);
    }

    /// <summary>
    /// This will get the type of action that the actions dictionary describes.
    /// </summary>
    /// <returns>The S entry of actions dictionary.</returns>
    public string? GetSubType()
    {
        return action.GetNameAsString(COSName.S);
    }

    /// <summary>
    /// This will set the type of action that the actions dictionary describes.
    /// </summary>
    /// <param name="s">The new type of action.</param>
    protected void SetSubType(string s)
    {
        action.SetName(COSName.S, s);
    }

    /// <summary>
    /// This will get the next action, or sequence of actions, to be performed after this one.
    /// The value is either a single action dictionary or an array of action dictionaries
    /// to be performed in order.
    /// </summary>
    /// <returns>The Next action or sequence of actions.</returns>
    public IList<PDAction>? GetNext()
    {
        IList<PDAction>? retval = null;
        COSBase? next = action.GetDictionaryObject(COSName.NEXT);
        if (next is COSDictionary nextDict)
        {
            PDAction? pdAction = PDActionFactory.CreateAction(nextDict);
            COSArray singleArray = new COSArray();
            if (pdAction != null) singleArray.Add(pdAction);
            retval = new COSArrayList<PDAction>(pdAction != null ? [pdAction] : [], singleArray);
        }
        else if (next is COSArray array)
        {
            List<PDAction> actions = new(array.Size());
            for (int i = 0; i < array.Size(); i++)
            {
                PDAction? a = PDActionFactory.CreateAction(array.GetObject(i) as COSDictionary);
                if (a != null)
                {
                    actions.Add(a);
                }
            }
            retval = new COSArrayList<PDAction>(actions, array);
        }
        return retval;
    }

    /// <summary>
    /// This will set the next action, or sequence of actions, to be performed after this one.
    /// </summary>
    /// <param name="next">The Next action or sequence of actions.</param>
    public void SetNext(IList<PDAction> next)
    {
        action.SetItem(COSName.NEXT, new COSArray(next.Cast<COSObjectable?>()));
    }
}
