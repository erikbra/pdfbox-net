/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunctionType0.java
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

public class PDFunctionType0 : PDFunction
{
    private COSArray? _encode;
    private COSArray? _decode;
    private COSArray? _size;
    private int[][]? _samples;

    public PDFunctionType0(COSBase function)
        : base(function)
    {
    }

    public override int GetFunctionType() => 0;

    public COSArray GetSize() => _size ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Size")) ?? new COSArray();

    public int GetBitsPerSample() => GetCOSObject().GetInt(COSName.GetPDFName("BitsPerSample"));

    public int GetOrder() => GetCOSObject().GetInt(COSName.GetPDFName("Order"), 1);

    public void SetBitsPerSample(int bps) => GetCOSObject().SetInt(COSName.GetPDFName("BitsPerSample"), bps);

    public PDRange? GetEncodeForParameter(int paramNum)
    {
        COSArray encodeValues = GetEncodeValues();
        return encodeValues.Size() >= (paramNum * 2) + 1 ? new PDRange(encodeValues, paramNum) : null;
    }

    public void SetEncodeValues(COSArray encodeValues)
    {
        _encode = encodeValues;
        GetCOSObject().SetItem(COSName.GetPDFName("Encode"), encodeValues);
    }

    public PDRange? GetDecodeForParameter(int paramNum)
    {
        COSArray decodeValues = GetDecodeValues();
        return decodeValues.Size() >= (paramNum * 2) + 1 ? new PDRange(decodeValues, paramNum) : null;
    }

    public void SetDecodeValues(COSArray decodeValues)
    {
        _decode = decodeValues;
        GetCOSObject().SetItem(COSName.GetPDFName("Decode"), decodeValues);
    }

    public override float[] Eval(float[] input)
    {
        float[] sizeValues = GetSize().ToFloatArray();
        int bitsPerSample = GetBitsPerSample();
        float maxSample = (float)(Math.Pow(2, bitsPerSample) - 1.0);
        int numberOfInputValues = input.Length;
        int numberOfOutputValues = GetNumberOfOutputParameters();

        int[] inputPrev = new int[numberOfInputValues];
        int[] inputNext = new int[numberOfInputValues];
        input = (float[])input.Clone();

        for (int i = 0; i < numberOfInputValues; i++)
        {
            PDRange domain = GetDomainForInput(i);
            PDRange? encodeValues = GetEncodeForParameter(i) ?? throw new IOException("Encode missing in function");
            float min = domain.GetMin();
            float max = domain.GetMax();
            input[i] = ClipToRange(input[i], min, max);
            input[i] = Interpolate(input[i], min, max, encodeValues.GetMin(), encodeValues.GetMax());
            input[i] = ClipToRange(input[i], 0, sizeValues[i] - 1);
            inputPrev[i] = (int)Math.Floor(input[i]);
            inputNext[i] = (int)Math.Ceiling(input[i]);
        }

        float[] outputValues = new Rinterpol(this, input, inputPrev, inputNext).Rinterpolate();
        for (int i = 0; i < numberOfOutputValues; i++)
        {
            PDRange range = GetRangeForOutput(i);
            PDRange? decodeValues = GetDecodeForParameter(i) ?? throw new IOException("Range missing in function /Decode entry");
            outputValues[i] = Interpolate(outputValues[i], 0, maxSample, decodeValues.GetMin(), decodeValues.GetMax());
            outputValues[i] = ClipToRange(outputValues[i], range.GetMin(), range.GetMax());
        }

        return outputValues;
    }

    private COSArray GetEncodeValues()
    {
        if (_encode is null)
        {
            _encode = GetCOSObject().GetCOSArray(COSName.GetPDFName("Encode"));
            if (_encode is null)
            {
                _encode = new COSArray();
                COSArray sizeValues = GetSize();
                for (int i = 0; i < sizeValues.Size(); i++)
                {
                    _encode.Add(COSInteger.ZERO);
                    _encode.Add(COSInteger.Get(sizeValues.GetInt(i) - 1L));
                }
            }
        }

        return _encode;
    }

    private COSArray GetDecodeValues()
    {
        _decode ??= GetCOSObject().GetCOSArray(COSName.GetPDFName("Decode")) ?? GetRangeValues() ?? new COSArray();
        return _decode;
    }

