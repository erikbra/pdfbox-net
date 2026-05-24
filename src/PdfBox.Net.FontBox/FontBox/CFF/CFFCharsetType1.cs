/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFCharsetType1.java
 * PDFBOX_SOURCE_COMMIT: 60c27ff0e94dea7f4471630733a7ac2381901e62
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 60c27ff0e94dea7f4471630733a7ac2381901e62
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

public class CFFCharsetType1 : CFFCharset
{
    private readonly Dictionary<int, int> _gidToSid = [];
    private readonly Dictionary<int, int> _sidToGid = [];
    private readonly Dictionary<int, string> _gidToName = [];
    private readonly Dictionary<string, int> _nameToSid = new(StringComparer.Ordinal);

    public bool IsCIDFont() => false;

    public void AddSID(int gid, int sid, string name)
    {
        _gidToSid[gid] = sid;
        _sidToGid[sid] = gid;
        _gidToName[gid] = name;
        _nameToSid[name] = sid;
    }

    public void AddCID(int gid, int cid)
    {
        AddSID(gid, cid, CFFStandardString.GetName(cid));
    }

    public int GetSIDForGID(int gid) => _gidToSid.TryGetValue(gid, out int sid) ? sid : 0;
    public int GetGIDForSID(int sid) => _sidToGid.TryGetValue(sid, out int gid) ? gid : 0;
    public int GetGIDForCID(int cid) => GetGIDForSID(cid);
    public int GetSID(string name) => _nameToSid.TryGetValue(name, out int sid) ? sid : 0;
    public string GetNameForGID(int gid) => _gidToName.TryGetValue(gid, out string? name) ? name : ".notdef";
    public int GetCIDForGID(int gid) => GetSIDForGID(gid);
}
