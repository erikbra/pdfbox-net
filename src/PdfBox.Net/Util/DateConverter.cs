/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/DateConverter.java
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

/*
 * Date format is described in PDF Reference 1.7 section 3.8.2
 * (www.adobe.com/devnet/acrobat/pdfs/pdf_reference_1-7.pdf)
 * and also in PDF 32000-1:2008
 * (http://www.adobe.com/devnet/acrobat/pdfs/PDF32000_2008.pdf))
 * although the latter inexplicably omits the trailing apostrophe.
 *
 * The interpretation of dates without timezones is unclear.
 * The code below assumes that such dates are in UTC+00 (aka GMT).
 * This is in keeping with the PDF Reference's assertion that:
 *      numerical fields default to zero values.
 * However, the Reference does go on to make the cryptic remark:
 *      If no UT information is specified, the relationship of the specified
 *      time to UT is considered to be unknown. Whether or not the time
 *      zone is known, the rest of the date should be specified in local time.
 * I understand this to refer to _creating_ a pdf date value. That is,
 * code that can get the wall clock time and cannot get the timezone
 * should write the wall clock time with a time zone of zero.
 * When _parsing_ a PDF date, the statement talks about "the rest of the date"
 * being local time, thus explicitly excluding the use of the local time
 * for the time zone.
*/

using System.Globalization;
using PdfBox.Net.COS;

namespace PdfBox.Net.Util;

/// <summary>
/// Converts dates to strings and back using the PDF date standard
/// in section 3.8.2 of PDF Reference 1.7.
/// </summary>
/// <remarks>
/// Adaptation notes: Java's <c>Calendar</c>/<c>GregorianCalendar</c>/<c>TimeZone</c> are replaced
/// by the custom <see cref="GregorianCalendar"/> and <see cref="SimpleTimeZone"/> helper classes
/// that preserve the original Java semantics needed by DateConverter.
/// Authors: Ben Litchfield, Fred Hansen
/// </remarks>
public static class DateConverter
{
    // milliseconds/1000 = seconds; seconds / 60 = minutes; minutes/60 = hours
    private const int MinutesPerHour = 60;
    private const int SecondsPerMinute = 60;
    private const int MillisPerMinute = SecondsPerMinute * 1000;
    private const int MillisPerHour = MinutesPerHour * MillisPerMinute;
    private const long HalfDay = 12L * MinutesPerHour * MillisPerMinute;
    private const long Day = 2 * HalfDay;

    /*
     * The Date format is supposed to be the PDF_DATE_FORMAT, but other
     * forms appear. These lists offer alternatives to be tried
     * if ParseBigEndianDate fails.
     *
     * The time zone offset generally trails the date string, so it is processed
     * separately with ParseTZoffset.
     *
     * Format codes follow Java SimpleDateFormat with equivalent .NET mappings:
     *   EEEE/EEE = dddd/ddd (day-of-week)
     *   yyyy/yy  = yyyy/yy  (year)
     *   MMMM/MMM/MM/M = MMMM/MMM/MM/M (month)
     *   dd/d     = dd/d     (day)
     *   HH/H     = HH/H     (24-hour)
     *   hh/h     = hh/h     (12-hour, needs tt)
     *   mm       = mm       (minute)
     *   ss       = ss       (second)
     *   a        = tt       (AM/PM)
     *   z        = timezone (handled separately)
     */
    private static readonly string[] AlphaStartFormats =
    {
        "dddd, dd MMM yyyy hh:mm:ss tt",
        "dddd, dd MMM yy hh:mm:ss tt",
        "dddd, MMM dd, yyyy hh:mm:ss tt",
        "dddd, MMM dd, yy hh:mm:ss tt",
        "dddd, MMM dd, yyyy 'at' hh:mmtt",   // Acrobat Net Distiller 1.0 for Windows
        "dddd, MMM dd, yy 'at' hh:mmtt",
        "dddd, MMM dd, yyyy",                  // Acrobat Distiller 1.0.2 for Macintosh && PDFBOX-465
        "dddd, MMM dd, yy",
        "dddd MMM dd, yyyy HH:mm:ss",          // ECMP5
        "dddd MMM dd, yy HH:mm:ss",
        "dddd MMM dd HH:mm:ss zzz yyyy",       // GNU Ghostscript 7.0.7
        "dddd MMM dd HH:mm:ss zzz yy",
        "dddd MMM dd HH:mm:ss yyyy",           // GNU Ghostscript 7.0.7 variant
        "dddd MMM dd HH:mm:ss yy",
    };

