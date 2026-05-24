/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFCharsetCID.java
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
/// A CFF charset for CID-keyed fonts. A charset is an array of CIDs for all glyphs in the font.
/// </summary>
public class CFFCharsetCID : CFFCharset
{
    private const string ExceptionMessage = "Not a Type 1-equivalent font";

    private readonly Dictionary<int, int> _sidOrCidToGid = new(250);
    private readonly Dictionary<int, int> _gidToCid = [];

    public bool IsCIDFont() => true;

    public void AddSID(int gid, int sid, string name) =>
        throw new InvalidOperationException(ExceptionMessage);

    public void AddCID(int gid, int cid)
    {
        _sidOrCidToGid[cid] = gid;
        _gidToCid[gid] = cid;
    }

    public int GetSIDForGID(int gid) =>
        throw new InvalidOperationException(ExceptionMessage);

    public int GetGIDForSID(int sid) =>
        throw new InvalidOperationException(ExceptionMessage);

    public int GetGIDForCID(int cid) =>
        _sidOrCidToGid.TryGetValue(cid, out int gid) ? gid : 0;

    public int GetSID(string name) =>
        throw new InvalidOperationException(ExceptionMessage);

    public string GetNameForGID(int gid) =>
        throw new InvalidOperationException(ExceptionMessage);

    public int GetCIDForGID(int gid) =>
        _gidToCid.TryGetValue(gid, out int cid) ? cid : 0;
}
