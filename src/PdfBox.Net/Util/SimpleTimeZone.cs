/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: (helper class — no direct Java upstream equivalent)
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

namespace PdfBox.Net.Util;

/// <summary>
/// A lightweight C# equivalent of Java's <c>SimpleTimeZone</c> used internally by
/// <see cref="DateConverter"/> and <see cref="GregorianCalendar"/>. Stores only a raw UTC
/// offset in milliseconds with no daylight saving time rules.
/// </summary>
public class SimpleTimeZone
{
    private int _rawOffsetMillis;
    private string _id;

    /// <summary>
    /// Constructs a SimpleTimeZone with the given UTC offset in milliseconds and ID.
    /// </summary>
    public SimpleTimeZone(int rawOffsetMillis, string id)
    {
        _rawOffsetMillis = rawOffsetMillis;
        _id = id;
    }

    /// <summary>
    /// Returns the raw UTC offset in milliseconds (no DST adjustment).
    /// </summary>
    public int GetRawOffset() => _rawOffsetMillis;

    /// <summary>
    /// Sets the raw UTC offset in milliseconds.
    /// </summary>
    public void SetRawOffset(int rawOffsetMillis)
    {
        _rawOffsetMillis = rawOffsetMillis;
    }

    /// <summary>
    /// Returns the timezone ID string (e.g. "GMT", "GMT+01:00", "EST").
    /// </summary>
    public string GetId() => _id;

    /// <summary>
    /// Sets the timezone ID string.
    /// </summary>
    public void SetId(string id)
    {
        _id = id;
    }

    /// <summary>
    /// Returns the DST offset — always 0 because SimpleTimeZone has no DST rules.
    /// </summary>
    public int GetDstOffset() => 0;
}
