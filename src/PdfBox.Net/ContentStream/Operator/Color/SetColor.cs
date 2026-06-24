/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetColor.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
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
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public abstract class SetColor : OperatorProcessor
{
    protected SetColor(string name, PDFStreamEngine context)
        : base(name, context)
    {
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        PDColorSpace colorSpace = GetColorSpace();
        if (colorSpace is null)
        {
            return;
        }

        if (colorSpace is PDPattern && operands.Count > 0 && operands[^1] is COSName patternName)
        {
            float[] patternComponents = new float[operands.Count - 1];
            for (int i = 0; i < patternComponents.Length; i++)
            {
                if (operands[i] is not COSNumber number)
                {
                    SetColorValue(new PDColor(patternName, colorSpace));
                    return;
                }

                patternComponents[i] = number.FloatValue();
            }

            SetColorValue(new PDColor(patternComponents, patternName, colorSpace));
            return;
        }

        if (colorSpace is not PDPattern && operands.Count < colorSpace.GetNumberOfComponents())
        {
            throw new MissingOperandException(op, operands);
        }

        float[] components = new float[operands.Count];
        for (int i = 0; i < operands.Count; i++)
        {
            if (operands[i] is not COSNumber number)
            {
                SetColorValue(new PDColor(Array.Empty<float>(), colorSpace));
                return;
            }

            components[i] = number.FloatValue();
        }

        SetColorValue(new PDColor(components, colorSpace));
    }

    protected abstract PDColorSpace GetColorSpace();

    protected abstract void SetColorValue(PDColor color);
}
