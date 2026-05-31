/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/MapEntry.java
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

/// <summary>Represents an abstract view of a dictionary item in the tree view.</summary>
public class MapEntry
{
    public PdfBox.Net.COS.COSName? Key { get; set; }

    public PdfBox.Net.COS.COSBase? Value { get; set; }

    public PdfBox.Net.COS.COSBase? Item { get; set; }

    public PdfBox.Net.COS.COSName? GetKey() => Key;

    public void SetKey(PdfBox.Net.COS.COSName? key) => Key = key;

    public PdfBox.Net.COS.COSBase? GetValue() => Value;

    public void SetValue(PdfBox.Net.COS.COSBase? value) => Value = value;

    public PdfBox.Net.COS.COSBase? GetItem() => Item;

    public void SetItem(PdfBox.Net.COS.COSBase? item) => Item = item;

    public override string ToString() => Key?.GetName() ?? "(null)";
}
