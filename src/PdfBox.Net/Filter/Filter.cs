/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted minimal filter base required by COSInputStream/COSOutputStream.
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
using PdfBox.Net.IO;

namespace PdfBox.Net.Filter;

public abstract class Filter
{
    public const string SyspropDeflateLevel = "org.apache.pdfbox.filter.deflatelevel";

    public abstract DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options);

    public abstract void Encode(Stream input, Stream output, COSDictionary parameters, int index);

    public static RandomAccessRead Decode(Stream input, IList<Filter> filters, COSDictionary parameters,
        DecodeOptions options, IList<DecodeResult> results)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(results);

        if (filters.Count == 0)
        {
            RandomAccessReadWriteBuffer passthrough = new();
            CopyTo(input, passthrough);
            passthrough.Seek(0);
            return passthrough;
        }

        Stream currentInput = input;
        RandomAccessReadWriteBuffer? currentBuffer = null;
        for (int i = 0; i < filters.Count; i++)
        {
            Filter filter = filters[i];
            RandomAccessReadWriteBuffer nextBuffer = new();
            using (RandomAccessOutputStream output = new(nextBuffer))
            {
                DecodeResult result = filter.Decode(currentInput, output, parameters, i, options);
                results.Add(result);
            }

            if (!ReferenceEquals(currentInput, input))
            {
                currentInput.Dispose();
            }

            currentBuffer?.Close();
            currentBuffer = nextBuffer;
            currentBuffer.Seek(0);
            currentInput = new RandomAccessInputStream(currentBuffer);
        }

        if (!ReferenceEquals(currentInput, input))
        {
            currentInput.Dispose();
        }

        currentBuffer ??= new RandomAccessReadWriteBuffer();
        currentBuffer.Seek(0);
        return currentBuffer;
    }

    protected COSDictionary GetDecodeParams(COSDictionary dictionary, int index)
    {
        COSBase? filter = dictionary.GetDictionaryObject(COSName.F, COSName.FILTER);
        COSBase? decodeParams = dictionary.GetDictionaryObject(COSName.DP, COSName.DECODE_PARMS);
        if (filter is COSName && decodeParams is COSDictionary asDictionary)
        {
            return asDictionary;
        }

        if (filter is COSArray && decodeParams is COSArray asArray && index < asArray.Size())
        {
            if (asArray.GetObject(index) is COSDictionary decodeParamDictionary)
            {
                return decodeParamDictionary;
            }
        }

        return new COSDictionary();
    }

    protected static int GetCompressionLevel()
    {
        string? value = Environment.GetEnvironmentVariable(SyspropDeflateLevel);
        if (value is null || !int.TryParse(value, out int level))
        {
            return -1;
        }

        return Math.Clamp(level, -1, 9);
    }

    private static void CopyTo(Stream input, RandomAccessReadWriteBuffer output)
    {
        byte[] buffer = new byte[8192];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
}
