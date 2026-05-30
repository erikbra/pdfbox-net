/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDMetadata.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

public class PDMetadata : PDStream
{
    private static readonly COSName SubtypeName = COSName.GetPDFName("Subtype");

    public PDMetadata(PDDocument document)
        : base(document)
    {
        GetCOSObject().SetName(COSName.TYPE, "Metadata");
        GetCOSObject().SetName(SubtypeName, "XML");
    }

    public PDMetadata(PDDocument document, Stream stream)
        : base(document, stream)
    {
        GetCOSObject().SetName(COSName.TYPE, "Metadata");
        GetCOSObject().SetName(SubtypeName, "XML");
    }

    public PDMetadata(COSStream stream)
        : base(stream)
    {
    }

    public Stream ExportXMPMetadata()
    {
        return CreateInputStream();
    }

    public void ImportXMPMetadata(byte[] xmp)
    {
        ArgumentNullException.ThrowIfNull(xmp);
        using Stream output = CreateOutputStream();
        output.Write(xmp, 0, xmp.Length);
    }
}
