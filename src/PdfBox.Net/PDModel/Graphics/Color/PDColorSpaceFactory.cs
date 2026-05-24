/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted factory wrapper for PDFBox color-space creation.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDColorSpace.java
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
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public static class PDColorSpaceFactory
{
    public static PDColorSpace Create(COSBase colorSpace)
    {
        return PDColorSpace.Create(colorSpace);
    }

    public static PDColorSpace Create(COSBase colorSpace, PDResources? resources)
    {
        return PDColorSpace.Create(colorSpace, resources);
    }

    public static PDColorSpace Create(COSName colorSpaceName, PDResources? resources)
    {
        return PDColorSpace.Create(colorSpaceName, resources);
    }
}
