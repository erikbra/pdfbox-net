/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSNumber.java
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
using System.Text.RegularExpressions;

namespace PdfBox.Net.COS;

/// <summary>
/// This class represents an abstract number in a PDF document.
/// </summary>
public abstract class COSNumber : COSBase
{
    public abstract float FloatValue();
    public abstract int IntValue();
    public abstract long LongValue();

    public static COSNumber Get(string number)
    {
        if (number.Length == 1)
        {
            char digit = number[0];
            if ('0' <= digit && digit <= '9')
            {
                return COSInteger.Get(digit - '0');
            }

            if (digit == '-' || digit == '.')
            {
                return COSInteger.ZERO;
            }

            throw new IOException($"Not a number: {number}");
        }

        if (IsFloat(number))
        {
            return new COSFloat(number);
        }

        try
        {
            return COSInteger.Get(long.Parse(number, CultureInfo.InvariantCulture));
        }
        catch (OverflowException)
        {
            return GetOutOfRangeInteger(number);
        }
        catch (FormatException)
        {
            return GetOutOfRangeInteger(number);
        }
    }

    private static COSNumber GetOutOfRangeInteger(string number)
    {
        string numberString = number.StartsWith('+') || number.StartsWith('-') ? number[1..] : number;
        if (!Regex.IsMatch(numberString, "^\\d*$"))
        {
            throw new IOException($"Not a number: {number}");
        }

        return number.StartsWith('-') ? COSInteger.OUT_OF_RANGE_MIN : COSInteger.OUT_OF_RANGE_MAX;
    }

    private static bool IsFloat(string number)
    {
        foreach (char digit in number)
        {
            if (digit == '.' || digit == 'e')
            {
                return true;
            }
        }

        return false;
    }
}
