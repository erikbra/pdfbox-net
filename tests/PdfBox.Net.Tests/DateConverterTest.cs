/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/util/TestDateUtil.java
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
using PdfBox.Net.Util;
using Xunit;

// Alias to avoid ambiguity with System.Globalization.GregorianCalendar
using PdfGregorianCalendar = PdfBox.Net.Util.GregorianCalendar;

namespace PdfBox.Net.Tests;

/*
 * Adaptation notes:
 * - Java Calendar.YEAR/MONTH/etc. constants map to PdfGregorianCalendar.Year/Month/etc. in C#
 * - Java Calendar months are 0-based (0=January); the C# PdfGregorianCalendar helper preserves this
 * - TimeZone.setDefault() has no .NET equivalent; tests that relied on it are adapted to use UTC
 * - assertCalendarEquals compares getTimeInMillis() and timezone raw offset
 * - PdfGregorianCalendar constructor(year, month0based, day) uses UTC by default in the C# helper
 */

/// <summary>
/// Tests for <see cref="DateConverter"/>.
/// </summary>
/// <remarks>Authors: Ben Litchfield, Fred Hansen</remarks>
public class DateConverterTest
{
    private const int Mins = 60 * 1000;
    private const int Hrs = 60 * Mins;
    /// <summary>expect parse fail</summary>
    private const int Bad = -666;

    private static void AssertCalendarEquals(PdfGregorianCalendar? expect, PdfGregorianCalendar? was)
    {
        Assert.NotNull(expect);
        Assert.NotNull(was);
        Assert.Equal(expect!.GetTimeInMillis(), was!.GetTimeInMillis());
        Assert.Equal(expect.GetTimeZone().GetRawOffset(), was.GetTimeZone().GetRawOffset());
    }

    /// <summary>
    /// Test common date formats.
    /// </summary>
    [Fact]
    public void TestExtract()
    {
        // D:05/12/2005 → year=2005, month=4(May 0-based), day=12, UTC
        AssertCalendarEquals(
            new PdfGregorianCalendar(2005, 4, 12),
            DateConverter.ToCalendar("D:05/12/2005"));

        // 5/12/2005 15:57:16 → year=2005, month=4, day=12, 15:57:16, UTC
        AssertCalendarEquals(
            new PdfGregorianCalendar(2005, 4, 12, 15, 57, 16),
            DateConverter.ToCalendar("5/12/2005 15:57:16"));

        // null arg returns null
        Assert.Null(DateConverter.ToCalendar((string?)null));
    }

    /// <summary>
    /// Test case for PDFBOX-598.
    /// </summary>
    [Fact]
    public void TestDateConversion()
    {
        PdfGregorianCalendar? c = DateConverter.ToCalendar("D:20050526205258+01'00'");
        Assert.NotNull(c);
        Assert.Equal(2005, c!.Get(PdfGregorianCalendar.Year));
        Assert.Equal(5 - 1, c.Get(PdfGregorianCalendar.Month));   // May = 4 (0-based)
        Assert.Equal(26, c.Get(PdfGregorianCalendar.DayOfMonth));
        Assert.Equal(20, c.Get(PdfGregorianCalendar.HourOfDay));
        Assert.Equal(52, c.Get(PdfGregorianCalendar.Minute));
        Assert.Equal(58, c.Get(PdfGregorianCalendar.Second));
        Assert.Equal(0, c.Get(PdfGregorianCalendar.Millisecond));
    }

