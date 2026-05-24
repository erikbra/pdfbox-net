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
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;

namespace PdfBox.Net.ContentStream.Operator
{
    public abstract class OperatorProcessor
    {
        protected OperatorProcessor(string name, PdfBox.Net.ContentStream.PDFStreamEngine context)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Context = context;
        }

        /// <summary>The PDF operator keyword this processor handles (e.g. "q", "BT").</summary>
        public string Name { get; }

        protected PdfBox.Net.ContentStream.PDFStreamEngine Context { get; }

        /// <summary>Execute this operator with the given stack of operands.</summary>
        public virtual void Process(Operator op, IList<COSBase> operands) { }
    }

    public sealed class DrawObject : OperatorProcessor
    {
        public DrawObject(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.DRAW_OBJECT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.XObject(new PdfBox.Net.PDModel.Graphics.PDXObject());
        }
    }
}

namespace PdfBox.Net.ContentStream.Operator.State
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class Concatenate : OperatorProcessor
    {
        public Concatenate(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.CONCAT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 6) return;
            float a = ((COSNumber)operands[0]).FloatValue();
            float b = ((COSNumber)operands[1]).FloatValue();
            float c = ((COSNumber)operands[2]).FloatValue();
            float d = ((COSNumber)operands[3]).FloatValue();
            float e = ((COSNumber)operands[4]).FloatValue();
            float f = ((COSNumber)operands[5]).FloatValue();
            Context.ConcatenateMatrix(new Matrix(a, b, c, d, e, f));
        }
    }

    public sealed class Restore : OperatorProcessor
    {
        public Restore(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.RESTORE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.RestoreGraphicsState();
        }
    }

    public sealed class Save : OperatorProcessor
    {
        public Save(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SAVE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.SaveGraphicsState();
        }
    }

    public sealed class SetGraphicsStateParameters : OperatorProcessor
    {
        public SetGraphicsStateParameters(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_GRAPHICS_STATE_PARAMS, context)
        {
        }
    }

    public sealed class SetMatrix : OperatorProcessor
    {
        public SetMatrix(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_MATRIX, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 6) return;
            float a = ((COSNumber)operands[0]).FloatValue();
            float b = ((COSNumber)operands[1]).FloatValue();
            float c = ((COSNumber)operands[2]).FloatValue();
            float d = ((COSNumber)operands[3]).FloatValue();
            float e = ((COSNumber)operands[4]).FloatValue();
            float f = ((COSNumber)operands[5]).FloatValue();
            Matrix m = new Matrix(a, b, c, d, e, f);
            Context.SetTextMatrices(m, m);
        }
    }
}

