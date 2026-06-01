/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/SCNToolTip.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Debugger.Streampane.Tooltip;

/// <summary>
/// Tooltip for SCN/scn (custom/special) operators.
/// Adapted from Apache PDFBox SCNToolTip (Khyrul Bashar).
/// </summary>
public sealed class SCNToolTip : ColorToolTip
{
    /// <param name="resources">Page/form resource dictionary.</param>
    /// <param name="colorSpaceName">
    ///   The color-space name token from the most-recent CS/cs operator (may start with '/').</param>
    /// <param name="rowText">Full row text including the SCN/scn operator.</param>
    public SCNToolTip(PDResources resources, string colorSpaceName, string rowText)
    {
        string csName = colorSpaceName.TrimStart('/').Trim();
        PDColorSpace? colorSpace = null;
        try
        {
            colorSpace = resources.GetColorSpace(COSName.GetPDFName(csName));
        }
        catch
        {
            // colorSpace stays null
        }

        if (colorSpace is PDPattern)
        {
            ToolTipText = "<html>Pattern</html>";
            return;
        }

        if (colorSpace != null)
        {
            float[]? values = ExtractColorValues(rowText);
            if (values != null)
            {
                try
                {
                    float[] rgb = colorSpace.ToRGB(values);
                    ToolTipText = GetMarkUp(ColorHexValue(rgb[0], rgb[1], rgb[2]));
                }
                catch
                {
                    // silently ignore
                }
            }
        }
    }
}
