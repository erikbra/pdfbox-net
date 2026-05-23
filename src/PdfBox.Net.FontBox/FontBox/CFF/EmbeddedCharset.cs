/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/EmbeddedCharset.java
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

public sealed class EmbeddedCharset : CFFCharset
{
    private readonly CFFCharset _charset;

    public EmbeddedCharset(bool isCidFont)
    {
        _charset = isCidFont ? new CFFCharsetCID() : new CFFCharsetType1();
    }

    public bool IsCIDFont() => _charset.IsCIDFont();
    public void AddSID(int gid, int sid, string name) => _charset.AddSID(gid, sid, name);
    public void AddCID(int gid, int cid) => _charset.AddCID(gid, cid);
    public int GetSIDForGID(int gid) => _charset.GetSIDForGID(gid);
    public int GetGIDForSID(int sid) => _charset.GetGIDForSID(sid);
    public int GetGIDForCID(int cid) => _charset.GetGIDForCID(cid);
    public int GetSID(string name) => _charset.GetSID(name);
    public string GetNameForGID(int gid) => _charset.GetNameForGID(gid);
    public int GetCIDForGID(int gid) => _charset.GetCIDForGID(gid);
}