    /// <summary>
    /// Verify checkParse helper for the given date string.
    /// </summary>
    private static void CheckParse(int yr, int mon, int day,
                int hr, int min, int sec, int offsetHours, int offsetMinutes,
                string orig)
    {
        string pdfDate = string.Format(CultureInfo.InvariantCulture,
            "D:{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6:+00;-00}{7:D2}'{8:D2}'",
            yr, mon, day, hr, min, sec, offsetHours, Math.Abs(offsetHours) == 0 && offsetMinutes == 0 ? 0 : 0, offsetMinutes);

        // Rebuild pdfDate properly
        string sign = offsetHours >= 0 ? "+" : "-";
        int absHrs = Math.Abs(offsetHours);
        pdfDate = string.Format(CultureInfo.InvariantCulture,
            "D:{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6}{7:D2}'{8:D2}'",
            yr, mon, day, hr, min, sec, sign, absHrs, offsetMinutes);

        string iso8601Date = string.Format(CultureInfo.InvariantCulture,
            "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}{6}{7:D2}:{8:D2}",
            yr, mon, day, hr, min, sec, sign, absHrs, offsetMinutes);

        PdfGregorianCalendar? cal = DateConverter.ToCalendar(orig);
        if (yr == Bad)
        {
            Assert.Null(cal);
        }
        else
        {
            Assert.NotNull(cal);
            Assert.Equal(iso8601Date, DateConverter.ToISO8601(cal!));
            Assert.Equal(pdfDate, DateConverter.ToString(cal!));
        }
    }