namespace PdfBox.Net.ContentStream.Operator.Text
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class BeginText : OperatorProcessor
    {
        public BeginText(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.BEGIN_TEXT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.SetTextMatrices(new Matrix(), new Matrix());
        }
    }

    public sealed class EndText : OperatorProcessor
    {
        public EndText(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.END_TEXT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.SetTextMatrices(new Matrix(), new Matrix());
        }
    }

    public sealed class SetFontAndSize : OperatorProcessor
    {
        public SetFontAndSize(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_FONT_AND_SIZE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 2) return;
            if (operands[1] is COSNumber sizeNumber)
            {
                Context.GetGraphicsState().GetTextState().FontSize = sizeNumber.FloatValue();
            }
        }
    }

    public sealed class SetTextHorizontalScaling : OperatorProcessor
    {
        public SetTextHorizontalScaling(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_TEXT_HORIZONTAL_SCALING, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().HorizontalScaling = n.FloatValue();
        }
    }

    public sealed class ShowTextAdjusted : OperatorProcessor
    {
        public ShowTextAdjusted(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SHOW_TEXT_ADJUSTED, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSArray array) return;

            PDTextState textState = Context.GetGraphicsState().GetTextState();
            float fontSize = textState.GetFontSize();
            float horizontalScaling = textState.GetHorizontalScaling() / 100f;

            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? obj = array.Get(i);
                if (obj is COSString cosString)
                {
                    Context.ShowStringGlyphs(cosString.GetBytes());
                }
                else if (obj is COSNumber number)
                {
                    float adjustment = number.FloatValue() / 1000f;
                    float tx = -adjustment * fontSize * horizontalScaling;
                    Matrix advance = Matrix.GetTranslateInstance(tx, 0);
                    Context.SetTextMatrix(advance.Multiply(Context.GetTextMatrix()));
                }
            }
        }
    }

    public sealed class ShowTextLine : OperatorProcessor
    {
        public ShowTextLine(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SHOW_TEXT_LINE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            new NextLine(Context).Process(op, new List<COSBase>());
            Context.ShowStringGlyphs(
                operands.Count > 0 && operands[0] is COSString s ? s.GetBytes() : []);
        }
    }

    public sealed class ShowTextLineAndSpace : OperatorProcessor
    {
        public ShowTextLineAndSpace(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SHOW_TEXT_LINE_AND_SPACE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 3) return;
            PDTextState ts = Context.GetGraphicsState().GetTextState();
            if (operands[0] is COSNumber ws) ts.WordSpacing = ws.FloatValue();
            if (operands[1] is COSNumber cs) ts.CharacterSpacing = cs.FloatValue();
            new ShowTextLine(Context).Process(op, new List<COSBase> { operands[2] });
        }
    }

    public sealed class MoveText : OperatorProcessor
    {
        public MoveText(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.MOVE_TEXT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 2) return;
            float tx = ((COSNumber)operands[0]).FloatValue();
            float ty = ((COSNumber)operands[1]).FloatValue();
            Matrix tlm = Matrix.GetTranslateInstance(tx, ty);
            Matrix newLineMatrix = tlm.Multiply(Context.GetTextLineMatrix());
            Context.SetTextLineMatrix(newLineMatrix);
            Context.SetTextMatrix(newLineMatrix);
        }
    }

    public sealed class MoveTextSetLeading : OperatorProcessor
    {
        public MoveTextSetLeading(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.MOVE_TEXT_SET_LEADING, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 2) return;
            float ty = ((COSNumber)operands[1]).FloatValue();
            Context.GetGraphicsState().GetTextState().Leading = -ty;
            new MoveText(Context).Process(op, operands);
        }
    }

    public sealed class NextLine : OperatorProcessor
    {
        public NextLine(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.NEXT_LINE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            float leading = Context.GetGraphicsState().GetTextState().GetLeading();
            new MoveText(Context).Process(op, new List<COSBase>
            {
                COSInteger.ZERO,
                new COSFloat(-leading),
            });
        }
    }

    public sealed class SetCharSpacing : OperatorProcessor
    {
        public SetCharSpacing(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_CHAR_SPACING, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().CharacterSpacing = n.FloatValue();
        }
    }

    public sealed class SetTextLeading : OperatorProcessor
    {
        public SetTextLeading(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_TEXT_LEADING, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().Leading = n.FloatValue();
        }
    }

    public sealed class SetTextRenderingMode : OperatorProcessor
    {
        public SetTextRenderingMode(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_TEXT_RENDERINGMODE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().RenderingMode = n.IntValue();
        }
    }

    public sealed class SetTextRise : OperatorProcessor
    {
        public SetTextRise(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_TEXT_RISE, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().Rise = n.FloatValue();
        }
    }

    public sealed class SetWordSpacing : OperatorProcessor
    {
        public SetWordSpacing(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SET_WORD_SPACING, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSNumber n) return;
            Context.GetGraphicsState().GetTextState().WordSpacing = n.FloatValue();
        }
    }

    public sealed class ShowText : OperatorProcessor
    {
        public ShowText(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.SHOW_TEXT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            if (operands.Count < 1 || operands[0] is not COSString s) return;
            Context.ShowStringGlyphs(s.GetBytes());
        }
    }
}

namespace PdfBox.Net.ContentStream.Operator.MarkedContent
{
    using PdfBox.Net.ContentStream.Operator;

    public sealed class BeginMarkedContentSequenceWithProperties : OperatorProcessor
    {
        public BeginMarkedContentSequenceWithProperties(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.BEGIN_MARKED_CONTENT_SEQ, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            COSName tag = operands.Count > 0 && operands[0] is COSName n ? n : COSName.GetPDFName("Unknown");
            COSDictionary? props = operands.Count > 1 ? operands[1] as COSDictionary : null;
            Context.BeginMarkedContentSequence(tag, props);
        }
    }

    public sealed class BeginMarkedContentSequence : OperatorProcessor
    {
        public BeginMarkedContentSequence(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.BEGIN_MARKED_CONTENT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            COSName tag = operands.Count > 0 && operands[0] is COSName n ? n : COSName.GetPDFName("Unknown");
            Context.BeginMarkedContentSequence(tag, null);
        }
    }

    public sealed class EndMarkedContentSequence : OperatorProcessor
    {
        public EndMarkedContentSequence(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.END_MARKED_CONTENT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.EndMarkedContentSequence();
        }
    }

    public sealed class MarkedContentPoint : OperatorProcessor
    {
        public MarkedContentPoint(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.MARKED_CONTENT_POINT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            COSName tag = operands.Count > 0 && operands[0] is COSName n ? n : COSName.GetPDFName("Unknown");
            Context.MarkedContentPoint(tag, null);
        }
    }

    public sealed class MarkedContentPointWithProperties : OperatorProcessor
    {
        public MarkedContentPointWithProperties(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.MARKED_CONTENT_POINT_WITH_PROPS, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            COSName tag = operands.Count > 0 && operands[0] is COSName n ? n : COSName.GetPDFName("Unknown");
            COSDictionary? props = operands.Count > 1 ? operands[1] as COSDictionary : null;
            Context.MarkedContentPoint(tag, props);
        }
    }

    public sealed class DrawObject : OperatorProcessor
    {
        public DrawObject(PdfBox.Net.ContentStream.PDFStreamEngine context)
            : base(OperatorName.DRAW_OBJECT, context)
        {
        }

        public override void Process(Operator op, IList<COSBase> operands)
        {
            Context.XObject(new PdfBox.Net.PDModel.Graphics.PDXObject());
        }
    }
}
