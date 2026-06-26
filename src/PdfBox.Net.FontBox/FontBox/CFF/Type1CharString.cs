/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type1CharString.java
 * PDFBOX_SOURCE_COMMIT: 5fff23e318cd4211b7ee6ba427f93282bfe09f4b
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5fff23e318cd4211b7ee6ba427f93282bfe09f4b
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.CFF;

public class Type1CharString
{
    private readonly GeneralPath _path = new();

    public Type1CharString(string fontName, string glyphName, byte[] bytes)
    {
        FontName = fontName;
        GlyphName = glyphName;
        Bytes = bytes;
    }

    public string FontName { get; }

    public string GlyphName { get; }

    public byte[] Bytes { get; }

    public string GetName() => GlyphName;

    public virtual GeneralPath GetPath() => _path;

    public virtual float GetWidth() => 0;
}
