/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/XrefEntries.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of the cross references of a PDF.</summary>
public class XrefEntries
{
    public static readonly string PATH = "CRT";

    private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<PdfBox.Net.COS.COSObjectKey, long>> _entries;
    private readonly PdfBox.Net.COS.COSDocument _document;

    public XrefEntries(PdfBox.Net.PDModel.PDDocument document)
    {
        System.Collections.Generic.Dictionary<PdfBox.Net.COS.COSObjectKey, long> xrefTable = document.GetDocument().GetXrefTable();
        _entries = xrefTable.OrderBy(static entry => entry.Key.GetNumber()).ToList();
        _document = document.GetDocument();
    }

    public int GetXrefEntryCount() => _entries.Count;

    public XrefEntry GetXrefEntry(int index)
    {
        System.Collections.Generic.KeyValuePair<PdfBox.Net.COS.COSObjectKey, long> entry = _entries[index];
        PdfBox.Net.COS.COSObject? objectFromPool = _document.GetObjectFromPool(entry.Key);
        return new XrefEntry(index, entry.Key, entry.Value, objectFromPool);
    }

    public int IndexOf(XrefEntry xrefEntry)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Key.Equals(xrefEntry.GetKey()))
            {
                return i;
            }
        }

        return 0;
    }

    public override string ToString() => PATH;
}