    private static readonly string[] DigitStartFormats =
    {
        "dd MMM yyyy HH:mm:ss",   // for 26 May 2000 11:25:00
        "dd MMM yy HH:mm:ss",
        "dd MMM yyyy HH:mm",      // for 26 May 2000 11:25
        "dd MMM yy HH:mm",
        "yyyy MMM d",              // ambiguity resolved only by omitting time
        "yyyyMMddHH:mm:ss",        // test case "200712172:2:3"
        "H:m M/d/yyyy",            // test case "9:47 5/12/2008"
        "H:m M/d/yy",
        "M/d/yyyy HH:mm:ss",
        "M/d/yy HH:mm:ss",
        "M/d/yyyy HH:mm",
        "M/d/yy HH:mm",
        "M/d/yyyy",
        "M/d/yy",
    };

    /// <summary>
    /// Converts a Calendar to a string formatted as:
    ///     D:yyyyMMddHHmmss#hh'mm'  where # is Z, +, or -.
    /// </summary>
    /// <param name="cal">The date to convert to a string. May be null.
    /// The DST_OFFSET is included when computing the output time zone.</param>
    /// <returns>The date as a String to be used in a PDF document,
    ///     or null if the cal value is null</returns>
    public static string? ToString(GregorianCalendar? cal)
    {
        if (cal == null)
        {
            return null;
        }
        string offset = FormatTZoffset(cal.Get(GregorianCalendar.ZoneOffset) +
                                        cal.Get(GregorianCalendar.DstOffset), "'");
        return string.Format(CultureInfo.InvariantCulture,
            "D:{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6}",
            cal.Get(GregorianCalendar.Year),
            cal.Get(GregorianCalendar.Month) + 1,    // PDF months are 1-based
            cal.Get(GregorianCalendar.DayOfMonth),
            cal.Get(GregorianCalendar.HourOfDay),
            cal.Get(GregorianCalendar.Minute),
            cal.Get(GregorianCalendar.Second),
            offset + "'");
    }

    /// <summary>
    /// Converts the date to ISO 8601 string format:
    ///     yyyy-mm-ddThh:MM:ss#hh:mm    (where '#" is '+' or '-').
    /// </summary>
    /// <param name="cal">The date to convert.  Must not be null.
    /// The DST_OFFSET is included in the output value.</param>
    /// <returns>The date represented as an ISO 8601 string.</returns>
    public static string ToISO8601(GregorianCalendar cal)
    {
        string offset = FormatTZoffset(cal.Get(GregorianCalendar.ZoneOffset) +
                                        cal.Get(GregorianCalendar.DstOffset), ":");
        return string.Format(CultureInfo.InvariantCulture,
            "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}{6}",
            cal.Get(GregorianCalendar.Year),
            cal.Get(GregorianCalendar.Month) + 1,
            cal.Get(GregorianCalendar.DayOfMonth),
            cal.Get(GregorianCalendar.HourOfDay),
            cal.Get(GregorianCalendar.Minute),
            cal.Get(GregorianCalendar.Second),
            offset);
    }

    /*
     * Constrain a timezone offset to the range [-14:00 thru +14:00].
     * by adding or subtracting multiples of a full day.
     */
    private static int RestrainTZoffset(long proposedOffset)
    {
        if (proposedOffset <= 14 * MillisPerHour && proposedOffset >= -14 * MillisPerHour)
        {
            // https://www.w3.org/TR/xmlschema-2/#dateTime-timezones
            // Timezones between 14:00 and -14:00 are valid
            return (int)proposedOffset;
        }
        // Constrain a timezone offset to the range  [-11:59 thru +12:00].
        proposedOffset = ((proposedOffset + HalfDay) % Day + Day) % Day;
        if (proposedOffset == 0)
        {
            return (int)HalfDay;
        }
        // 0 <= proposedOffset < DAY
        proposedOffset = (proposedOffset - HalfDay) % HalfDay;
        // -HALF_DAY < proposedOffset < HALF_DAY
        return (int)proposedOffset;
    }

