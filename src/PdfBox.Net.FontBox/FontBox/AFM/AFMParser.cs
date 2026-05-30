/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/AFMParser.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.AFM;

/// <summary>
/// This class is used to parse AFM (Adobe Font Metrics) files.
/// </summary>
public class AFMParser
{
    // Top-level keywords
    private const string START_FONT_METRICS = "StartFontMetrics";
    private const string END_FONT_METRICS = "EndFontMetrics";
    private const string COMMENT = "Comment";
    private const string FONT_NAME = "FontName";
    private const string FULL_NAME = "FullName";
    private const string FAMILY_NAME = "FamilyName";
    private const string WEIGHT = "Weight";
    private const string FONT_BBOX = "FontBBox";
    private const string VERSION = "Version";
    private const string NOTICE = "Notice";
    private const string ENCODING_SCHEME = "EncodingScheme";
    private const string MAPPING_SCHEME = "MappingScheme";
    private const string ESC_CHAR = "EscChar";
    private const string CHARACTER_SET = "CharacterSet";
    private const string CHARACTERS = "Characters";
    private const string IS_BASE_FONT = "IsBaseFont";
    private const string V_VECTOR = "VVector";
    private const string IS_FIXED_V = "IsFixedV";
    private const string CAP_HEIGHT = "CapHeight";
    private const string X_HEIGHT = "XHeight";
    private const string ASCENDER = "Ascender";
    private const string DESCENDER = "Descender";
    private const string STD_HW = "StdHW";
    private const string STD_VW = "StdVW";
    private const string START_DIRECTION = "StartDirection";
    private const string END_DIRECTION = "EndDirection";
    private const string UNDERLINE_POSITION = "UnderlinePosition";
    private const string UNDERLINE_THICKNESS = "UnderlineThickness";
    private const string ITALIC_ANGLE = "ItalicAngle";
    private const string CHAR_WIDTH = "CharWidth";
    private const string IS_FIXED_PITCH = "IsFixedPitch";
    private const string START_CHAR_METRICS = "StartCharMetrics";
    private const string END_CHAR_METRICS = "EndCharMetrics";
    private const string START_COMPOSITES = "StartComposites";
    private const string END_COMPOSITES = "EndComposites";
    private const string START_KERN_DATA = "StartKernData";
    private const string END_KERN_DATA = "EndKernData";
    private const string START_TRACK_KERN = "StartTrackKern";
    private const string END_TRACK_KERN = "EndTrackKern";
    private const string TRACK_KERN = "TrackKern";
    private const string START_KERN_PAIRS = "StartKernPairs";
    private const string START_KERN_PAIRS0 = "StartKernPairs0";
    private const string START_KERN_PAIRS1 = "StartKernPairs1";
    private const string END_KERN_PAIRS = "EndKernPairs";

    // CharMetric field keywords
    private const string CHARMETRIC_C = "C";
    private const string CHARMETRIC_CH = "CH";
    private const string CHARMETRIC_WX = "WX";
    private const string CHARMETRIC_W0X = "W0X";
    private const string CHARMETRIC_W1X = "W1X";
    private const string CHARMETRIC_WY = "WY";
    private const string CHARMETRIC_W0Y = "W0Y";
    private const string CHARMETRIC_W1Y = "W1Y";
    private const string CHARMETRIC_W = "W";
    private const string CHARMETRIC_W0 = "W0";
    private const string CHARMETRIC_W1 = "W1";
    private const string CHARMETRIC_VV = "VV";
    private const string CHARMETRIC_N = "N";
    private const string CHARMETRIC_B = "B";
    private const string CHARMETRIC_L = "L";

    // Composite keywords
    private const string CC = "CC";
    private const string PCC = "PCC";

    // Kern pair keywords
    private const string KP = "KP";
    private const string KPH = "KPH";
    private const string KPX = "KPX";
    private const string KPY = "KPY";

    private const string SEMICOLON = ";";

    private readonly Stream _input;

    /// <summary>
    /// Creates a new AFMParser from the given stream.
    /// </summary>
    /// <param name="input">The stream to read AFM data from.</param>
    public AFMParser(Stream input)
    {
        _input = input;
    }

