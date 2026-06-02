/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSDeviceN.java
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

namespace PdfBox.Net.Debugger.Colorpane;

/// <summary>
/// Data model for a DeviceN color space.  Extracts per-colorant maximum/minimum RGB values.
/// Adapted from Apache PDFBox CSDeviceN (Khyrul Bashar).
/// </summary>
public sealed class CSDeviceN
{
    public DeviceNColorant[] Colorants { get; }

    public string? ErrorMessage { get; private set; }

    public bool HasError => ErrorMessage != null;

    public CSDeviceN(COSArray array)
    {
        try
        {
            Colorants = GetColorantData(array);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Colorants = [];
        }
    }

    private static DeviceNColorant[] GetColorantData(COSArray array)
    {
        // Read colorant names directly from the COSArray (index 1 is a COSArray of COSName).
        COSArray? nameArray = array.Size() > 1 ? array.GetObject(1) as COSArray : null;
        int componentCount = nameArray?.Size() ?? 0;
        if (componentCount == 0)
        {
            return [];
        }

        var deviceN = new PDDeviceN(array, null);
        var colorants = new DeviceNColorant[componentCount];
        for (int i = 0; i < componentCount; i++)
        {
            string name = nameArray!.GetName(i) ?? $"Component{i}";
            float[] maximum = new float[componentCount];
            float[] minimum = new float[componentCount];
            maximum[i] = 1f;

            float[] maxRgb = deviceN.ToRGB(maximum);
            float[] minRgb = deviceN.ToRGB(minimum);

            colorants[i] = new DeviceNColorant
            {
                Name = name,
                Maximum = (maxRgb[0], maxRgb[1], maxRgb[2]),
                Minimum = (minRgb[0], minRgb[1], minRgb[2]),
            };
        }

        return colorants;
    }
}