    /*
     * Formats a time zone offset as #hh^mm
     * where # is + or -, hh is hours, ^ is a separator, and mm is minutes.
     * Any separator may be specified by the second argument;
     * the usual values are ":" (ISO 8601), "" (RFC 822), and "'" (PDF).
     * The returned value is constrained to the range -11:59 ... 11:59.
     * For offset of 0 millis, the String returned is "+00^00", never "Z".
     *
     * package-private for testing
     */
    internal static string FormatTZoffset(long millis, string sep)
    {
        int constrained = RestrainTZoffset(millis);
        char sign = constrained >= 0 ? '+' : '-';
        int abs = Math.Abs(constrained);
        int hh = abs / MillisPerHour;
        int mm = (abs % MillisPerHour) / MillisPerMinute;
        return string.Format(CultureInfo.InvariantCulture, "{0}{1:D2}{2}{3:D2}", sign, hh, sep, mm);
    }

    /*
     * Parses an integer from a string, starting at and advancing a ParsePosition.
     * Returns The integer that was at the given parse position, or the remedy value
     * if no digits were found.
     */
    private static int ParseTimeField(string text, ParsePosition where, int maxlen, int remedy)
    {
        if (text == null)
        {
            return remedy;
        }
        int retval = 0;
        int index = where.Index;
        int limit = index + Math.Min(maxlen, text.Length - index);
        for (; index < limit; index++)
        {
            int cval = text[index] - '0';
            if (cval < 0 || cval > 9)
            {
                break;
            }
            retval = retval * 10 + cval;
        }
        if (index == where.Index)
        {
            return remedy;
        }
        where.Index = index;
        return retval;
    }

    /*
     * Advances the ParsePosition past any and all the characters that match
     * those in the optionals list. Returns the last non-space character passed over.
     */
    private static char SkipOptionals(string text, ParsePosition where, string optionals)
    {
        char retval = ' ';
        while (where.Index < text.Length &&
               optionals.IndexOf(text[where.Index]) >= 0)
        {
            char currch = text[where.Index];
            retval = (currch != ' ') ? currch : retval;
            where.Index++;
        }
        return retval;
    }

    /*
     * If the victim string is at the given position in the text, this method
     * advances the position past that string.
     */
    private static bool SkipString(string text, string victim, ParsePosition where)
    {
        if (where.Index + victim.Length <= text.Length &&
            text.Substring(where.Index, victim.Length) == victim)
        {
            where.Index += victim.Length;
            return true;
        }
        return false;
    }

    private static bool SkipStringExact(string text, string victim, ParsePosition where)
    {
        if (where.Index + victim.Length <= text.Length &&
            text.Substring(where.Index, victim.Length) == victim)
        {
            where.Index += victim.Length;
            return true;
        }
        return false;
    }

    /*
     * Construct a new GregorianCalendar and set defaults.
     * Locale is ENGLISH.
     * TimeZone is "UTC" (zero offset and no DST).
     * Parsing is NOT lenient. Milliseconds are zero.
     *
     * package-private for testing
     */
    internal static GregorianCalendar NewGreg()
    {
        GregorianCalendar retCal = new GregorianCalendar(new SimpleTimeZone(0, "UTC"));
        retCal.SetLenient(false);
        retCal.Set(GregorianCalendar.Millisecond, 0);
        return retCal;
    }

    /*
     * Install a TimeZone on a GregorianCalendar without changing the
     * hours value. A plain GregorianCalendar.setTimeZone()
     * adjusts the Calendar.HOUR value to compensate. This is *BAD*
     * (not to say *EVIL*) when we have already set the time.
     */
    private static void AdjustTimeZoneNicely(GregorianCalendar cal, SimpleTimeZone tz)
    {
        cal.SetTimeZone(tz);
        int offset = (cal.Get(GregorianCalendar.ZoneOffset) + cal.Get(GregorianCalendar.DstOffset)) /
                MillisPerMinute;
        cal.Add(GregorianCalendar.Minute, -offset);
    }

