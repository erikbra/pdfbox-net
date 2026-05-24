/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunction.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

public abstract class PDFunction : COSObjectable
{
    private PDStream? _functionStream;
    private COSDictionary? _functionDictionary;
    private COSArray? _domain;
    private COSArray? _range;
    private int _numberOfInputValues = -1;
    private int _numberOfOutputValues = -1;

    protected PDFunction(COSBase? function)
    {
        if (function is COSStream stream)
        {
            _functionStream = new PDStream(stream);
            _functionStream.GetCOSObject().SetItem(COSName.TYPE, COSName.GetPDFName("Function"));
        }
        else if (function is COSDictionary dictionary)
        {
            _functionDictionary = dictionary;
        }
        else
        {
            _functionDictionary = new COSDictionary();
        }
    }

    public abstract int GetFunctionType();

    public COSDictionary GetCOSObject() => _functionStream?.GetCOSObject() ?? _functionDictionary!;

    COSBase COSObjectable.GetCOSObject() => GetCOSObject();

    protected PDStream? GetPDStream() => _functionStream;

    public static PDFunction Create(COSBase function)
    {
        if (ReferenceEquals(function, COSName.IDENTITY))
        {
            return new PDFunctionTypeIdentity(null);
        }

        COSBase? baseValue = function is COSObject cosObject ? cosObject.GetObject() : function;
        if (baseValue is not COSDictionary functionDictionary)
        {
            throw new IOException($"Error: Function must be a Dictionary, but is {(baseValue is null ? "(null)" : baseValue.GetType().Name)}");
        }

        int functionType = functionDictionary.GetInt(COSName.GetPDFName("FunctionType"));
        return functionType switch
        {
            0 => new PDFunctionType0(functionDictionary),
            2 => new PDFunctionType2(functionDictionary),
            3 => new PDFunctionType3(functionDictionary),
            4 => new PDFunctionType4(functionDictionary),
            _ => throw new IOException($"Error: Unknown function type {functionType}")
        };
    }

    public int GetNumberOfOutputParameters()
    {
        if (_numberOfOutputValues == -1)
        {
            COSArray? rangeValues = GetRangeValues();
            _numberOfOutputValues = rangeValues is null ? 0 : rangeValues.Size() / 2;
        }

        return _numberOfOutputValues;
    }

    public PDRange GetRangeForOutput(int n)
    {
        COSArray rangeValues = GetRangeValues() ?? throw new IOException("Range missing in function");
        return new PDRange(rangeValues, n);
    }

    public void SetRangeValues(COSArray rangeValues)
    {
        _range = rangeValues;
        GetCOSObject().SetItem(COSName.GetPDFName("Range"), rangeValues);
    }

    public int GetNumberOfInputParameters()
    {
        if (_numberOfInputValues == -1)
        {
            COSArray array = GetDomainValues();
            _numberOfInputValues = array.Size() / 2;
        }

        return _numberOfInputValues;
    }

    public PDRange GetDomainForInput(int n)
    {
        return new PDRange(GetDomainValues(), n);
    }

    public void SetDomainValues(COSArray domainValues)
    {
        _domain = domainValues;
        GetCOSObject().SetItem(COSName.GetPDFName("Domain"), domainValues);
    }

    public abstract float[] Eval(float[] input);

    protected virtual COSArray? GetRangeValues()
    {
        _range ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Range"));
        return _range;
    }

    protected float[] ClipToRange(float[] inputValues)
    {
        COSArray? rangesArray = GetRangeValues();
        if (rangesArray is null || rangesArray.IsEmpty())
        {
            return inputValues;
        }

        float[] rangeValues = rangesArray.ToFloatArray();
        int numberOfRanges = rangeValues.Length / 2;
        float[] result = new float[numberOfRanges];
        for (int i = 0; i < numberOfRanges; i++)
        {
            int index = i << 1;
            result[i] = ClipToRange(inputValues[i], rangeValues[index], rangeValues[index + 1]);
        }

        return result;
    }

    protected float ClipToRange(float x, float rangeMin, float rangeMax)
    {
        if (x < rangeMin)
        {
            return rangeMin;
        }

        if (x > rangeMax)
        {
            return rangeMax;
        }

        return x;
    }

    protected float Interpolate(float x, float xRangeMin, float xRangeMax, float yRangeMin, float yRangeMax)
    {
        if (xRangeMax == xRangeMin)
        {
            return yRangeMin;
        }

        return yRangeMin + (((x - xRangeMin) * (yRangeMax - yRangeMin)) / (xRangeMax - xRangeMin));
    }

    private COSArray GetDomainValues()
    {
        _domain ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Domain")) ?? throw new IOException("Domain missing in function");
        return _domain;
    }
}
