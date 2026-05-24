/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunctionType4.java
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

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function.Type4;

namespace PdfBox.Net.PDModel.Common.Function;

public class PDFunctionType4 : PDFunction
{
    private static readonly Operators Operators = new();
    private readonly InstructionSequence _instructions;

    public PDFunctionType4(COSBase functionStream)
        : base(functionStream)
    {
        byte[] bytes = GetPDStream()?.ToByteArray() ?? [];
        string functionText = Encoding.Latin1.GetString(bytes);
        _instructions = InstructionSequenceBuilder.Parse(functionText);
    }

    public override int GetFunctionType() => 4;

    public override float[] Eval(float[] input)
    {
        Type4.ExecutionContext context = new(Operators);
        for (int i = 0; i < input.Length; i++)
        {
            PDRange domain = GetDomainForInput(i);
            float value = ClipToRange(input[i], domain.GetMin(), domain.GetMax());
            context.GetStack().Push(value);
        }

        _instructions.Execute(context);
        int numberOfOutputValues = GetNumberOfOutputParameters();
        int numberOfActualOutputValues = context.GetStack().Count;
        if (numberOfActualOutputValues < numberOfOutputValues)
        {
            throw new InvalidOperationException($"The type 4 function returned {numberOfActualOutputValues} values but the Range entry indicates that {numberOfOutputValues} values be returned.");
        }

        float[] outputValues = new float[numberOfOutputValues];
        for (int i = numberOfOutputValues - 1; i >= 0; i--)
        {
            PDRange range = GetRangeForOutput(i);
            outputValues[i] = context.PopReal();
            outputValues[i] = ClipToRange(outputValues[i], range.GetMin(), range.GetMax());
        }

        return outputValues;
    }
}
