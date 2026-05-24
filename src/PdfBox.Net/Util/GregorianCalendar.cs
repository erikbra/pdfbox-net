/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: (helper class — no direct Java upstream equivalent)
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

namespace PdfBox.Net.Util;

/// <summary>
/// A lightweight C# equivalent of Java's <c>GregorianCalendar</c> used internally by
/// <see cref="DateConverter"/>. Stores year, month (0-based), day, hour, minute, second,
/// millisecond, and a raw timezone offset in milliseconds (no DST).
/// </summary>
public class GregorianCalendar
{
    // Calendar field constants (matching Java Calendar constants)
    /// <summary>Field constant for the year.</summary>
    public const int Year = 1;
    /// <summary>Field constant for the month (0-based: January = 0).</summary>
    public const int Month = 2;
    /// <summary>Field constant for the day of the month.</summary>
    public const int DayOfMonth = 5;
    /// <summary>Field constant for the hour of the day (0–23).</summary>
    public const int HourOfDay = 11;
    /// <summary>Field constant for the minute within the hour.</summary>
    public const int Minute = 12;
    /// <summary>Field constant for the second within the minute.</summary>
    public const int Second = 13;
    /// <summary>Field constant for the millisecond.</summary>
    public const int Millisecond = 14;
    /// <summary>Field constant for the raw zone offset in milliseconds.</summary>
    public const int ZoneOffset = 15;
    /// <summary>Field constant for the DST offset (always 0 for SimpleTimeZone).</summary>
    public const int DstOffset = 16;

    private int _year;
    private int _month;        // 0-based
    private int _day;
    private int _hour;
    private int _minute;
    private int _second;
    private int _millisecond;
    private int _zoneOffsetMillis;
    private bool _lenient = true;
    private SimpleTimeZone _timezone;