    /// <summary>
    /// Parses the AFM stream and returns a <see cref="FontMetrics"/> object.
    /// </summary>
    /// <returns>The parsed font metrics.</returns>
    public FontMetrics Parse()
    {
        return Parse(false);
    }

    /// <summary>
    /// Parses the AFM stream.
    /// </summary>
    /// <param name="reducedDataOnly">If true, only font-level data is parsed (no char metrics, kern, composites).</param>
    /// <returns>The parsed font metrics.</returns>
    public FontMetrics Parse(bool reducedDataOnly)
    {
        FontMetrics metrics = new();
        bool endFound = false;

        using StreamReader reader = new(_input, System.Text.Encoding.Latin1, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

        string? startLine = reader.ReadLine();
        if (startLine == null || !startLine.StartsWith(START_FONT_METRICS, StringComparison.Ordinal))
        {
            throw new IOException($"Invalid AFM file: expected '{START_FONT_METRICS}', got '{startLine}'");
        }

        string versionPart = startLine.Substring(START_FONT_METRICS.Length).Trim();
        if (versionPart.Length > 0 && float.TryParse(versionPart, NumberStyles.Float, CultureInfo.InvariantCulture, out float afmVersion))
        {
            metrics.AfmVersion = afmVersion;
        }

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.TrimEnd();
            if (trimmed.Length == 0)
            {
                continue;
            }

            // Split keyword from rest
            int spaceIdx = trimmed.IndexOf(' ');
            string keyword = spaceIdx >= 0 ? trimmed.Substring(0, spaceIdx) : trimmed;
            string rest = spaceIdx >= 0 ? trimmed.Substring(spaceIdx + 1) : string.Empty;

            switch (keyword)
            {
                case COMMENT:
                    metrics.Comments.Add(rest);
                    break;
                case FONT_NAME:
                    metrics.FontName = rest;
                    break;
                case FULL_NAME:
                    metrics.FullName = rest;
                    break;
                case FAMILY_NAME:
                    metrics.FamilyName = rest;
                    break;
                case WEIGHT:
                    metrics.Weight = rest;
                    break;
                case FONT_BBOX:
                    metrics.FontBBox = ParseBoundingBox(rest);
                    break;
                case VERSION:
                    metrics.FontVersion = rest;
                    break;
                case NOTICE:
                    metrics.Notice = rest;
                    break;
                case ENCODING_SCHEME:
                    metrics.EncodingScheme = rest;
                    break;
                case MAPPING_SCHEME:
                    metrics.MappingScheme = ParseInt(rest);
                    break;
                case ESC_CHAR:
                    metrics.EscChar = ParseInt(rest);
                    break;
                case CHARACTER_SET:
                    metrics.CharacterSet = rest;
                    break;
                case CHARACTERS:
                    metrics.Characters = ParseInt(rest);
                    break;
                case IS_BASE_FONT:
                    metrics.IsBaseFont = ParseBool(rest);
                    break;
                case V_VECTOR:
                {
                    float[] vv = ParseFloatArray(rest, 2);
                    metrics.VVector = vv;
                    break;
                }
                case IS_FIXED_V:
                    metrics.IsFixedV = ParseBool(rest);
                    break;
                case CAP_HEIGHT:
                    metrics.CapHeight = ParseFloat(rest);
                    break;
                case X_HEIGHT:
                    metrics.XHeight = ParseFloat(rest);
                    break;
                case ASCENDER:
                    metrics.Ascender = ParseFloat(rest);
                    break;
                case DESCENDER:
                    metrics.Descender = ParseFloat(rest);
                    break;
                case STD_HW:
                    metrics.StdHW = ParseFloat(rest);
                    break;
                case STD_VW:
                    metrics.StdVW = ParseFloat(rest);
                    break;
                case UNDERLINE_POSITION:
                    metrics.UnderlinePosition = ParseFloat(rest);
                    break;
                case UNDERLINE_THICKNESS:
                    metrics.UnderlineThickness = ParseFloat(rest);
                    break;
                case ITALIC_ANGLE:
                    metrics.ItalicAngle = ParseFloat(rest);
                    break;
                case CHAR_WIDTH:
                    metrics.CharWidth = ParseFloatArray(rest, 2);
                    break;
                case IS_FIXED_PITCH:
                    metrics.IsFixedPitch = ParseBool(rest);
                    break;
                case START_DIRECTION:
                    // skip direction-specific block (re-read direction-level fields until EndDirection)
                    ParseDirectionMetrics(reader, metrics);
                    break;
                case START_CHAR_METRICS:
                {
                    int count = ParseInt(rest);
                    if (!reducedDataOnly)
                    {
                        ParseCharMetrics(reader, metrics, count);
                    }
                    else
                    {
                        SkipToEnd(reader, END_CHAR_METRICS);
                    }

                    break;
                }
                case START_KERN_DATA:
                    if (!reducedDataOnly)
                    {
                        ParseKernData(reader, metrics);
                    }
                    else
                    {
                        SkipToEnd(reader, END_KERN_DATA);
                    }

                    break;
                case START_COMPOSITES:
                {
                    int count = ParseInt(rest);
                    if (!reducedDataOnly)
                    {
                        ParseComposites(reader, metrics, count);
                    }
                    else
                    {
                        SkipToEnd(reader, END_COMPOSITES);
                    }

                    break;
                }
                case END_FONT_METRICS:
                    endFound = true;
                    break;
            }

            if (endFound)
            {
                break;
            }
        }

        return metrics;
    }

