/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceColorSpace.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Device colour spaces directly specify colours or shades of gray produced by an output device.
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public abstract class PDDeviceColorSpace : PDColorSpace
{
    /// <summary>
    /// Initialises a device colour space from a COS name.
    /// </summary>
    protected PDDeviceColorSpace(COSBase cosObject) : base(cosObject)
    {
    }

    /// <inheritdoc/>
    public override COSBase GetCOSObject() => COSName.GetPDFName(GetName());

    /// <inheritdoc/>
    public override string ToString() => GetName();
}
