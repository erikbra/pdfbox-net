/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSInteger.java
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

using System.Globalization;
using System.IO;
using System.Text;

namespace PdfBox.Net.COS;

/// <summary>
/// This class represents an integer number in a PDF document.
/// </summary>
public sealed class COSInteger : COSNumber
{
    private const int LOW = -100;
    private const int HIGH = 256;
    private static readonly COSInteger?[] STATIC = new COSInteger[HIGH - LOW + 1];

    public static readonly COSInteger ZERO = Get(0);
    public static readonly COSInteger ONE = Get(1);
    public static readonly COSInteger TWO = Get(2);
    public static readonly COSInteger THREE = Get(3);

    internal static readonly COSInteger OUT_OF_RANGE_MAX = GetInvalid(true);
    internal static readonly COSInteger OUT_OF_RANGE_MIN = GetInvalid(false);

    private readonly long _value;
    private readonly bool _isValid;

    private COSInteger(long value, bool valid)
    {
        _value = value;
        _isValid = valid;
    }

    public static COSInteger Get(long val)
    {
        if (LOW <= val && val <= HIGH)
        {
            int index = (int)val - LOW;
            if (STATIC[index] == null)
            {
                STATIC[index] = new COSInteger(val, true);
            }

            return STATIC[index]!;
        }

        return new COSInteger(val, true);
    }

    private static COSInteger GetInvalid(bool maxValue)
    {
        return maxValue ? new COSInteger(long.MaxValue, false) : new COSInteger(long.MinValue, false);
    }

    public override bool Equals(object? o)
    {
        return o is COSInteger other && other.IntValue() == IntValue();
    }

    public override int GetHashCode()
    {
        return (int)(_value ^ (_value >> 32));
    }

    public override string ToString()
    {
        return $"COSInt{{{_value}}}";
    }

    public override float FloatValue()
    {
        return _value;
    }

    public override int IntValue()
    {
        return (int)_value;
    }

    public override long LongValue()
    {
        return _value;
    }

    public bool IsValid()
    {
        return _isValid;
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromInt(this);
    }

    public void WritePDF(Stream output)
    {
        output.Write(Encoding.Latin1.GetBytes(_value.ToString(CultureInfo.InvariantCulture)));
    }
}
