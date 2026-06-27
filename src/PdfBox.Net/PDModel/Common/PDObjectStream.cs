/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDObjectStream.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

/// <summary>
/// Wrapper for object streams.
/// </summary>
public partial class PDObjectStream : PDStream
{
    private static readonly COSName ObjStmName = COSName.GetPDFName("ObjStm");
    private static readonly COSName ExtendsName = COSName.GetPDFName("Extends");

    public PDObjectStream(COSStream stream)
        : base(stream)
    {
    }

    public static PDObjectStream CreateStream(PdfBox.Net.PDModel.PDDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        PDObjectStream stream = new(new COSStream());
        stream.GetCOSObject().SetItem(COSName.TYPE, ObjStmName);
        return stream;
    }

    public string? GetTypeName()
    {
        return GetCOSObject().GetNameAsString(COSName.TYPE);
    }

    public int GetNumberOfObjects()
    {
        return GetCOSObject().GetInt(COSName.N, 0);
    }

    public void SetNumberOfObjects(int value)
    {
        GetCOSObject().SetInt(COSName.N, value);
    }

    public int GetFirstByteOffset()
    {
        return GetCOSObject().GetInt(COSName.FIRST, 0);
    }

    public void SetFirstByteOffset(int value)
    {
        GetCOSObject().SetInt(COSName.FIRST, value);
    }

    public PDObjectStream? GetExtends()
    {
        COSStream? stream = GetCOSObject().GetCOSStream(ExtendsName);
        return stream is null ? null : new PDObjectStream(stream);
    }

    public void SetExtends(PDObjectStream? stream)
    {
        GetCOSObject().SetItem(ExtendsName, stream);
    }
}
