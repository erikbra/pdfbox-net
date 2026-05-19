/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSFloat.java
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
using System.Text.RegularExpressions;

namespace PdfBox.Net.COS;

/// <summary>
/// This class represents a floating point number in a PDF document.
/// </summary>
public class COSFloat : COSNumber
{
    private const float MIN_NORMAL = 1.17549435E-38f;

    private readonly float _value;
    private string? _valueAsString;

    public static readonly COSFloat ZERO = new(0f, "0.0");
    public static readonly COSFloat ONE = new(1f, "1.0");

    public COSFloat(float aFloat)
    {
        _value = aFloat;
    }

    private COSFloat(float aFloat, string valueString)
    {
        _value = aFloat;
        _valueAsString = valueString;
    }

    public COSFloat(string aFloat)
    {
        float parsedValue;
        string? stringValue = null;
        try
        {
            float f = float.Parse(aFloat, CultureInfo.InvariantCulture);
            parsedValue = Coerce(f);
            stringValue = f == parsedValue ? aFloat : null;
        }
        catch (FormatException)
        {
            aFloat = FixMalformedInput(aFloat);
            try
            {
                parsedValue = Coerce(float.Parse(aFloat, CultureInfo.InvariantCulture));
            }
            catch (FormatException e)
            {
                throw new IOException($"Error expected floating point number actual='{aFloat}'", e);
            }
        }
        catch (OverflowException)
        {
            aFloat = FixMalformedInput(aFloat);
            try
            {
                parsedValue = Coerce(float.Parse(aFloat, CultureInfo.InvariantCulture));
            }
            catch (FormatException e)
            {
                throw new IOException($"Error expected floating point number actual='{aFloat}'", e);
            }
            catch (OverflowException e)
            {
                throw new IOException($"Error expected floating point number actual='{aFloat}'", e);
            }
        }

        _value = parsedValue;
        _valueAsString = stringValue;
    }

    private static string FixMalformedInput(string value)
    {
        if (value.StartsWith("--", StringComparison.Ordinal))
        {
            return value[1..];
        }

        if (Regex.IsMatch(value, "^0\\.0*-\\d+"))
        {
            int minusIndex = value.IndexOf('-', StringComparison.Ordinal);
            return minusIndex >= 0
                ? "-" + value.Remove(minusIndex, 1)
                : value;
        }

        if (Regex.IsMatch(value, "^-\\d+\\.-\\d+"))
        {
            return "-" + value.Replace("-", "", StringComparison.Ordinal);
        }

        throw new IOException($"Error expected floating point number actual='{value}'");
    }

    private static float Coerce(float floatValue)
    {
        if (floatValue == float.PositiveInfinity)
        {
            return float.MaxValue;
        }

        if (floatValue == float.NegativeInfinity)
        {
            return -float.MaxValue;
        }

        if (Math.Abs(floatValue) < MIN_NORMAL)
        {
            return 0f;
        }

        return floatValue;
    }

    public override float FloatValue()
    {
        return _value;
    }

    public override long LongValue()
    {
        return (long)_value;
    }

    public override int IntValue()
    {
        return (int)_value;
    }

    public override bool Equals(object? o)
    {
        return o is COSFloat other
               && BitConverter.SingleToInt32Bits(other._value) == BitConverter.SingleToInt32Bits(_value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return $"COSFloat{{{FormatString()}}}";
    }

    private string FormatString()
    {
        if (_valueAsString == null)
        {
            string s = _value.ToString("R", CultureInfo.InvariantCulture);
            _valueAsString = s.Contains('E', StringComparison.Ordinal)
                ? _value.ToString("0.################################################################################################################################################################################################################################################################", CultureInfo.InvariantCulture)
                : s;
        }

        return _valueAsString;
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromFloat(this);
    }

    public void WritePDF(Stream output)
    {
        output.Write(Encoding.Latin1.GetBytes(FormatString()));
    }
}
