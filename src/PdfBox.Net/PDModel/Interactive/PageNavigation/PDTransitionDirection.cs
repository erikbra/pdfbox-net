/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDTransitionDirection.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

/// <summary>
/// Direction used by transition effects that require directional movement.
/// </summary>
public enum PDTransitionDirection
{
    LEFT_TO_RIGHT,
    BOTTOM_TO_TOP,
    RIGHT_TO_LEFT,
    TOP_TO_BOTTOM,
    TOP_LEFT_TO_BOTTOM_RIGHT,
    NONE
}

internal static class PDTransitionDirectionExtensions
{
    public static COSBase GetCOSBase(this PDTransitionDirection direction)
    {
        return direction switch
        {
            PDTransitionDirection.LEFT_TO_RIGHT => COSInteger.Get(0),
            PDTransitionDirection.BOTTOM_TO_TOP => COSInteger.Get(90),
            PDTransitionDirection.RIGHT_TO_LEFT => COSInteger.Get(180),
            PDTransitionDirection.TOP_TO_BOTTOM => COSInteger.Get(270),
            PDTransitionDirection.TOP_LEFT_TO_BOTTOM_RIGHT => COSInteger.Get(315),
            PDTransitionDirection.NONE => COSName.NONE,
            _ => COSInteger.ZERO
        };
    }
}