    /*
     * Parses the end of a date string for a time zone and, if one is found,
     * sets the time zone of the GregorianCalendar. Otherwise the calendar
     * time zone is unchanged.
     *
     * package-private for testing
     */
    internal static bool ParseTZoffset(string text, GregorianCalendar cal,
                                        ParsePosition initialWhere)
    {
        ParsePosition where = new ParsePosition(initialWhere.Index);
        SimpleTimeZone tz = new SimpleTimeZone(0, "GMT");
        int tzHours, tzMin;
        char sign = SkipOptionals(text, where, "Z+- ");
        bool hadGMT = (sign == 'Z' || SkipStringExact(text, "GMT", where) ||
                       SkipStringExact(text, "UTC", where));
        sign = (!hadGMT) ? sign : SkipOptionals(text, where, "+- ");

        tzHours = ParseTimeField(text, where, 2, -999);
        SkipOptionals(text, where, "': ");
        tzMin = ParseTimeField(text, where, 2, 0);
        SkipOptionals(text, where, "' ");

        if (tzHours != -999)
        {
            // we parsed a time zone in default format
            int hrSign = (sign == '-' ? -1 : 1);
            int rawOffset = RestrainTZoffset(hrSign * (tzHours * (long)MillisPerHour +
                                                       tzMin * (long)MillisPerMinute));
            tz.SetRawOffset(rawOffset);
            UpdateZoneId(tz);
        }
        else if (!hadGMT)
        {
            // try to process as a name; "GMT" or "UTC" has already been processed
            string tzText = text.Substring(initialWhere.Index).Trim();
            SimpleTimeZone? namedTz = GetTimeZoneByName(tzText);
            // getTimeZone returns "GMT" for unknown ids
            if (namedTz == null || namedTz.GetId() == "GMT")
            {
                // no timezone in text, cal and initialWhere are unchanged
                return false;
            }
            else
            {
                // we got a tz by name; use it
                tz = namedTz;
                where.Index = text.Length;
            }
        }
        AdjustTimeZoneNicely(cal, tz);
        initialWhere.Index = where.Index;
        return true;
    }

    /// <summary>
    /// Update the zone ID based on the raw offset. This is either GMT, GMT+hh:mm or GMT-hh:mm.
    /// </summary>
    private static void UpdateZoneId(SimpleTimeZone tz)
    {
        int offset = tz.GetRawOffset();
        char pm = '+';
        if (offset < 0)
        {
            pm = '-';
            offset = -offset;
        }
        int hh = offset / 3600000;
        int mm = offset % 3600000 / 60000;
        if (offset == 0)
        {
            tz.SetId("GMT");
        }
        else if (pm == '+' && hh <= 12)
        {
            tz.SetId(string.Format(CultureInfo.InvariantCulture, "GMT+{0:D2}:{1:D2}", hh, mm));
        }
        else if (pm == '-' && hh <= 14)
        {
            tz.SetId(string.Format(CultureInfo.InvariantCulture, "GMT-{0:D2}:{1:D2}", hh, mm));
        }
        else
        {
            tz.SetId("unknown");
        }
    }

