/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/table/common/ScriptTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 */

/* Licensed to the Apache Software Foundation (ASF) under one or more contributor license agreements. See the NOTICE file distributed with this work for additional information regarding copyright ownership. The ASF licenses this file to You under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. */

namespace PdfBox.Net.FontBox.TTF.Table.Common;

public class ScriptTable(LangSysTable? defaultLangSysTable, IReadOnlyDictionary<string, LangSysTable> langSysTables)
{
    public LangSysTable? DefaultLangSysTable { get; } = defaultLangSysTable;
    public IReadOnlyDictionary<string, LangSysTable> LangSysTables { get; } = langSysTables;

    public LangSysTable? GetDefaultLangSysTable() => DefaultLangSysTable;
    public IReadOnlyDictionary<string, LangSysTable> GetLangSysTables() => LangSysTables;
    public override string ToString() => $"ScriptTable[hasDefault={DefaultLangSysTable != null},langSysRecordsCount={LangSysTables.Count}]";
}
