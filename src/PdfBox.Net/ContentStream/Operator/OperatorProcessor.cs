/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/OperatorProcessor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.ContentStream.Operator;

/// <summary>
/// Base class for all PDF content-stream operator processors.
/// Each concrete subclass handles exactly one PDF operator keyword.
/// </summary>
public abstract class OperatorProcessor
{
    protected OperatorProcessor(string name, PDFStreamEngine context)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Context = context;
    }

    /// <summary>The PDF operator keyword this processor handles (e.g. "q", "BT").</summary>
    public string Name { get; }

    protected PDFStreamEngine Context { get; }

    public virtual string GetName()
    {
        return Name;
    }

    /// <summary>Execute this operator with the given stack of operands.</summary>
    public virtual void Process(Operator op, IList<COSBase> operands) { }
}
