/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/ICOSParser.java
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

using PdfBox.Net.IO;

namespace PdfBox.Net.COS;

public interface ICOSParser
{
    /// <summary>
    /// Dereference the COSBase object which is referenced by the given COSObject.
    /// </summary>
    /// <param name="obj">The COSObject which references the COSBase object to be dereferenced.</param>
    /// <returns>The referenced object.</returns>
    /// <exception cref="IOException">If something went wrong when dereferencing the COSBase object.</exception>
    COSBase DereferenceCOSObject(COSObject obj);

    /// <summary>
    /// Creates a random access read view starting at the given position with the given length.
    /// </summary>
    /// <param name="startPosition">Start position within the underlying random access read.</param>
    /// <param name="streamLength">Stream length.</param>
    /// <returns>The random access read view.</returns>
    /// <exception cref="IOException">If something went wrong when creating the view for the RandomAccessRead.</exception>
    RandomAccessReadView CreateRandomAccessReadView(long startPosition, long streamLength);
}
