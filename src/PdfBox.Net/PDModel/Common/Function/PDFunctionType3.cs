/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunctionType3.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common.Function;

public class PDFunctionType3 : PDFunction
{
    private COSArray? _functions;
    private COSArray? _encode;
    private COSArray? _bounds;
    private PDFunction[]? _functionsArray;
    private float[]? _boundsValues;

    public PDFunctionType3(COSBase functionStream)
        : base(functionStream)
    {
    }

    public override int GetFunctionType() => 3;

    public override float[] Eval(float[] input)
    {
        PDFunction? function = null;
        float x = input[0];
        PDRange domain = GetDomainForInput(0);
        x = ClipToRange(x, domain.GetMin(), domain.GetMax());

        _functionsArray ??= GetFunctions().ToList().Select(baseValue => Create(baseValue!)).ToArray();
        if (_functionsArray.Length == 1)
        {
            function = _functionsArray[0];
            PDRange encRange = GetEncodeForParameter(0);
            x = Interpolate(x, domain.GetMin(), domain.GetMax(), encRange.GetMin(), encRange.GetMax());
        }
        else
        {
            _boundsValues ??= GetBounds().ToFloatArray();
            float[] partitionValues = new float[_boundsValues.Length + 2];
            partitionValues[0] = domain.GetMin();
            partitionValues[^1] = domain.GetMax();
            Array.Copy(_boundsValues, 0, partitionValues, 1, _boundsValues.Length);
            for (int i = 0; i < partitionValues.Length - 1; i++)
            {
                if (x >= partitionValues[i] && (x < partitionValues[i + 1] || (i == partitionValues.Length - 2 && x.Equals(partitionValues[i + 1]))))
                {
                    function = _functionsArray[i];
                    PDRange encRange = GetEncodeForParameter(i);
                    x = Interpolate(x, partitionValues[i], partitionValues[i + 1], encRange.GetMin(), encRange.GetMax());
                    break;
                }
            }
        }

        if (function is null)
        {
            throw new IOException("partition not found in type 3 function");
        }

        float[] functionResult = function.Eval([x]);
        return ClipToRange(functionResult);
    }

    public COSArray GetFunctions() => _functions ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Functions")) ?? new COSArray();

    public COSArray GetBounds() => _bounds ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Bounds")) ?? new COSArray();

    public COSArray GetEncode() => _encode ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Encode")) ?? new COSArray();

    private PDRange GetEncodeForParameter(int n) => new(GetEncode(), n);
}
