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

public class COSUpdateState(COSUpdateInfo updateInfo)
{
    private readonly COSUpdateInfo _updateInfo = updateInfo;
    private COSDocumentState? _originDocumentState;
    private bool _updated;

    public void SetOriginDocumentState(COSDocumentState? originDocumentState)
    {
        SetOriginDocumentState(originDocumentState, false);
    }

    public COSDocumentState? GetOriginDocumentState()
    {
        return _originDocumentState;
    }

    public bool IsUpdated()
    {
        return _updated;
    }

    internal bool IsAcceptingUpdates()
    {
        return _originDocumentState is not null && _originDocumentState.IsAcceptingUpdates();
    }

    internal void Update()
    {
        Update(true);
    }

    internal void Update(bool updated)
    {
        if (IsAcceptingUpdates())
        {
            _updated = updated;
        }
    }

    internal void Update(COSBase? child)
    {
        Update();
        if (child is COSUpdateInfo updateChild)
        {
            updateChild.GetUpdateState().SetOriginDocumentState(_originDocumentState);
        }
    }

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

    internal void DereferenceChild(COSBase? child)
    {
        if (child is COSUpdateInfo updateChild)
        {
            updateChild.GetUpdateState().SetOriginDocumentState(_originDocumentState, true);
        }
    }

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
