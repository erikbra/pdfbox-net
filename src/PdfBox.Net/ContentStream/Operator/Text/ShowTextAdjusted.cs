/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/text/ShowTextAdjusted.java
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

namespace PdfBox.Net.ContentStream.Operator.Text;

/// <summary>
/// Processes the PDF "TJ" operator: show text strings with individual glyph
/// positioning.  The operand is an array whose elements are either strings
/// (shown as glyphs) or numbers (kerning adjustments applied to the text matrix).
/// </summary>
public sealed class ShowTextAdjusted : OperatorProcessor
{
    public ShowTextAdjusted(PDFStreamEngine context)
        : base(OperatorName.SHOW_TEXT_ADJUSTED, context)
    {
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSArray array) return;

        PDTextState textState = Context.GetGraphicsState().GetTextState();
        float fontSize = textState.GetFontSize();
        float horizontalScaling = textState.GetHorizontalScaling() / 100f;
        bool isVertical = textState.GetFont()?.IsVertical() ?? false;

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
                float tx;
                float ty;
                if (isVertical)
                {
                    tx = 0;
                    ty = -adjustment * fontSize;
                }
                else
                {
                    tx = -adjustment * fontSize * horizontalScaling;
                    ty = 0;
                }

                Context.SetTextMatrix(Context.GetTextMatrix().Translate(tx, ty));
            }
        }
    }
}
