/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/CMapManager.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using System.Collections.Concurrent;
using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.IO;

namespace PdfBox.Net.PDModel.Font;

public static class CMapManager
{
    private static readonly ConcurrentDictionary<string, CMap> Cache = new(StringComparer.Ordinal);

    public static CMap GetPredefinedCMap(string cMapName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cMapName);
        return Cache.GetOrAdd(cMapName, static name => new CMapParser().ParsePredefined(name));
    }

    public static CMap? ParseCMap(RandomAccessRead? randomAccessRead)
    {
        return randomAccessRead == null ? null : new CMapParser().Parse(randomAccessRead);
    }
}
