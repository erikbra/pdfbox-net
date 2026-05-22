/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSIncrement.java
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

public class COSIncrement(COSUpdateInfo? incrementOrigin) : IEnumerable<COSBase>
{
    private readonly List<COSBase> _objects = [];
    private readonly HashSet<COSBase> _objectSet = [];
    private readonly HashSet<COSBase> _excluded = [];
    private readonly HashSet<COSObject> _processedObjects = [];
    private readonly COSUpdateInfo? _incrementOrigin = incrementOrigin;
    private bool _initialized;

    public bool Contains(COSBase? obj)
    {
        return obj is not null &&
               (_objectSet.Contains(obj) || (obj is COSObject cosObject && _processedObjects.Contains(cosObject)));
    }

    public COSIncrement Exclude(params COSBase?[]? values)
    {
        if (values is null)
        {
            return this;
        }

        foreach (COSBase? value in values)
        {
            if (value is not null)
            {
                _excluded.Add(value);
            }
        }

        return this;
    }

    public IReadOnlyCollection<COSBase> GetObjects()
    {
        if (!_initialized && _incrementOrigin is not null)
        {
            Collect(_incrementOrigin.GetCOSObject());
            _initialized = true;
        }

        return _objects.AsReadOnly();
    }

    public IEnumerator<COSBase> GetEnumerator()
    {
        return GetObjects().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private bool Collect(COSBase? obj)
    {
        if (obj is null || Contains(obj))
        {
            return false;
        }

        return obj switch
        {
            COSDictionary dictionary => Collect(dictionary),
            COSObject cosObject =>
                CollectObject(cosObject),
            COSArray array => Collect(array),
            _ => false
        };
    }

    private bool Collect(COSDictionary dictionary)
    {
        COSUpdateState updateState = dictionary.GetUpdateState();
        if (!IsExcluded(dictionary) && !Contains(dictionary) && updateState.IsUpdated())
        {
            Add(dictionary);
        }

        bool childDemandsParentUpdate = false;
        foreach (COSBase entry in dictionary.GetValues())
        {
            if (entry is not COSUpdateInfo updatableEntry || Contains(entry))
            {
                continue;
            }

            COSUpdateState entryUpdateState = updatableEntry.GetUpdateState();
            UpdateDifferentOrigin(entryUpdateState);
            if (updatableEntry.IsNeedToBeUpdated() &&
                ((entry is not COSObject && entry.IsDirect()) || entry is COSArray))
            {
                Exclude(entry);
                childDemandsParentUpdate = true;
            }

            childDemandsParentUpdate = Collect(entry) || childDemandsParentUpdate;
        }

        if (IsExcluded(dictionary))
        {
            return childDemandsParentUpdate;
        }

        if (childDemandsParentUpdate && !Contains(dictionary))
        {
            Add(dictionary);
        }

        return false;
    }

    private bool Collect(COSArray array)
    {
        COSUpdateState updateState = array.GetUpdateState();
        bool childDemandsParentUpdate = updateState.IsUpdated();
        foreach (COSBase? entry in array)
        {
            if (entry is not COSUpdateInfo updateInfo || Contains(entry))
            {
                continue;
            }

            COSUpdateState entryUpdateState = updateInfo.GetUpdateState();
            UpdateDifferentOrigin(entryUpdateState);
            childDemandsParentUpdate = Collect(entry) || childDemandsParentUpdate;
        }

        return childDemandsParentUpdate;
    }

    private bool CollectObject(COSObject obj)
    {
        if (Contains(obj))
        {
            return false;
        }

        AddProcessedObject(obj);
        COSUpdateState updateState = obj.GetUpdateState();
        UpdateDifferentOrigin(updateState);

        COSUpdateInfo? actual = null;
        if (updateState.IsUpdated() || obj.IsDereferenced())
        {
            actual = obj.GetObject() as COSUpdateInfo;
        }

        if (actual is null || Contains(actual.GetCOSObject()))
        {
            return false;
        }

        bool childDemandsParentUpdate = false;
        COSUpdateState actualUpdateState = actual.GetUpdateState();
        if (actualUpdateState.IsUpdated())
        {
            childDemandsParentUpdate = true;
        }

        Exclude(actual.GetCOSObject());
        childDemandsParentUpdate = Collect(actual.GetCOSObject()) || childDemandsParentUpdate;
        if (updateState.IsUpdated() || childDemandsParentUpdate)
        {
            Add(actual.GetCOSObject());
        }

        return false;
    }

    private void UpdateDifferentOrigin(COSUpdateState? updateState)
    {
        if (_incrementOrigin is not null &&
            updateState is not null &&
            !ReferenceEquals(_incrementOrigin.GetUpdateState().GetOriginDocumentState(), updateState.GetOriginDocumentState()))
        {
            updateState.Update();
        }
    }

    private void Add(COSBase? obj)
    {
        if (obj is not null && _objectSet.Add(obj))
        {
            _objects.Add(obj);
        }
    }

    private void AddProcessedObject(COSObject? obj)
    {
        if (obj is not null)
        {
            _processedObjects.Add(obj);
        }
    }

    private bool IsExcluded(COSBase? obj)
    {
        return obj is not null && _excluded.Contains(obj);
    }
}
