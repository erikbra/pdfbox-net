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

/// <summary>
/// A <see cref="COSIncrement"/> starts at a given <see cref="COSUpdateInfo"/> to collect updates, that have been
/// made to a <see cref="COSDocument"/> and therefore should be added to its next increment.
/// </summary>
public class COSIncrement(COSUpdateInfo? incrementOrigin) : IEnumerable<COSBase>
{
    /// <summary>
    /// Contains the <see cref="COSBase"/> instances, that shall be added to the increment at top level.
    /// </summary>
    private readonly List<COSBase> _objects = [];
    /// <summary>
    /// Fast lookup set for the objects already added to <see cref="_objects"/>.
    /// </summary>
    private readonly HashSet<COSBase> _objectSet = [];
    /// <summary>
    /// Contains the direct <see cref="COSBase"/> instances, that are either written directly by structures contained in
    /// the increment or that must be excluded from being written as indirect <see cref="COSObject"/> instances.
    /// </summary>
    private readonly HashSet<COSBase> _excluded = [];
    /// <summary>
    /// Contains all <see cref="COSObject"/> instances, that have already been processed.
    /// </summary>
    private readonly HashSet<COSObject> _processedObjects = [];
    /// <summary>
    /// Contains the <see cref="COSUpdateInfo"/> that this <see cref="COSIncrement"/> creates an increment for.
    /// </summary>
    private readonly COSUpdateInfo? _incrementOrigin = incrementOrigin;
    /// <summary>
    /// Whether this <see cref="COSIncrement"/> has already been determined.
    /// </summary>
    private bool _initialized;

    /// <summary>
    /// Returns <c>true</c> if the given <see cref="COSBase"/> is already known to and has been processed by this
    /// <see cref="COSIncrement"/>.
    /// </summary>
    /// <param name="obj">The <see cref="COSBase"/> to check.</param>
    /// <returns>
    /// <c>true</c> if the given <see cref="COSBase"/> is already known to and has been processed by this increment.
    /// </returns>
    public bool Contains(COSBase? obj)
    {
        return obj is not null &&
               (_objectSet.Contains(obj) || (obj is COSObject cosObject && _processedObjects.Contains(cosObject)));
    }

    /// <summary>
    /// The given <see cref="COSBase"/> instances are not fit for inclusion in an increment and shall be added to
    /// the excluded set.
    /// </summary>
    /// <param name="values">The values to exclude.</param>
    /// <returns>The <see cref="COSIncrement"/> itself, to allow method chaining.</returns>
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

    /// <summary>
    /// Returns all indirect <see cref="COSBase"/> instances that shall be written to an increment as top-level
    /// <see cref="COSObject"/> values. Calling this method initializes the increment.
    /// </summary>
    /// <returns>All indirect <see cref="COSBase"/> instances to include in the increment.</returns>
    public IReadOnlyCollection<COSBase> GetObjects()
    {
        if (!_initialized && _incrementOrigin is not null)
        {
            Collect(_incrementOrigin.GetCOSObject());
            _initialized = true;
        }

        return _objects.AsReadOnly();
    }

    /// <summary>
    /// Return an iterator for the determined objects contained in this <see cref="COSIncrement"/>.
    /// </summary>
    /// <returns>An iterator for this increment's collected objects.</returns>
    public IEnumerator<COSBase> GetEnumerator()
    {
        return GetObjects().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Collect all updates made to the given <see cref="COSBase"/> and its contained structures.
    /// </summary>
    /// <param name="obj">The object updates shall be collected for.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="COSBase"/> represents a direct child structure that would require its parent to
    /// be updated instead.
    /// </returns>
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

    /// <summary>
    /// Collect all updates made to the given <see cref="COSDictionary"/> and its contained structures.
    /// </summary>
    /// <param name="dictionary">The dictionary updates shall be collected for.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="COSDictionary"/> represents a direct child structure that would require its
    /// parent to be updated instead.
    /// </returns>
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

    /// <summary>
    /// Collect all updates made to the given <see cref="COSArray"/> and its contained structures.
    /// </summary>
    /// <param name="array">The array updates shall be collected for.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="COSArray"/>'s elements changed and therefore require its parent to be updated.
    /// </returns>
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

    /// <summary>
    /// Collect all updates made to the given <see cref="COSObject"/> and its contained structures.
    /// </summary>
    /// <param name="obj">The object updates shall be collected for.</param>
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

    /// <summary>
    /// Check whether the given update state's document origin differs from this increment's origin, and mark it as
    /// updated when it does.
    /// </summary>
    /// <param name="updateState">The update state to check and potentially update.</param>
    private void UpdateDifferentOrigin(COSUpdateState? updateState)
    {
        if (_incrementOrigin is not null &&
            updateState is not null &&
            !ReferenceEquals(_incrementOrigin.GetUpdateState().GetOriginDocumentState(), updateState.GetOriginDocumentState()))
        {
            updateState.Update();
        }
    }

    /// <summary>
    /// Add an object to the increment object collection, if possible.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    private void Add(COSBase? obj)
    {
        if (obj is not null && _objectSet.Add(obj))
        {
            _objects.Add(obj);
        }
    }

    /// <summary>
    /// Mark the given object as processed.
    /// </summary>
    /// <param name="obj">The object to mark as processed.</param>
    private void AddProcessedObject(COSObject? obj)
    {
        if (obj is not null)
        {
            _processedObjects.Add(obj);
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the given object has been excluded from the increment.
    /// </summary>
    /// <param name="obj">The object to check for exclusion.</param>
    /// <returns><c>true</c> if the object has been excluded from the increment.</returns>
    private bool IsExcluded(COSBase? obj)
    {
        return obj is not null && _excluded.Contains(obj);
    }
}
