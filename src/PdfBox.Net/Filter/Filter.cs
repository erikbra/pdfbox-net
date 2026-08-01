/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/Filter.java
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
using PdfBox.Net.IO;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.Filter;

public abstract class Filter
{
    private static ILogger<Filter> LOG => PdfBoxLogging.CreateLogger<Filter>();

    public const string SyspropDeflateLevel = "org.apache.pdfbox.filter.deflatelevel";
    public const string SyspropCcittFaxMaxBytes = "org.apache.pdfbox.filter.ccittmaxbytes";

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

        if (filters.Count > 1)
        {
            List<Filter> reducedFilters = [];
            foreach (Filter filter in filters)
            {
                if (!reducedFilters.Contains(filter))
                {
                    reducedFilters.Add(filter);
                }
            }
            if (reducedFilters.Count != filters.Count)
            {
                filters = reducedFilters;
                LOG.LogWarning("Removed duplicated filter entries");
            }
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
        else if (decodeParams is not null && filter is not COSArray && decodeParams is not COSArray)
        {
            LOG.LogError("Expected DecodeParams to be an Array or Dictionary but found {TypeName}",
                decodeParams.GetType().FullName);
        }

        return new COSDictionary();
    }

    protected static int GetCompressionLevel()
    {
        string? value = Environment.GetEnvironmentVariable(SyspropDeflateLevel);
        if (value is null)
        {
            return -1;
        }

        if (!int.TryParse(value, out int level))
        {
            FormatException exception = new($"The input string '{value}' was not in a correct format.");
            LOG.LogWarning(exception, "{Message}", exception.Message);
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
