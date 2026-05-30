/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
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

namespace PdfBox.Net.ContentStream.Operator.Graphics;

/// <summary>
/// EI — ends an inline image object.
/// </summary>
/// <remarks>
/// <para>
/// Architecture note: upstream PDFBox combines BI/ID/EI handling in a single class
/// (<c>BeginInlineImage.java</c>). In the .NET port the three tokens are modelled as
/// separate <see cref="OperatorProcessor"/> subclasses to align with the split-operator
/// design of the .NET content-stream engine.
/// </para>
/// <para>
/// In normal PDF processing, the <c>EI</c> token is consumed internally by
/// <see cref="PdfBox.Net.PdfParser.PDFStreamParser"/> as part of inline-image data
/// collection, so this operator fires only in edge-case direct token dispatch.
/// </para>
/// </remarks>
public sealed class EndInlineImage : OperatorProcessor
{
    /// <summary>Initialises the processor bound to the given stream engine context.</summary>
    public EndInlineImage(PDFStreamEngine context) : base(OperatorName.END_INLINE_IMAGE, context) { }

    /// <inheritdoc/>
    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.EndInlineImage();
    }
}
