/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreateSeparationColorBox.java
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

using PdfBox.Net.PDModel;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example shows how to use a separation color / spot color. Here it is a placeholder for gold,
/// and it is displayed as yellow.
/// </summary>
public class CreateSeparationColorBox
{
    private CreateSeparationColorBox()
    {
    }

    public static void Main(string[] args)
    {
        // NOTE: Separation color space support and the required COSName constants (Separation,
        // DeviceRGB) and PDSeparation/PDFunctionType2 construction from raw COSArrays are not
        // fully available in this .NET port.
        throw new NotSupportedException(
            "Separation color space operations are not yet fully implemented in this .NET port.");
    }
}
