/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/IndexedTableModel.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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

namespace PdfBox.Net.Debugger.Colorpane;

/// <summary>
/// Table data model for an Indexed color space.
/// Adapted from Apache PDFBox IndexedTableModel (Khyrul Bashar).
/// Columns: Index (int), RGB value (string), Color (RGB tuple).
/// </summary>
public sealed class IndexedTableModel
{
    private static readonly string[] ColumnNames = ["Index", "RGB value", "Color"];

    private readonly IndexedColorant[] _data;

    public IndexedTableModel(IndexedColorant[] colorants)
        => _data = colorants;

    public int RowCount => _data.Length;

    public int ColumnCount => ColumnNames.Length;

    public string GetColumnName(int column) => ColumnNames[column];

    public object? GetValueAt(int row, int column) => column switch
    {
        0 => _data[row].Index,
        1 => _data[row].GetRGBValuesString(),
        2 => _data[row].GetColor(),
        _ => null
    };
}
