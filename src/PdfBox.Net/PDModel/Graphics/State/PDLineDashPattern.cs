/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/PDLineDashPattern.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics.State;

public sealed class PDLineDashPattern : COSObjectable
{
    private readonly int _phase;
    private readonly float[] _dashArray;

    public PDLineDashPattern()
        : this(Array.Empty<float>(), 0)
    {
    }

    public PDLineDashPattern(COSArray array, int phase)
        : this(array?.ToFloatArray() ?? Array.Empty<float>(), phase)
    {
    }

    public PDLineDashPattern(float[] dashArray, int phase)
    {
        _dashArray = (float[])(dashArray ?? Array.Empty<float>()).Clone();
        if (phase < 0)
        {
            float sum2 = _dashArray.Sum() * 2f;
            if (sum2 > 0f)
            {
                phase += (-phase < sum2) ? (int)sum2 : (int)((Math.Floor(-phase / sum2) + 1) * sum2);
            }
            else
            {
                phase = 0;
            }
        }

        _phase = phase;
    }

    public int GetPhase() => _phase;
    public int GetPhaseStart() => _phase;
    public float[] GetDashArray() => (float[])_dashArray.Clone();

    public COSArray GetCOSObject()
    {
        COSArray cos = new();
        cos.Add(COSArray.Of(_dashArray));
        cos.Add(COSInteger.Get(_phase));
        return cos;
    }

    COSBase COSObjectable.GetCOSObject() => GetCOSObject();
}
