/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/TextPositionComparator.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

namespace PdfBox.Net.Text;

/// <summary>
/// Comparator for <see cref="TextPosition"/> instances.
/// </summary>
public class TextPositionComparator : IComparer<TextPosition>
{
    /// <inheritdoc/>
    public int Compare(TextPosition? pos1, TextPosition? pos2)
    {
        if (ReferenceEquals(pos1, pos2))
        {
            return 0;
        }

        if (pos1 is null)
        {
            return -1;
        }

        if (pos2 is null)
        {
            return 1;
        }

        int cmp1 = pos1.GetDir().CompareTo(pos2.GetDir());
        if (cmp1 != 0)
        {
            return cmp1;
        }

        float x1 = pos1.GetXDirAdj();
        float x2 = pos2.GetXDirAdj();
        float pos1YBottom = pos1.GetYDirAdj();
        float pos2YBottom = pos2.GetYDirAdj();
        float pos1YTop = pos1YBottom - pos1.GetHeightDir();
        float pos2YTop = pos2YBottom - pos2.GetHeightDir();
        float yDifference = MathF.Abs(pos1YBottom - pos2YBottom);

        if (yDifference < 0.1f ||
            pos2YBottom >= pos1YTop && pos2YBottom <= pos1YBottom ||
            pos1YBottom >= pos2YTop && pos1YBottom <= pos2YBottom)
        {
            return x1.CompareTo(x2);
        }
        else if (pos1YBottom < pos2YBottom)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}