    /*
     * Tries to find a timezone by name (IANA or abbreviation).
     * Returns null if the timezone is not recognized.
     * Returns a SimpleTimeZone with ID="GMT" if the name is genuinely unknown.
     */
    private static SimpleTimeZone? GetTimeZoneByName(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        // Handle well-known abbreviations that Java treats as valid
        // (Java returns "GMT" with ID="GMT" for unknowns like "EDT")
        switch (id.ToUpperInvariant())
        {
            case "PST": return new SimpleTimeZone(-8 * MillisPerHour, "PST");
            case "MST": return new SimpleTimeZone(-7 * MillisPerHour, "MST");
            case "CST": return new SimpleTimeZone(-6 * MillisPerHour, "CST");
            case "EST": return new SimpleTimeZone(-5 * MillisPerHour, "EST");
            case "GMT": return new SimpleTimeZone(0, "GMT");
            case "UTC": return new SimpleTimeZone(0, "UTC");
            // Java treats EDT, CDT, MDT, PDT as unknown → return GMT
            case "EDT":
            case "CDT":
            case "MDT":
            case "PDT":
                return new SimpleTimeZone(0, "GMT");
        }

        // Try IANA timezone name using .NET TimeZoneInfo
        try
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(id);
            // Use the base UTC offset (standard, no DST)
            int rawOffsetMs = (int)tzInfo.BaseUtcOffset.TotalMilliseconds;
            string tzId = id;
            return new SimpleTimeZone(rawOffsetMs, tzId);
        }
        catch (TimeZoneNotFoundException)
        {
            // unknown: return GMT (ID="GMT") so parseTZoffset sees it as not found
            return new SimpleTimeZone(0, "GMT");
        }
    }

    /*
     * Parses a big-endian date: year month day hour min sec.
     * The year must be four digits. Other fields may be adjacent
     * and delimited by length or they may follow appropriate delimiters.
     *     year [ -/]* month [ -/]* dayofmonth [ T]* hour [:] min [:] sec [.secFraction]
     * If any numeric field is omitted, all following fields must also be omitted.
     * No time zone is processed.
     */
    private static GregorianCalendar? ParseBigEndianDate(string text,
            ParsePosition initialWhere)
    {
        ParsePosition where = new ParsePosition(initialWhere.Index);
        int year = ParseTimeField(text, where, 4, 0);
        if (where.Index != 4 + initialWhere.Index)
        {
            return null;
        }
        SkipOptionals(text, where, "/- ");
        int month = ParseTimeField(text, where, 2, 1) - 1; // Calendar months are 0...11
        SkipOptionals(text, where, "/- ");
        int day = ParseTimeField(text, where, 2, 1);
        SkipOptionals(text, where, " T");
        int hour = ParseTimeField(text, where, 2, 0);
        SkipOptionals(text, where, ": ");
        int minute = ParseTimeField(text, where, 2, 0);
        SkipOptionals(text, where, ": ");
        int second = ParseTimeField(text, where, 2, 0);
        char nextC = SkipOptionals(text, where, ".");
        if (nextC == '.')
        {
            // fractions of a second: skip up to 19 digits
            ParseTimeField(text, where, 19, 0);
        }

        GregorianCalendar dest = NewGreg();
        try
        {
            dest.Set(year, month, day, hour, minute, second);
            // trigger limit tests
            _ = dest.GetTimeInMillis();
        }
        catch (ArgumentException)
        {
            return null;
        }
        initialWhere.Index = where.Index;
        SkipOptionals(text, initialWhere, " ");
        // dest has at least a year value
        return dest;
    }

    /*
     * See if text can be parsed as a date according to any of a list of
     * formats. The time zone may be included as part of the format, or
     * omitted in favor of later testing for a trailing time zone.
     */
    private static GregorianCalendar? ParseSimpleDate(string text, string[] fmts,
            ParsePosition initialWhere)
    {
        foreach (string fmt in fmts)
        {
            ParsePosition where = new ParsePosition(initialWhere.Index);
            GregorianCalendar? retCal = TryParseWithFormat(text.Substring(where.Index), fmt, where);
            if (retCal != null)
            {
                initialWhere.Index = where.Index;
                SkipOptionals(text, initialWhere, " ");
                return retCal;
            }
        }
        return null;
    }

    /*
     * Try to parse a date string with the given format.
     * Returns null if parsing fails.
     */
    private static GregorianCalendar? TryParseWithFormat(string text, string fmt, ParsePosition where)
    {
        // Map Java SimpleDateFormat patterns to .NET DateTime parse format strings
        string dotnetFmt = ConvertJavaFormatToDotNet(fmt);

        // Trim leading whitespace
        string trimmed = text.TrimStart();
        int trimmedOffset = text.Length - trimmed.Length;

        if (DateTime.TryParseExact(trimmed, dotnetFmt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite,
                out DateTime parsed))
        {
            // Check that parsed date is valid (non-lenient)
            GregorianCalendar retCal = NewGreg();
            try
            {
                // DateTime months are 1-based; GregorianCalendar months are 0-based
                retCal.Set(parsed.Year, parsed.Month - 1, parsed.Day,
                           parsed.Hour, parsed.Minute, parsed.Second);
                // Validate
                _ = retCal.GetTimeInMillis();
            }
            catch (ArgumentException)
            {
                return null;
            }
            // Advance the where position by the full trimmed text length
            // (simple date parsers consume the whole pattern)
            where.Index += trimmedOffset + trimmed.Length;
            return retCal;
        }
        return null;
    }

    /*
     * Convert a Java SimpleDateFormat pattern string to a .NET DateTime format string.
     */
    private static string ConvertJavaFormatToDotNet(string javaFmt)
    {
        // Map known Java format tokens to .NET equivalents
        return javaFmt
            .Replace("EEEEEEEEEE", "dddd")   // long day-of-week (any repetition > 4)
            .Replace("EEEEEE", "dddd")
            .Replace("EEEE", "dddd")
            .Replace("EEE", "ddd")
            .Replace("EE", "ddd")
            .Replace("E,", "ddd,")
            // 'at' literal is the same in .NET
            // Java hh:mm:ss a → .NET hh:mm:ss tt (12-hour with AM/PM)
            .Replace(" a", " tt")
            .Replace("a", "tt")
            // year
            .Replace("yyyy", "yyyy")
            .Replace("yy", "yy")
            // Java 'H' is 24-hour same as .NET 'H'
            // Java 'h' is 12-hour same as .NET 'h'
            ;
    }

    /*
     * Parses a String to see if it begins with a date, and if so,
     * returns that date. The date must be strictly correct.
     */
    private static GregorianCalendar? ParseDate(string text, ParsePosition initialWhere)
    {
        if (text == null || text.Length == 0 || "D:".Equals(text.Trim()))
        {
            return null;
        }

        // remember longest date string
        int longestLen = int.MinValue;

        GregorianCalendar? longestDate = null;
        int whereLen;

        ParsePosition where = new ParsePosition(initialWhere.Index);
        // check for null (throws exception) and trim off surrounding spaces
        SkipOptionals(text, where, " ");
        int startPosition = where.Index;

        // try big-endian parse
        GregorianCalendar? retCal = ParseBigEndianDate(text, where);
        // check for success and a timezone
        if (retCal != null && (where.Index == text.Length ||
                               ParseTZoffset(text, retCal, where)))
        {
            // if text is fully consumed, return the date else remember it and its length
            whereLen = where.Index;
            if (whereLen == text.Length)
            {
                initialWhere.Index = whereLen;
                return retCal;
            }
            longestLen = whereLen;
            longestDate = retCal;
        }

        // try one of the sets of standard formats
        where.Index = startPosition;
        string[] formats = (startPosition < text.Length && char.IsDigit(text[startPosition]))
                ? DigitStartFormats
                : AlphaStartFormats;
        retCal = ParseSimpleDate(text, formats, where);
        // check for success and a timezone
        if (retCal != null &&
                (where.Index == text.Length ||
                 ParseTZoffset(text, retCal, where)))
        {
            // if text is fully consumed, return the date else remember it and its length
            whereLen = where.Index;
            if (whereLen == text.Length)
            {
                initialWhere.Index = whereLen;
                return retCal;
            }
            if (whereLen > longestLen)
            {
                longestLen = whereLen;
                longestDate = retCal;
            }
        }

        if (longestDate != null)
        {
            initialWhere.Index = longestLen;
            return longestDate;
        }
        return retCal;
    }

    /// <summary>
    /// Returns the Calendar for a given COS string containing a date,
    /// or <c>null</c> if it cannot be parsed.
    ///
    /// The returned value will have 0 for DST_OFFSET.
    /// </summary>
    /// <param name="text">A COS string containing a date.</param>
    /// <returns>The Calendar that the text string represents, or <c>null</c> if it cannot be parsed.</returns>
    public static GregorianCalendar? ToCalendar(COSString? text)
    {
        if (text == null)
        {
            return null;
        }
        return ToCalendar(text.GetString());
    }

    /// <summary>
    /// Returns the Calendar for a given string containing a date,
    /// or <c>null</c> if it cannot be parsed.
    ///
    /// The returned value will have 0 for DST_OFFSET.
    /// </summary>
    /// <param name="text">A COS string containing a date.</param>
    /// <returns>The Calendar that the text string represents, or <c>null</c> if it cannot be parsed.</returns>
    public static GregorianCalendar? ToCalendar(string? text)
    {
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        ParsePosition where = new ParsePosition(0);
        SkipOptionals(text, where, " ");
        SkipStringExact(text, "D:", where);
        GregorianCalendar? calendar = ParseDate(text, where);

        if (calendar == null || where.Index != text.Length)
        {
            // the date string is invalid
            return null;
        }
        return calendar;
    }
}
