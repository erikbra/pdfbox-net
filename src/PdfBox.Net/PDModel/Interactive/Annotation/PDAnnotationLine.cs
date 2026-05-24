/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLine.java
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

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationLine : PDAnnotationMarkup
{
    public const string SUB_TYPE = "Line";

    public PDAnnotationLine()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationLine(COSDictionary dict)
        : base(dict)
    {
    }

    public float[]? GetLine()
    {
        COSArray? lineArray = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("L"));
        if (lineArray == null)
        {
            return null;
        }

        float[] values = new float[lineArray.Size()];
        for (int i = 0; i < lineArray.Size(); i++)
        {
            values[i] = lineArray.GetObject(i) is COSNumber number ? number.FloatValue() : 0;
        }
        return values;
    }

    public void SetLine(float[]? line)
    {
        if (line == null)
        {
            GetCOSDictionary().RemoveItem(COSName.GetPDFName("L"));
            return;
        }

        COSArray lineArray = new();
        foreach (float value in line)
        {
            lineArray.Add(new COSFloat(value));
        }
        GetCOSDictionary().SetItem(COSName.GetPDFName("L"), lineArray);
    }
}
