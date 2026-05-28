/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/PDFCloneUtility.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Utility class used to clone PDF objects. It keeps track of objects it has already cloned.
/// </summary>
public class PDFCloneUtility
{
    private readonly PDDocument _destination;
    private readonly Dictionary<COSBase, COSBase> _clonedVersion = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<COSBase> _clonedValues = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Creates a new instance for the given target document.
    /// </summary>
    /// <param name="destination">The destination PDF document that will receive the clones.</param>
    public PDFCloneUtility(PDDocument destination)
    {
        _destination = destination ?? throw new ArgumentNullException(nameof(destination));
    }

    /// <summary>
    /// Returns the destination PDF document this cloner instance is set up for.
    /// </summary>
    public PDDocument GetDestination()
    {
        return _destination;
    }

    /// <summary>
    /// Deep-clones the given object for inclusion into a different PDF document.
    /// </summary>
    public TCOSBase? CloneForNewDocument<TCOSBase>(TCOSBase? baseObject)
        where TCOSBase : COSBase
    {
        if (baseObject is null)
        {
            return null;
        }

        if (_clonedVersion.TryGetValue(baseObject, out COSBase? existing))
        {
            return (TCOSBase)existing;
        }

        if (_clonedValues.Contains(baseObject))
        {
            return baseObject;
        }

        COSBase clone = CloneCOSBaseForNewDocument(baseObject);
        _clonedVersion[baseObject] = clone;
        _clonedValues.Add(clone);
        return (TCOSBase)clone;
    }

    /// <summary>
    /// Merges two objects of the same type by deep-cloning source members into target.
    /// </summary>
    public void CloneMerge(COSObjectable? source, COSObjectable? target)
    {
        if (source is null || target is null || ReferenceEquals(source, target))
        {
            return;
        }

        CloneMergeCOSBase(source.GetCOSObject(), target.GetCOSObject());
    }

    private COSBase CloneCOSBaseForNewDocument(COSBase baseObject)
    {
        if (baseObject is COSObject cosObject)
        {
            return CloneForNewDocument(cosObject.GetObject()) ?? COSNull.NULL;
        }

        if (baseObject is COSArray array)
        {
            return CloneCOSArray(array);
        }

        if (baseObject is COSStream stream)
        {
            return CloneCOSStream(stream);
        }

        if (baseObject is COSDictionary dictionary)
        {
            return CloneCOSDictionary(dictionary);
        }

        return baseObject;
    }

    private COSArray CloneCOSArray(COSArray array)
    {
        COSArray newArray = new();
        for (int i = 0; i < array.Size(); i++)
        {
            COSBase? value = array.Get(i);
            if (HasSelfReference(array, value))
            {
                newArray.Add(newArray);
            }
            else
            {
                newArray.Add(value is null ? null : CloneForNewDocument(value));
            }
        }

        return newArray;
    }

    private COSStream CloneCOSStream(COSStream stream)
    {
        COSStream newStream = new();
        using (Stream output = newStream.CreateRawOutputStream())
        using (Stream input = stream.CreateRawInputStream())
        {
            input.CopyTo(output);
        }

        _clonedVersion[stream] = newStream;
        foreach (KeyValuePair<COSName, COSBase> entry in stream.EntrySet())
        {
            COSBase value = entry.Value;
            if (HasSelfReference(stream, value))
            {
                newStream.SetItem(entry.Key, newStream);
            }
            else
            {
                newStream.SetItem(entry.Key, CloneForNewDocument(value));
            }
        }

        return newStream;
    }

    private COSDictionary CloneCOSDictionary(COSDictionary dictionary)
    {
        COSDictionary newDictionary = new();
        _clonedVersion[dictionary] = newDictionary;

        foreach (KeyValuePair<COSName, COSBase> entry in dictionary.EntrySet())
        {
            COSBase value = entry.Value;
            if (HasSelfReference(dictionary, value))
            {
                newDictionary.SetItem(entry.Key, newDictionary);
            }
            else
            {
                newDictionary.SetItem(entry.Key, CloneForNewDocument(value));
            }
        }

        return newDictionary;
    }

    private void CloneMergeCOSBase(COSBase source, COSBase target)
    {
        COSBase sourceBase = source is COSObject sourceObject ? sourceObject.GetObject() ?? COSNull.NULL : source;
        COSBase targetBase = target is COSObject targetObject ? targetObject.GetObject() ?? COSNull.NULL : target;

        if (sourceBase is COSArray sourceArray && targetBase is COSArray targetArray)
        {
            for (int i = 0; i < sourceArray.Size(); i++)
            {
                COSBase? item = sourceArray.Get(i);
                targetArray.Add(item is null ? null : CloneForNewDocument(item));
            }
        }
        else if (sourceBase is COSDictionary sourceDictionary && targetBase is COSDictionary targetDictionary)
        {
            foreach (KeyValuePair<COSName, COSBase> entry in sourceDictionary.EntrySet())
            {
                COSName key = entry.Key;
                COSBase value = entry.Value;
                COSBase? existingValue = targetDictionary.GetItem(key);
                if (existingValue is not null)
                {
                    CloneMergeCOSBase(value, existingValue);
                }
                else
                {
                    targetDictionary.SetItem(key, CloneForNewDocument(value));
                }
            }
        }
    }

    private static bool HasSelfReference(COSBase parent, COSBase? value)
    {
        return value is COSObject objectValue && ReferenceEquals(objectValue.GetObject(), parent);
    }
}
