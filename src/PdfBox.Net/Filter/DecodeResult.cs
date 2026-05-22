/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/DecodeResult.java
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

namespace PdfBox.Net.Filter;

/// <summary>
/// The result of a filter decode operation.
/// </summary>
public sealed class DecodeResult
{
    private readonly COSDictionary _parameters;
    private object? _colorSpace;
    private object? _smask;

    internal DecodeResult(COSDictionary parameters)
    {
        _parameters = parameters;
    }

    internal DecodeResult(COSDictionary parameters, object? colorSpace)
    {
        _parameters = parameters;
        _colorSpace = colorSpace;
    }

    public static DecodeResult CreateDefault()
    {
        return new DecodeResult(new COSDictionary());
    }

    public COSDictionary GetParameters()
    {
        return _parameters;
    }

    public object? GetJPXColorSpace()
    {
        return _colorSpace;
    }

    internal void SetColorSpace(object? colorSpace)
    {
        _colorSpace = colorSpace;
    }

    internal void SetJPXSMask(object? smask)
    {
        _smask = smask;
    }

    public object? GetJPXSMask()
    {
        return _smask;
    }
}
