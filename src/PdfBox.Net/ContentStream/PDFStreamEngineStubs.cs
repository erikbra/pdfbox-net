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

namespace PdfBox.Net.ContentStream.Operator
{
    public abstract class OperatorProcessor
    {
        protected OperatorProcessor(PdfBox.Net.ContentStream.PDFStreamEngine context)
        {
            Context = context;
        }

        protected PdfBox.Net.ContentStream.PDFStreamEngine Context { get; }
    }

    public sealed class DrawObject : OperatorProcessor
    {
        public DrawObject(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(context)
        {
        }
    }
}

namespace PdfBox.Net.ContentStream.Operator.State
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class Concatenate : OperatorProcessor { public Concatenate(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class Restore : OperatorProcessor { public Restore(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class Save : OperatorProcessor { public Save(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetGraphicsStateParameters : OperatorProcessor { public SetGraphicsStateParameters(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetMatrix : OperatorProcessor { public SetMatrix(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
}

namespace PdfBox.Net.ContentStream.Operator.Text
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class BeginText : OperatorProcessor { public BeginText(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class EndText : OperatorProcessor { public EndText(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetFontAndSize : OperatorProcessor { public SetFontAndSize(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetTextHorizontalScaling : OperatorProcessor { public SetTextHorizontalScaling(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class ShowTextAdjusted : OperatorProcessor { public ShowTextAdjusted(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class ShowTextLine : OperatorProcessor { public ShowTextLine(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class ShowTextLineAndSpace : OperatorProcessor { public ShowTextLineAndSpace(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class MoveText : OperatorProcessor { public MoveText(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class MoveTextSetLeading : OperatorProcessor { public MoveTextSetLeading(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class NextLine : OperatorProcessor { public NextLine(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetCharSpacing : OperatorProcessor { public SetCharSpacing(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetTextLeading : OperatorProcessor { public SetTextLeading(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetTextRenderingMode : OperatorProcessor { public SetTextRenderingMode(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetTextRise : OperatorProcessor { public SetTextRise(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class SetWordSpacing : OperatorProcessor { public SetWordSpacing(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class ShowText : OperatorProcessor { public ShowText(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
}

namespace PdfBox.Net.ContentStream.Operator.MarkedContent
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class BeginMarkedContentSequenceWithProperties : OperatorProcessor { public BeginMarkedContentSequenceWithProperties(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class BeginMarkedContentSequence : OperatorProcessor { public BeginMarkedContentSequence(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class EndMarkedContentSequence : OperatorProcessor { public EndMarkedContentSequence(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class MarkedContentPoint : OperatorProcessor { public MarkedContentPoint(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class MarkedContentPointWithProperties : OperatorProcessor { public MarkedContentPointWithProperties(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
    public sealed class DrawObject : OperatorProcessor { public DrawObject(PdfBox.Net.ContentStream.PDFStreamEngine context) : base(context) { } }
}
