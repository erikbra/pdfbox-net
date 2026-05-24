/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/PDXObject.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics;

/// <summary>
/// A PDF XObject (external object) that can be referenced from a content stream via the "Do" operator.
/// The base class provides access to the underlying COS stream dictionary.
/// Concrete subclasses (image XObjects, form XObjects) extend this with type-specific behavior.
/// </summary>
public class PDXObject
{
    private readonly COSStream? _stream;

    /// <summary>Creates a PDXObject with no backing stream (used as a placeholder reference).</summary>
    public PDXObject()
    {
    }

    /// <summary>Creates a PDXObject backed by the given COS stream.</summary>
    public PDXObject(COSStream stream)
    {
        _stream = stream;
    }

    /// <summary>Returns the underlying COS stream, or null if this object has no backing stream.</summary>
    public COSStream? GetStream() => _stream;

    /// <summary>Returns the subtype name from the stream dictionary, or null.</summary>
    public string? GetSubtype() => _stream?.GetString(COSName.GetPDFName("Subtype"));
}
