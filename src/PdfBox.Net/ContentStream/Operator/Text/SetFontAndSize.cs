/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/text/SetFontAndSize.java
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
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.ContentStream.Operator.Text;

/// <summary>
/// Processes the PDF "Tf" operator: set the text font and font size.
/// When an active resource dictionary is available the font is resolved from
/// the /Font sub-dictionary; otherwise the font entry in the text state is
/// cleared so that callers receive <see langword="null"/> rather than a stale value.
/// </summary>
public sealed class SetFontAndSize : OperatorProcessor
{
    public SetFontAndSize(PDFStreamEngine context)
        : base(OperatorName.SET_FONT_AND_SIZE, context)
    {
    }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2) return;

        // operands[0] is the font resource name (COSName), operands[1] is the size.
        if (operands[1] is COSNumber sizeNumber)
        {
            Context.GetGraphicsState().GetTextState().FontSize = sizeNumber.FloatValue();
        }

        PDFont? font = null;
        if (operands[0] is COSName fontName)
        {
            font = Context.GetResources()?.GetFont(fontName);
        }

        // Always write back (even null) so callers see a consistent state.
        Context.GetGraphicsState().GetTextState().Font = font;
    }
}
