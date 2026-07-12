/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PatchMeshesShadingContext.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: fc00e427de8a1046efe6348d64d5529b479aea13
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

using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

public class PatchMeshesShadingContext : TriangleBasedShadingContext
{
    /// <summary>
    /// Creates a context used for patch-mesh fill operations.
    /// </summary>
    /// <param name="shading">The shading type to be used.</param>
    /// <param name="matrix">The pattern matrix concatenated with that of the parent content stream.</param>
    protected PatchMeshesShadingContext(PDMeshBasedShadingType shading, Matrix matrix)
        : base(shading)
    {
    }
}
