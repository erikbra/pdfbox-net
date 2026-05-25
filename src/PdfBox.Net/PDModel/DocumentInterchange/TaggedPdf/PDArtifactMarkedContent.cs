/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDArtifactMarkedContent.java
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;

/// <summary>
/// Artifact marked-content sequence specialization.
/// </summary>
public class PDArtifactMarkedContent : PDMarkedContent
{
    public PDArtifactMarkedContent(COSDictionary properties)
        : base(COSName.GetPDFName("Artifact"), properties)
    {
    }

    public string? GetArtifactType() => Properties?.GetNameAsString(COSName.TYPE);

    public PDRectangle? GetBBox()
    {
        COSArray? array = Properties?.GetCOSArray(COSName.GetPDFName("BBox"));
        return array is null ? null : new PDRectangle(array);
    }

    public bool IsTopAttached() => IsAttached("Top");
    public bool IsBottomAttached() => IsAttached("Bottom");
    public bool IsLeftAttached() => IsAttached("Left");
    public bool IsRightAttached() => IsAttached("Right");

    public string? GetSubtype() => Properties?.GetNameAsString(COSName.SUBTYPE);

    private bool IsAttached(string edge)
    {
        COSArray? array = Properties?.GetCOSArray(COSName.GetPDFName("Attached"));
        if (array is null)
        {
            return false;
        }

        for (int i = 0; i < array.Size(); i++)
        {
            if (string.Equals(edge, array.GetName(i), StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