    /// <summary>
    /// Test dates in various formats.
    /// Years differ to make it easier to find failures.
    /// </summary>
    [Fact]
    public void TestDateConverter()
    {
        int year = DateTimeOffset.UtcNow.Year;

        CheckParse(2010, 4, 23, 0, 0, 0, 0, 0, "D:20100423");
        CheckParse(2011, 4, 23, 0, 0, 0, 0, 0, "20110423");
        CheckParse(2012, 1, 1, 0, 0, 0, 0, 0, "D:2012");
        CheckParse(2013, 1, 1, 0, 0, 0, 0, 0, "2013");

        // PDFBOX-1219
        CheckParse(2001, 1, 31, 10, 33, 0, +1, 0, "2001-01-31T10:33+01:00  ");

        // PDFBOX-465
        CheckParse(2002, 5, 12, 9, 47, 0, 0, 0, "9:47 5/12/2002");
        // PDFBOX-465
        CheckParse(2003, 12, 17, 2, 2, 3, 0, 0, "200312172:2:3");
        // PDFBOX-465
        CheckParse(2009, 3, 19, 20, 1, 22, 0, 0, "  20090319 200122");

        CheckParse(2014, 4, 1, 0, 0, 0, +2, 0, "20140401+0200");

        CheckParse(2016, 4, 1, 0, 0, 0, +4, 0, "20160401+04'00'");
        CheckParse(2017, 4, 1, 0, 0, 0, +9, 0, "20170401+09'00'");
        CheckParse(2017, 4, 1, 0, 0, 0, +9, 30, "20170401+09'30'");
        CheckParse(2018, 4, 1, 0, 0, 0, -2, 0, "20180401-02'00'");
        CheckParse(2019, 4, 1, 6, 1, 1, -11, 0, "20190401 6:1:1 -1100");

        // half hour timezones
        CheckParse(2016, 4, 1, 0, 0, 0, +4, 30, "20160401+04'30'");
        CheckParse(2017, 4, 1, 0, 0, 0, +9, 30, "20170401+09'30'");
        CheckParse(2018, 4, 1, 0, 0, 0, -2, 30, "20180401-02'30'");
        CheckParse(2019, 4, 1, 6, 1, 1, -11, 30, "20190401 6:1:1 -1130");

        // try dates invalid due to out of limit values
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "19921301 11:25");
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "19921232 11:25");
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "19921001 11:60");
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "19920401 24:25");

        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0,
            "20070430193647+713'00' illegal tz hr");  // PDFBOX-465
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "nodigits");
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "Unknown"); // PDFBOX-465
        CheckParse(Bad, 0, 0, 0, 0, 0, 0, 0, "333three digit year");

        CheckParse(2000, 2, 29, 0, 0, 0, 0, 0, "2000 Feb 29"); // valid date

        // PDFBOX-3315 GMT+12
        CheckParse(2016, 4, 11, 16, 01, 15, 12, 0, "D:20160411160115+12'00'");
    }

    private static void CheckToString(int yr, int mon, int day,
                int hr, int min, int sec,
                SimpleTimeZone tz, int offsetHours, int offsetMinutes)
    {
        // Construct a PdfGregorianCalendar from args (month is 1-based in parameter, but PdfGregorianCalendar is 0-based)
        PdfGregorianCalendar cal = new PdfGregorianCalendar(new SimpleTimeZone(tz.GetRawOffset(), tz.GetId()));
        cal.Set(yr, mon - 1, day, hr, min, sec);

        // create expected strings
        string sign = offsetHours >= 0 ? "+" : "-";
        int absHrs = Math.Abs(offsetHours);
        string pdfDate = string.Format(CultureInfo.InvariantCulture,
            "D:{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6}{7:D2}'{8:D2}'",
            yr, mon, day, hr, min, sec, sign, absHrs, offsetMinutes);
        string iso8601Date = string.Format(CultureInfo.InvariantCulture,
            "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}{6}{7:D2}:{8:D2}",
            yr, mon, day, hr, min, sec, sign, absHrs, offsetMinutes);

        Assert.Equal(pdfDate, DateConverter.ToString(cal));
        Assert.Equal(iso8601Date, DateConverter.ToISO8601(cal));
    }

    /// <summary>
    /// Test ToString() and ToISO8601() for various dates.
    /// </summary>
    [Fact]
    public void TestToString()
    {
        // std: +1h standard (+2h DST for Europe/Berlin), etc.
        // We use SimpleTimeZone with the fixed standard offset matching the expected test values
        SimpleTimeZone tzPghWinter = new SimpleTimeZone(-5 * Hrs, "America/New_York");
        SimpleTimeZone tzPghSummer = new SimpleTimeZone(-4 * Hrs, "America/New_York");
        SimpleTimeZone tzBerlinWinter = new SimpleTimeZone(+1 * Hrs, "Europe/Berlin");
        SimpleTimeZone tzBerlinSummer = new SimpleTimeZone(+2 * Hrs, "Europe/Berlin");
        SimpleTimeZone tzMaputo = new SimpleTimeZone(+2 * Hrs, "Africa/Maputo");
        SimpleTimeZone tzAruba = new SimpleTimeZone(-4 * Hrs, "America/Aruba");
        SimpleTimeZone tzJamaica = new SimpleTimeZone(-5 * Hrs, "America/Jamaica");
        SimpleTimeZone tzAdelaideSummer = new SimpleTimeZone((int)(9.5 * Hrs), "Australia/Adelaide");
        SimpleTimeZone tzAdelaideWinter = new SimpleTimeZone((int)(10.5 * Hrs), "Australia/Adelaide");

        Assert.Null(DateConverter.ToCalendar((string?)null));
        Assert.Null(DateConverter.ToCalendar("D:    "));
        Assert.Null(DateConverter.ToCalendar("D:"));

        CheckToString(2013, 8, 28, 3, 14, 15, tzPghSummer, -4, 0);
        CheckToString(2014, 2, 28, 3, 14, 15, tzPghWinter, -5, 0);
        CheckToString(2015, 8, 28, 3, 14, 15, tzBerlinSummer, +2, 0);
        CheckToString(2016, 2, 28, 3, 14, 15, tzBerlinWinter, +1, 0);
        CheckToString(2017, 8, 28, 3, 14, 15, tzAruba, -4, 0);
        CheckToString(2018, 1, 1, 1, 14, 15, tzJamaica, -5, 0);
        CheckToString(2019, 12, 31, 12, 59, 59, tzJamaica, -5, 0);
        CheckToString(2020, 2, 29, 0, 0, 0, tzMaputo, +2, 0);
        CheckToString(2015, 8, 28, 3, 14, 15, tzAdelaideSummer, +9, 30);
        CheckToString(2016, 2, 28, 3, 14, 15, tzAdelaideWinter, +10, 30);
    }

    private static void CheckParseTZ(int expect, string src)
    {
        PdfGregorianCalendar dest = DateConverter.NewGreg();
        DateConverter.ParseTZoffset(src, dest, new ParsePosition(0));
        Assert.Equal(expect, dest.Get(PdfGregorianCalendar.ZoneOffset));
    }

    /// <summary>
    /// Timezone testcase.
    /// </summary>
    [Fact]
    public void TestParseTZ()
    {
        CheckParseTZ(0 * Hrs + 0 * Mins, "+00:00");
        CheckParseTZ(0 * Hrs + 0 * Mins, "-0000");
        CheckParseTZ(1 * Hrs + 0 * Mins, "+1:00");
        CheckParseTZ(-(1 * Hrs + 0 * Mins), "-1:00");
        CheckParseTZ(-(1 * Hrs + 30 * Mins), "-0130");
        CheckParseTZ(11 * Hrs + 59 * Mins, "1159");
        CheckParseTZ(12 * Hrs + 30 * Mins, "1230");
        CheckParseTZ(-(12 * Hrs + 30 * Mins), "-12:30");
        CheckParseTZ(0 * Hrs + 0 * Mins, "Z");
        CheckParseTZ(-(8 * Hrs + 0 * Mins), "PST");
        CheckParseTZ(0 * Hrs + 0 * Mins, "EDT");  // EDT does not parse
        CheckParseTZ(-(3 * Hrs + 0 * Mins), "GMT-0300");
        CheckParseTZ(+(11 * Hrs + 0 * Mins), "GMT+11:00");
        CheckParseTZ((5 * Hrs + 0 * Mins), "0500");
        CheckParseTZ((5 * Hrs + 0 * Mins), "+0500");
        CheckParseTZ((11 * Hrs + 0 * Mins), "+11'00'");
        CheckParseTZ(0, "Z");
        // PDFBOX-3315, PDFBOX-2420
        CheckParseTZ(12 * Hrs + 0 * Mins, "+12:00");
        CheckParseTZ(-(12 * Hrs + 0 * Mins), "-12:00");
        CheckParseTZ(14 * Hrs + 0 * Mins, "1400");
        CheckParseTZ(-(14 * Hrs + 0 * Mins), "-1400");
    }

    private static void CheckFormatOffset(double off, string expect)
    {
        SimpleTimeZone tz = new SimpleTimeZone((int)(off * 60 * 60 * 1000), "junkID");
        string got = DateConverter.FormatTZoffset(tz.GetRawOffset(), ":");
        Assert.Equal(expect, got);
    }

    /// <summary>
    /// Timezone offset testcase.
    /// </summary>
    [Fact]
    public void TestFormatTZoffset()
    {
        CheckFormatOffset(-12.1, "-12:06");
        CheckFormatOffset(12.1, "+12:06");
        CheckFormatOffset(0, "+00:00");
        CheckFormatOffset(-1, "-01:00");
        CheckFormatOffset(.5, "+00:30");
        CheckFormatOffset(-0.5, "-00:30");
        CheckFormatOffset(.1, "+00:06");
        CheckFormatOffset(-0.1, "-00:06");
        CheckFormatOffset(-12, "-12:00");
        CheckFormatOffset(12, "+12:00");
        CheckFormatOffset(-11.5, "-11:30");
        CheckFormatOffset(11.5, "+11:30");
        CheckFormatOffset(11.9, "+11:54");
        CheckFormatOffset(11.1, "+11:06");
        CheckFormatOffset(-11.9, "-11:54");
        CheckFormatOffset(-11.1, "-11:06");
        // PDFBOX-2420
        CheckFormatOffset(14, "+14:00");
        CheckFormatOffset(-14, "-14:00");
    }
}
