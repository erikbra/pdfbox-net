/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# DateTimeOffset parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/DateConverter.java
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


using System.Globalization;
using System.Text.RegularExpressions;

namespace PdfBox.Net.XmpBox;

public static class DateConverter
{
    private static readonly Regex IsoPrefix = new("^\\d{4}-\\d{2}-\\d{2}T.*", RegexOptions.Compiled);

    public static DateTimeOffset ToCalendar(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            throw new FormatException("Error: Invalid date format");
        }

        date = date.Trim();

        int month = 1;
        int day = 1;
        int hour = 0;
        int minute = 0;
        int second = 0;

        try
        {
            if (IsoPrefix.IsMatch(date))
            {
                return FromIso8601(date);
            }

            if (date.StartsWith("D:", StringComparison.Ordinal))
            {
                date = date[2..];
            }

            int posOfT = date.IndexOf('T');
            if (posOfT != 10 && posOfT != -1)
            {
                throw new FormatException($"Error converting date:{date}");
            }

            date = Regex.Replace(date, "[-:T]", string.Empty);
            if (date.Length < 4)
            {
                throw new FormatException($"Error: Invalid date format '{date}'");
            }

            int year = int.Parse(date[..4], CultureInfo.InvariantCulture);
            if (date.Length >= 6)
            {
                month = int.Parse(date[4..6], CultureInfo.InvariantCulture);
            }

            if (date.Length >= 8)
            {
                day = int.Parse(date[6..8], CultureInfo.InvariantCulture);
            }

            if (date.Length >= 10)
            {
                hour = int.Parse(date[8..10], CultureInfo.InvariantCulture);
            }

            if (date.Length >= 12)
            {
                minute = int.Parse(date[10..12], CultureInfo.InvariantCulture);
            }

            int timeZonePos = 12;
            if (date.Length == 14 || date.Length - 12 > 5 || (date.Length - 12 == 3 && date.EndsWith("Z", StringComparison.Ordinal)))
            {
                second = int.Parse(date[12..14], CultureInfo.InvariantCulture);
                timeZonePos = 14;
            }

            TimeSpan? offset = null;
            if (date.Length >= timeZonePos + 1)
            {
                char sign = date[timeZonePos];
                if (sign == 'Z')
                {
                    offset = TimeSpan.Zero;
                }
                else
                {
                    int hours = 0;
                    int minutes = 0;
                    if (date.Length >= timeZonePos + 3)
                    {
                        hours = sign == '+'
                            ? int.Parse(date.Substring(timeZonePos + 1, 2), CultureInfo.InvariantCulture)
                            : -int.Parse(date.Substring(timeZonePos + 1, 2), CultureInfo.InvariantCulture);
                    }

                    if (sign == '+')
                    {
                        if (date.Length >= timeZonePos + 5)
                        {
                            minutes = int.Parse(date.Substring(timeZonePos + 3, 2), CultureInfo.InvariantCulture);
                        }
                    }
                    else if (date.Length >= timeZonePos + 5)
                    {
                        minutes = -int.Parse(date.Substring(timeZonePos + 3, 2), CultureInfo.InvariantCulture);
                    }

                    offset = new TimeSpan(hours, minutes, 0);
                }
            }

            DateTime unspecified = new(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            TimeSpan actualOffset = offset ?? TimeZoneInfo.Local.GetUtcOffset(unspecified);
            return new DateTimeOffset(unspecified, actualOffset);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or FormatException)
        {
            throw new FormatException($"Error converting date:{date}", ex);
        }
    }

    public static string ToISO8601(DateTimeOffset cal)
    {
        return ToISO8601(cal, false);
    }

    public static string ToISO8601(DateTimeOffset cal, bool printMillis)
    {
        string baseValue = cal.ToString(printMillis ? "yyyy-MM-dd'T'HH:mm:ss.fff" : "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        TimeSpan offset = cal.Offset;
        char sign = offset < TimeSpan.Zero ? '-' : '+';
        offset = offset.Duration();
        return $"{baseValue}{sign}{offset.Hours:00}:{offset.Minutes:00}";
    }

    private static DateTimeOffset FromIso8601(string dateString)
    {
        if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset zonedDateTime))
        {
            return zonedDateTime;
        }

        DateTime localDateTime = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        return new DateTimeOffset(localDateTime, TimeSpan.Zero);
    }
}
