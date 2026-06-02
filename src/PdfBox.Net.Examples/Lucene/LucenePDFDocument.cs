/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/lucene/LucenePDFDocument.java
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

namespace PdfBox.Net.Examples.Lucene;

/// <summary>
/// Maps a PDF document to a Lucene document for full-text indexing. Requires the Lucene.Net
/// package which is not included in this .NET port.
/// </summary>
public class LucenePDFDocument
{
    private LucenePDFDocument()
    {
    }

    /// <summary>
    /// Returns a Lucene document for the given PDF file.
    /// </summary>
    /// <param name="file">The PDF file to index.</param>
    /// <returns>A Lucene document.</returns>
    public static object GetDocument(string file)
    {
        // NOTE: Lucene.Net integration is not yet implemented in this .NET port.
        throw new NotSupportedException(
            "Lucene.Net indexing is not yet implemented in this .NET port. " +
            "Add a reference to Lucene.Net and port the Java implementation.");
    }
}
