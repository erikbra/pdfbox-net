/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/CIDSystemInfo.java
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

namespace PdfBox.Net.PDModel.Font;

/// <summary>
/// Represents a CIDSystemInfo for the FontMapper API.
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public sealed class CIDSystemInfo
{
    private readonly string _registry;
    private readonly string _ordering;
    private readonly int _supplement;

    /// <summary>
    /// Creates a new CIDSystemInfo.
    /// </summary>
    public CIDSystemInfo(string registry, string ordering, int supplement)
    {
        _registry = registry;
        _ordering = ordering;
        _supplement = supplement;
    }

    /// <summary>
    /// Returns the registry name.
    /// </summary>
    public string GetRegistry() => _registry;

    /// <summary>
    /// Returns the ordering string.
    /// </summary>
    public string GetOrdering() => _ordering;

    /// <summary>
    /// Returns the supplement number.
    /// </summary>
    public int GetSupplement() => _supplement;

    /// <inheritdoc/>
    public override string ToString() => $"{GetRegistry()}-{GetOrdering()}-{GetSupplement()}";
}