    private int[][] GetSamples()
    {
        if (_samples is not null)
        {
            return _samples;
        }

        int arraySize = 1;
        int nIn = GetNumberOfInputParameters();
        int nOut = GetNumberOfOutputParameters();
        COSArray sizes = GetSize();
        for (int i = 0; i < nIn; i++)
        {
            arraySize *= sizes.GetInt(i);
        }

        _samples = new int[arraySize][];
        for (int i = 0; i < arraySize; i++)
        {
            _samples[i] = new int[nOut];
        }

        using Stream input = GetPDStream()?.CreateInputStream() ?? Stream.Null;
        using BitStreamReader reader = new(input);
        int bitsPerSample = GetBitsPerSample();
        for (int i = 0; i < arraySize; i++)
        {
            for (int k = 0; k < nOut; k++)
            {
                _samples[i][k] = reader.ReadBits(bitsPerSample);
            }
        }

        return _samples;
    }

    private sealed class Rinterpol
    {
        private readonly PDFunctionType0 _owner;
        private readonly float[] _input;
        private readonly int[] _inputPrev;
        private readonly int[] _inputNext;
        private readonly int _numberOfOutputValues;

        public Rinterpol(PDFunctionType0 owner, float[] input, int[] inputPrev, int[] inputNext)
        {
            _owner = owner;
            _input = input;
            _inputPrev = inputPrev;
            _inputNext = inputNext;
            _numberOfOutputValues = owner.GetNumberOfOutputParameters();
        }

        public float[] Rinterpolate() => RinterpolRecursive(new int[_input.Length], 0);

        private float[] RinterpolRecursive(int[] coord, int step)
        {
            float[] resultSample = new float[_numberOfOutputValues];
            if (step == _input.Length - 1)
            {
                if (_inputPrev[step] == _inputNext[step])
                {
                    coord[step] = _inputPrev[step];
                    int[] tmpSample = _owner.GetSamples()[CalcSampleIndex(coord)];
                    for (int i = 0; i < _numberOfOutputValues; i++)
                    {
                        resultSample[i] = tmpSample[i];
                    }
                    return resultSample;
                }

                coord[step] = _inputPrev[step];
                int[] sample1 = _owner.GetSamples()[CalcSampleIndex(coord)];
                coord[step] = _inputNext[step];
                int[] sample2 = _owner.GetSamples()[CalcSampleIndex(coord)];
                for (int i = 0; i < _numberOfOutputValues; i++)
                {
                    resultSample[i] = _owner.Interpolate(_input[step], _inputPrev[step], _inputNext[step], sample1[i], sample2[i]);
                }

                return resultSample;
            }

            if (_inputPrev[step] == _inputNext[step])
            {
                coord[step] = _inputPrev[step];
                return RinterpolRecursive(coord, step + 1);
            }

            coord[step] = _inputPrev[step];
            float[] first = RinterpolRecursive(coord, step + 1);
            coord[step] = _inputNext[step];
            float[] second = RinterpolRecursive(coord, step + 1);
            for (int i = 0; i < _numberOfOutputValues; i++)
            {
                resultSample[i] = _owner.Interpolate(_input[step], _inputPrev[step], _inputNext[step], first[i], second[i]);
            }

            return resultSample;
        }

        private int CalcSampleIndex(int[] vector)
        {
            float[] sizeValues = _owner.GetSize().ToFloatArray();
            int index = 0;
            int sizeProduct = 1;
            for (int i = vector.Length - 2; i >= 0; i--)
            {
                sizeProduct *= (int)sizeValues[i];
            }

            for (int i = vector.Length - 1; i >= 0; i--)
            {
                index += sizeProduct * vector[i];
                if (i - 1 >= 0)
                {
                    sizeProduct /= (int)sizeValues[i - 1];
                }
            }

            return index;
        }
    }

    private sealed class BitStreamReader : IDisposable
    {
        private readonly Stream _stream;
        private int _currentByte = -1;
        private int _bitsRemaining;

        public BitStreamReader(Stream stream)
        {
            _stream = stream;
        }

        public int ReadBits(int bitCount)
        {
            uint result = 0;
            for (int i = 0; i < bitCount; i++)
            {
                if (_bitsRemaining == 0)
                {
                    _currentByte = _stream.ReadByte();
                    if (_currentByte < 0)
                    {
                        throw new EndOfStreamException();
                    }
                    _bitsRemaining = 8;
                }

                result <<= 1;
                result |= (uint)((_currentByte >> (_bitsRemaining - 1)) & 1);
                _bitsRemaining--;
            }

            return unchecked((int)result);
        }

        public void Dispose()
        {
        }
    }
}
