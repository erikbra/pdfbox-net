/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSObjectKey.java
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

namespace PdfBox.Net.COS;

/// <summary>
/// Object representing the physical reference to an indirect pdf object.
/// </summary>
public sealed class COSObjectKey : IComparable<COSObjectKey>
{
    private const int NumberOffset = sizeof(short) * 8;
    private const long GenerationMask = (1L << NumberOffset) - 1;

    private readonly long _numberAndGeneration;
    private readonly int _streamIndex;

    public COSObjectKey(long num, int gen)
        : this(num, gen, -1)
    {
    }

    public COSObjectKey(long num, int gen, int index)
    {
        if (num < 0)
        {
            throw new ArgumentException("Object number must not be a negative value");
        }

        if (gen < 0)
        {
            throw new ArgumentException("Generation number must not be a negative value");
        }

        _numberAndGeneration = ComputeInternalHash(num, gen);
        _streamIndex = index;
    }

    public static long ComputeInternalHash(long num, int gen)
    {
        return (num << NumberOffset) | (gen & GenerationMask);
    }

    public long GetInternalHash()
    {
        return _numberAndGeneration;
    }

    public int GetGeneration()
    {
        return (int)(_numberAndGeneration & GenerationMask);
    }

    public long GetNumber()
    {
        return (long)((ulong)_numberAndGeneration >> NumberOffset);
    }

    public int GetStreamIndex()
    {
        return _streamIndex;
    }

    public int CompareTo(COSObjectKey? other)
    {
        return other is null ? 1 : _numberAndGeneration.CompareTo(other._numberAndGeneration);
    }

    public override bool Equals(object? obj)
    {
        return obj is COSObjectKey other && other._numberAndGeneration == _numberAndGeneration;
    }

    public override int GetHashCode()
    {
        return _numberAndGeneration.GetHashCode();
    }

    public override string ToString()
    {
        return $"{GetNumber()} {GetGeneration()} R";
    }
}