    /// <summary>
    /// Constructs a GregorianCalendar with the given UTC timezone and all date/time fields
    /// initialised from the current moment in that timezone.
    /// </summary>
    public GregorianCalendar(SimpleTimeZone timezone)
    {
        _timezone = timezone;
        _zoneOffsetMillis = timezone.GetRawOffset();
        DateTimeOffset now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromMilliseconds(_zoneOffsetMillis));
        _year = now.Year;
        _month = now.Month - 1;  // convert to 0-based
        _day = now.Day;
        _hour = now.Hour;
        _minute = now.Minute;
        _second = now.Second;
        _millisecond = now.Millisecond;
    }

    /// <summary>
    /// Constructs a GregorianCalendar with UTC timezone and the given date (time fields = 0).
    /// Months are 0-based (January = 0), matching Java's convention.
    /// </summary>
    public GregorianCalendar(int year, int month, int day)
        : this(new SimpleTimeZone(0, "UTC"))
    {
        _year = year;
        _month = month;
        _day = day;
        _hour = 0; _minute = 0; _second = 0; _millisecond = 0;
    }

    /// <summary>
    /// Constructs a GregorianCalendar with UTC timezone and the given date and time.
    /// Months are 0-based (January = 0), matching Java's convention.
    /// </summary>
    public GregorianCalendar(int year, int month, int day, int hour, int minute, int second)
        : this(new SimpleTimeZone(0, "UTC"))
    {
        _year = year;
        _month = month;
        _day = day;
        _hour = hour;
        _minute = minute;
        _second = second;
        _millisecond = 0;
    }

    /// <summary>
    /// Sets leniency. When lenient is false, <see cref="GetTimeInMillis"/> throws for invalid dates.
    /// </summary>
    public void SetLenient(bool lenient)
    {
        _lenient = lenient;
    }

    /// <summary>
    /// Sets all main date/time fields at once. Month is 0-based.
    /// </summary>
    public void Set(int year, int month, int day, int hour, int minute, int second)
    {
        _year = year;
        _month = month;
        _day = day;
        _hour = hour;
        _minute = minute;
        _second = second;
        if (!_lenient)
        {
            // Validate by constructing a DateTimeOffset
            BuildDateTimeOffset(); // throws ArgumentOutOfRangeException if invalid
        }
    }

    /// <summary>
    /// Sets a single calendar field.
    /// </summary>
    public void Set(int field, int value)
    {
        switch (field)
        {
            case Year: _year = value; break;
            case Month: _month = value; break;
            case DayOfMonth: _day = value; break;
            case HourOfDay: _hour = value; break;
            case Minute: _minute = value; break;
            case Second: _second = value; break;
            case Millisecond: _millisecond = value; break;
            case ZoneOffset: _zoneOffsetMillis = value; break;
            default: throw new ArgumentException($"Unknown calendar field: {field}");
        }
    }

    /// <summary>
    /// Gets the value of a calendar field.
    /// </summary>
    public int Get(int field)
    {
        return field switch
        {
            Year => _year,
            Month => _month,
            DayOfMonth => _day,
            HourOfDay => _hour,
            Minute => _minute,
            Second => _second,
            Millisecond => _millisecond,
            ZoneOffset => _zoneOffsetMillis,
            DstOffset => 0,  // SimpleTimeZone has no DST
            _ => throw new ArgumentException($"Unknown calendar field: {field}")
        };
    }

    /// <summary>
    /// Adds a delta to the given field, with carry propagation (e.g. 60 minutes → 1 hour).
    /// </summary>
    public void Add(int field, int amount)
    {
        if (amount == 0) return;

        // Build a DateTimeOffset, add, then extract back
        DateTimeOffset dto;
        try
        {
            dto = BuildDateTimeOffset();
        }
        catch (ArgumentOutOfRangeException)
        {
            // If current fields are invalid, do arithmetic directly
            AddDirectly(field, amount);
            return;
        }

        dto = field switch
        {
            Year => dto.AddYears(amount),
            Month => dto.AddMonths(amount),
            DayOfMonth => dto.AddDays(amount),
            HourOfDay => dto.AddHours(amount),
            Minute => dto.AddMinutes(amount),
            Second => dto.AddSeconds(amount),
            Millisecond => dto.AddMilliseconds(amount),
            _ => throw new ArgumentException($"Cannot add to field: {field}")
        };

        _year = dto.Year;
        _month = dto.Month - 1;  // back to 0-based
        _day = dto.Day;
        _hour = dto.Hour;
        _minute = dto.Minute;
        _second = dto.Second;
        _millisecond = dto.Millisecond;
    }

    private void AddDirectly(int field, int amount)
    {
        // Fallback arithmetic when DateTimeOffset construction fails
        switch (field)
        {
            case Minute:
                _minute += amount;
                // Carry minutes to hours
                int hr = _minute / 60;
                _minute = ((_minute % 60) + 60) % 60;
                _hour += hr + ((_minute < 0) ? -1 : 0);
                break;
            case HourOfDay:
                _hour += amount;
                break;
            case Second:
                _second += amount;
                break;
            default:
                throw new ArgumentException($"Cannot add directly to field: {field}");
        }
    }

    /// <summary>
    /// Returns the number of milliseconds since the Unix epoch (1970-01-01T00:00:00Z).
    /// </summary>
    /// <exception cref="ArgumentException">If the date is invalid and lenient is false.</exception>
    public long GetTimeInMillis()
    {
        try
        {
            return BuildDateTimeOffset().ToUnixTimeMilliseconds();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            if (!_lenient)
            {
                throw new ArgumentException("Invalid date/time fields", ex);
            }
            throw;
        }
    }

    /// <summary>
    /// Returns the timezone associated with this calendar.
    /// </summary>
    public SimpleTimeZone GetTimeZone()
    {
        return _timezone;
    }

    /// <summary>
    /// Sets the timezone. Like Java's GregorianCalendar.setTimeZone(), this auto-converts the
    /// stored local time from the old timezone to the new timezone (adding the offset delta).
    /// The caller is expected to undo this with <c>Add(Minute, -delta)</c> if needed.
    /// </summary>
    public void SetTimeZone(SimpleTimeZone tz)
    {
        int oldOffset = _zoneOffsetMillis;
        int newOffset = tz.GetRawOffset();
        int deltaMins = (newOffset - oldOffset) / 60000;
        Add(Minute, deltaMins);
        _zoneOffsetMillis = newOffset;
        _timezone = tz;
    }

    private DateTimeOffset BuildDateTimeOffset()
    {
        // Month is 0-based internally; DateTimeOffset months are 1-based
        return new DateTimeOffset(_year, _month + 1, _day, _hour, _minute, _second, _millisecond,
                                  TimeSpan.FromMilliseconds(_zoneOffsetMillis));
    }
}
