/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CIDKeyedType2CharString.java
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

namespace PdfBox.Net.FontBox.CFF;

/// <summary>
/// A CID-Keyed Type 2 CharString.
/// </summary>
public class CIDKeyedType2CharString : Type2CharString
{
    /// <summary>
    /// Initializes a new CID-keyed Type 2 charstring.
    /// </summary>
    /// <param name="fontName">Font name.</param>
    /// <param name="cid">CID (character identifier).</param>
    /// <param name="bytes">Raw charstring bytes.</param>
    public CIDKeyedType2CharString(string fontName, int cid, byte[] bytes)
        : base(fontName, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:x4}", cid), bytes)
    {
        CID = cid;
    }

    /// <summary>
    /// Returns the CID (character identifier) of this charstring.
    /// </summary>
    public int CID { get; }
}
