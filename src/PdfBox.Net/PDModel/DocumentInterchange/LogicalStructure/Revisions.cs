/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/Revisions.java
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

using System.Text;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// Stores objects together with their revision numbers.
/// </summary>
/// <typeparam name="T">The object type.</typeparam>
public class Revisions<T>
{
    private readonly List<T> _objects = [];
    private readonly List<int> _revisionNumbers = [];

    /// <summary>
    /// Returns the object at the specified position.
    /// </summary>
    public T GetObject(int index) => _objects[index];

    /// <summary>
    /// Returns the revision number at the specified position.
    /// </summary>
    public int GetRevisionNumber(int index) => _revisionNumbers[index];

    /// <summary>
    /// Adds an object with a revision number.
    /// </summary>
    public void AddObject(T obj, int revisionNumber)
    {
        _objects.Add(obj);
        _revisionNumbers.Add(revisionNumber);
    }

    /// <summary>
    /// Sets the revision number for the specified object if present.
    /// </summary>
    public void SetRevisionNumber(T obj, int revisionNumber)
    {
        int index = _objects.IndexOf(obj);
        if (index > -1)
        {
            _revisionNumbers[index] = revisionNumber;
        }
    }

    /// <summary>
    /// Returns the number of entries.
    /// </summary>
    public int Size() => _objects.Count;

    /// <summary>
    /// Returns a text representation of all objects and revision numbers.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append('{');
        for (int i = 0; i < _objects.Count; i++)
        {
            if (i > 0)
            {
                sb.Append("; ");
            }

            sb.Append("object=").Append(_objects[i]).Append(", revisionNumber=").Append(GetRevisionNumber(i));
        }
        sb.Append('}');
        return sb.ToString();
    }
}