    private static void ParseDirectionMetrics(StreamReader reader, FontMetrics metrics)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.TrimEnd();
            if (trimmed.Length == 0)
            {
                continue;
            }

            int spaceIdx = trimmed.IndexOf(' ');
            string keyword = spaceIdx >= 0 ? trimmed.Substring(0, spaceIdx) : trimmed;
            string rest = spaceIdx >= 0 ? trimmed.Substring(spaceIdx + 1) : string.Empty;

            switch (keyword)
            {
                case UNDERLINE_POSITION:
                    metrics.UnderlinePosition = ParseFloat(rest);
                    break;
                case UNDERLINE_THICKNESS:
                    metrics.UnderlineThickness = ParseFloat(rest);
                    break;
                case ITALIC_ANGLE:
                    metrics.ItalicAngle = ParseFloat(rest);
                    break;
                case CHAR_WIDTH:
                    metrics.CharWidth = ParseFloatArray(rest, 2);
                    break;
                case IS_FIXED_PITCH:
                    metrics.IsFixedPitch = ParseBool(rest);
                    break;
                case END_DIRECTION:
                    return;
            }
        }
    }

    private static void ParseCharMetrics(StreamReader reader, FontMetrics metrics, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            CharMetric metric = ParseCharMetricLine(line);
            metrics.CharMetrics.Add(metric);
        }

        // Read EndCharMetrics line
        string? end = reader.ReadLine();
        while (end != null && !end.TrimStart().StartsWith(END_CHAR_METRICS, StringComparison.Ordinal))
        {
            // skip blank lines between metrics and end marker
            if (end.Trim().Length > 0 && !end.TrimStart().StartsWith(END_CHAR_METRICS, StringComparison.Ordinal))
            {
                // Could be an extra metric line if count was wrong; try to parse it
                string trimmedEnd = end.Trim();
                if (trimmedEnd.StartsWith(CHARMETRIC_C + " ", StringComparison.Ordinal) ||
                    trimmedEnd.StartsWith(CHARMETRIC_CH + " ", StringComparison.Ordinal))
                {
                    metrics.CharMetrics.Add(ParseCharMetricLine(end));
                }
            }

            end = reader.ReadLine();
        }
    }

    private static CharMetric ParseCharMetricLine(string line)
    {
        CharMetric metric = new();
        // Tokenize by semicolons
        string[] parts = line.Split(';');
        foreach (string part in parts)
        {
            string token = part.Trim();
            if (token.Length == 0)
            {
                continue;
            }

            // Split into keyword and value
            int spaceIdx = token.IndexOf(' ');
            string key = spaceIdx >= 0 ? token.Substring(0, spaceIdx) : token;
            string value = spaceIdx >= 0 ? token.Substring(spaceIdx + 1).Trim() : string.Empty;

            switch (key)
            {
                case CHARMETRIC_C:
                    metric.CharacterCode = ParseInt(value);
                    break;
                case CHARMETRIC_CH:
                    // hex character code e.g. <0041>
                    metric.CharacterCode = ParseHexCode(value);
                    break;
                case CHARMETRIC_WX:
                case "Wx": // case-insensitive alias
                    metric.Wx = ParseFloat(value);
                    metric.W0x = metric.Wx; // WX sets W0X
                    break;
                case CHARMETRIC_W0X:
                    metric.W0x = ParseFloat(value);
                    break;
                case CHARMETRIC_W1X:
                    metric.W1x = ParseFloat(value);
                    break;
                case CHARMETRIC_WY:
                    metric.Wy = ParseFloat(value);
                    metric.W0y = metric.Wy; // WY sets W0Y
                    break;
                case CHARMETRIC_W0Y:
                    metric.W0y = ParseFloat(value);
                    break;
                case CHARMETRIC_W1Y:
                    metric.W1y = ParseFloat(value);
                    break;
                case CHARMETRIC_W:
                case CHARMETRIC_W0:
                {
                    float[] ww = ParseFloatArray(value, 2);
                    if (ww.Length >= 2)
                    {
                        metric.W0x = ww[0];
                        metric.W0y = ww[1];
                        if (key == CHARMETRIC_W)
                        {
                            metric.Wx = ww[0];
                            metric.Wy = ww[1];
                        }
                    }

                    break;
                }
                case CHARMETRIC_W1:
                {
                    float[] ww = ParseFloatArray(value, 2);
                    if (ww.Length >= 2)
                    {
                        metric.W1x = ww[0];
                        metric.W1y = ww[1];
                    }

                    break;
                }
                case CHARMETRIC_VV:
                    // ignore VV for now
                    break;
                case CHARMETRIC_N:
                    metric.Name = value;
                    break;
                case CHARMETRIC_B:
                    metric.BoundingBox = ParseBoundingBox(value);
                    break;
                case CHARMETRIC_L:
                {
                    // L successor ligature
                    string[] ligParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (ligParts.Length >= 2)
                    {
                        Ligature lig = new() { Successor = ligParts[0], LigatureValue = ligParts[1] };
                        metric.Ligatures.Add(lig);
                    }

                    break;
                }
            }
        }

        return metric;
    }

    private static int ParseHexCode(string value)
    {
        // Format: <004B>
        string v = value.Trim();
        if (v.StartsWith('<') && v.EndsWith('>'))
        {
            v = v.Substring(1, v.Length - 2);
        }

        return int.TryParse(v, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result) ? result : -1;
    }

    private static void ParseKernData(StreamReader reader, FontMetrics metrics)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.TrimEnd();
            if (trimmed.Length == 0)
            {
                continue;
            }

            int spaceIdx = trimmed.IndexOf(' ');
            string keyword = spaceIdx >= 0 ? trimmed.Substring(0, spaceIdx) : trimmed;
            string rest = spaceIdx >= 0 ? trimmed.Substring(spaceIdx + 1) : string.Empty;

            switch (keyword)
            {
                case START_TRACK_KERN:
                    ParseTrackKern(reader, metrics, ParseInt(rest));
                    break;
                case START_KERN_PAIRS:
                    ParseKernPairs(reader, metrics.KernPairs, ParseInt(rest));
                    break;
                case START_KERN_PAIRS0:
                    ParseKernPairs(reader, metrics.KernPairs0, ParseInt(rest));
                    break;
                case START_KERN_PAIRS1:
                    ParseKernPairs(reader, metrics.KernPairs1, ParseInt(rest));
                    break;
                case END_KERN_DATA:
                    return;
            }
        }
    }

    private static void ParseTrackKern(StreamReader reader, FontMetrics metrics, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            string trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                i--;
                continue;
            }

            if (trimmed.StartsWith(TRACK_KERN, StringComparison.Ordinal))
            {
                string rest = trimmed.Substring(TRACK_KERN.Length).Trim();
                string[] tokens = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 5)
                {
                    TrackKern tk = new()
                    {
                        Degree = ParseInt(tokens[0]),
                        MinPtSize = ParseFloat(tokens[1]),
                        MinKern = ParseFloat(tokens[2]),
                        MaxPtSize = ParseFloat(tokens[3]),
                        MaxKern = ParseFloat(tokens[4])
                    };
                    metrics.TrackKerns.Add(tk);
                }
            }
        }

        SkipToEnd(reader, END_TRACK_KERN);
    }

    private static void ParseKernPairs(StreamReader reader, List<KernPair> pairs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            string trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                i--;
                continue;
            }

            string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                continue;
            }

            KernPair pair = new();
            string pairType = tokens[0];
            switch (pairType)
            {
                case KPX:
                    if (tokens.Length >= 4)
                    {
                        pair.FirstGlyph = tokens[1];
                        pair.SecondGlyph = tokens[2];
                        pair.DeltaX = ParseFloat(tokens[3]);
                        pair.DeltaY = 0;
                        pairs.Add(pair);
                    }

                    break;
                case KPY:
                    if (tokens.Length >= 4)
                    {
                        pair.FirstGlyph = tokens[1];
                        pair.SecondGlyph = tokens[2];
                        pair.DeltaX = 0;
                        pair.DeltaY = ParseFloat(tokens[3]);
                        pairs.Add(pair);
                    }

                    break;
                case KP:
                    if (tokens.Length >= 5)
                    {
                        pair.FirstGlyph = tokens[1];
                        pair.SecondGlyph = tokens[2];
                        pair.DeltaX = ParseFloat(tokens[3]);
                        pair.DeltaY = ParseFloat(tokens[4]);
                        pairs.Add(pair);
                    }

                    break;
                case KPH:
                    // KPH <hex1> <hex2> dx dy — hex glyph codes; treat similarly to KP
                    if (tokens.Length >= 5)
                    {
                        pair.FirstGlyph = tokens[1];
                        pair.SecondGlyph = tokens[2];
                        pair.DeltaX = ParseFloat(tokens[3]);
                        pair.DeltaY = ParseFloat(tokens[4]);
                        pairs.Add(pair);
                    }

                    break;
            }
        }

        SkipToEnd(reader, END_KERN_PAIRS);
    }

    private static void ParseComposites(StreamReader reader, FontMetrics metrics, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            string trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                i--;
                continue;
            }

            Composite? composite = ParseCompositeLine(trimmed);
            if (composite != null)
            {
                metrics.Composites.Add(composite);
            }
        }

        SkipToEnd(reader, END_COMPOSITES);
    }

    private static Composite? ParseCompositeLine(string line)
    {
        // Format: CC <name> <count> ; PCC <part> <dx> <dy> ; PCC ...
        string[] semicolonParts = line.Split(';');
        if (semicolonParts.Length == 0)
        {
            return null;
        }

        string header = semicolonParts[0].Trim();
        string[] headerTokens = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (headerTokens.Length < 2 || !headerTokens[0].Equals(CC, StringComparison.Ordinal))
        {
            return null;
        }

        Composite composite = new() { Name = headerTokens[1] };

        for (int j = 1; j < semicolonParts.Length; j++)
        {
            string part = semicolonParts[j].Trim();
            if (part.Length == 0)
            {
                continue;
            }

            string[] partTokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partTokens.Length >= 4 && partTokens[0].Equals(PCC, StringComparison.Ordinal))
            {
                CompositePart cp = new()
                {
                    Name = partTokens[1],
                    DisplacementX = ParseInt(partTokens[2]),
                    DisplacementY = ParseInt(partTokens[3])
                };
                composite.Parts.Add(cp);
            }
        }

        return composite;
    }

    private static void SkipToEnd(StreamReader reader, string endKeyword)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.TrimStart().StartsWith(endKeyword, StringComparison.Ordinal))
            {
                return;
            }
        }
    }

    private static BoundingBox ParseBoundingBox(string value)
    {
        float[] values = ParseFloatArray(value, 4);
        if (values.Length >= 4)
        {
            return new BoundingBox(values[0], values[1], values[2], values[3]);
        }

        return new BoundingBox();
    }

    private static float[] ParseFloatArray(string value, int expectedCount)
    {
        string[] tokens = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int actual = Math.Min(tokens.Length, expectedCount);
        float[] result = new float[actual];
        for (int i = 0; i < actual; i++)
        {
            result[i] = ParseFloat(tokens[i]);
        }

        return result;
    }

    private static float ParseFloat(string value)
    {
        return float.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
            ? result
            : 0f;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            ? result
            : 0;
    }

    private static bool ParseBool(string value)
    {
        return value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
