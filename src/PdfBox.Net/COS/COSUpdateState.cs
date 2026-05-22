/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSUpdateState.java
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

namespace PdfBox.Net.COS;

/// <summary>
/// A <see cref="COSUpdateState"/> instance manages update states for a <see cref="COSUpdateInfo"/>.
/// Such states are used to create a <see cref="COSIncrement"/> for the incremental saving of a <see cref="COSDocument"/>.
/// </summary>
/// <remarks>Original author: Christian Appl.</remarks>
public class COSUpdateState(COSUpdateInfo updateInfo)
{
    private readonly COSUpdateInfo _updateInfo = updateInfo;
    private COSDocumentState? _originDocumentState;
    private bool _updated;

    /// <summary>
    /// Links the given <see cref="COSDocumentState"/> to this update state and propagates it to contained substructures.
    /// </summary>
    /// <param name="originDocumentState">The document state that shall be linked to this update state.</param>
    public void SetOriginDocumentState(COSDocumentState? originDocumentState)
    {
        SetOriginDocumentState(originDocumentState, false);
    }

    /// <summary>
    /// Returns the linked origin document state.
    /// </summary>
    /// <returns>The linked document state, if any.</returns>
    public COSDocumentState? GetOriginDocumentState()
    {
        return _originDocumentState;
    }

    /// <summary>
    /// Returns the current update state of the managed <see cref="COSUpdateInfo"/>.
    /// </summary>
    /// <returns><c>true</c> if updated; otherwise <c>false</c>.</returns>
    public bool IsUpdated()
    {
        return _updated;
    }

    /// <summary>
    /// Returns whether updates are currently accepted.
    /// </summary>
    /// <returns><c>true</c> when updates are accepted; otherwise <c>false</c>.</returns>
    internal bool IsAcceptingUpdates()
    {
        return _originDocumentState is not null && _originDocumentState.IsAcceptingUpdates();
    }

    /// <summary>
    /// Updates the state to <c>true</c> when updates are accepted.
    /// </summary>
    internal void Update()
    {
        Update(true);
    }

    /// <summary>
    /// Updates the state to the given value when updates are accepted.
    /// </summary>
    /// <param name="updated">The new update state.</param>
    internal void Update(bool updated)
    {
        if (IsAcceptingUpdates())
        {
            _updated = updated;
        }
    }

    /// <summary>
    /// Updates this state and propagates origin document state to a child update info object.
    /// </summary>
    /// <param name="child">The child to update.</param>
    internal void Update(COSBase? child)
    {
        Update();
        if (child is COSUpdateInfo updateChild)
        {
            updateChild.GetUpdateState().SetOriginDocumentState(_originDocumentState);
        }
    }

    /// <summary>
    /// Updates this state and propagates origin document state to child update info objects.
    /// </summary>
    /// <param name="children">The children to update.</param>
    internal void Update(IEnumerable<COSBase?>? children)
    {
        Update();
        if (children is null)
        {
            return;
        }

        foreach (COSBase? child in children)
        {
            if (child is COSUpdateInfo updateChild)
            {
                updateChild.GetUpdateState().SetOriginDocumentState(_originDocumentState);
            }
        }
    }

    /// <summary>
    /// Propagates origin document state to a dereferenced child without changing update-state flags.
    /// </summary>
    /// <param name="child">The dereferenced child.</param>
    internal void DereferenceChild(COSBase? child)
    {
        if (child is COSUpdateInfo updateChild)
        {
            updateChild.GetUpdateState().SetOriginDocumentState(_originDocumentState, true);
        }
    }

    /// <summary>
    /// Uses the managed update info as the base object of a new <see cref="COSIncrement"/>.
    /// </summary>
    /// <returns>A <see cref="COSIncrement"/> based on the managed update info.</returns>
    internal COSIncrement ToIncrement()
    {
        return new COSIncrement(_updateInfo);
    }

    private void SetOriginDocumentState(COSDocumentState? originDocumentState, bool dereferencing)
    {
        if (_originDocumentState is not null || originDocumentState is null)
        {
            return;
        }

        _originDocumentState = originDocumentState;
        if (!dereferencing)
        {
            Update();
        }

        switch (_updateInfo)
        {
            case COSDictionary dictionary:
                foreach (COSBase entry in dictionary.GetValues())
                {
                    if (entry is COSUpdateInfo updateEntry)
                    {
                        updateEntry.GetUpdateState().SetOriginDocumentState(originDocumentState, dereferencing);
                    }
                }

                break;
            case COSArray array:
                foreach (COSBase? entry in array)
                {
                    if (entry is COSUpdateInfo updateEntry)
                    {
                        updateEntry.GetUpdateState().SetOriginDocumentState(originDocumentState, dereferencing);
                    }
                }

                break;
            case COSObject obj when obj.IsDereferenced():
                if (obj.GetObject() is COSUpdateInfo updateReference)
                {
                    updateReference.GetUpdateState().SetOriginDocumentState(originDocumentState, dereferencing);
                }

                break;
        }
    }
}
