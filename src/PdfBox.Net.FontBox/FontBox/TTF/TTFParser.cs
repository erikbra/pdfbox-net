/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// TrueType font file parser.
/// </summary>
public class TTFParser
{
    private readonly bool _isEmbedded;
    private readonly bool _allowOpenType;

    public TTFParser() : this(false)
    {
    }

    public TTFParser(bool isEmbedded)
    {
        _isEmbedded = isEmbedded;
        _allowOpenType = isEmbedded;
    }

    public virtual TrueTypeFont Parse(RandomAccessRead randomAccessRead)
    {
        MemoryTTFDataStream dataStream = new(randomAccessRead);
        try
        {
            randomAccessRead.Close();
            return Parse(dataStream);
        }
        catch
        {
            dataStream.Close();
            throw;
        }
    }

    public virtual TrueTypeFont Parse(byte[] bytes)
    {
        return Parse(new MemoryTTFDataStream(bytes));
    }

    public virtual TrueTypeFont ParseEmbedded(Stream inputStream)
    {
        MemoryTTFDataStream dataStream = new(inputStream);
        try
        {
            inputStream.Close();
            return Parse(dataStream);
        }
        catch
        {
            dataStream.Close();
            throw;
        }
    }

    public virtual FontHeaders ParseTableHeaders(RandomAccessRead randomAccessRead)
    {
        using MemoryTTFDataStream dataStream = new(randomAccessRead);
        randomAccessRead.Close();
        return ParseTableHeaders(dataStream);
    }

    private TrueTypeFont CreateFontWithTables(TTFDataStream raf)
    {
        TrueTypeFont font = NewFont(raf);
        font.SetVersion(raf.Read32Fixed());
        int numberOfTables = raf.ReadUnsignedShort();
        font.NumberOfTables = (ushort)numberOfTables;
        _ = raf.ReadUnsignedShort();
        _ = raf.ReadUnsignedShort();
        _ = raf.ReadUnsignedShort();
        for (int i = 0; i < numberOfTables; i++)
        {
            TTFTable? table = ReadTableDirectory(raf);
            if (table != null)
            {
                if ((long)table.GetOffset() + table.GetLength() <= font.GetOriginalDataSize())
                {
                    font.AddTable(table);
                }
            }
        }

        return font;
    }

    internal virtual TrueTypeFont Parse(TTFDataStream raf)
    {
        TrueTypeFont font = CreateFontWithTables(raf);
        ParseTables(font);
        return font;
    }

    internal virtual TrueTypeFont NewFont(TTFDataStream raf)
    {
        return new(raf);
    }

    private void ParseTables(TrueTypeFont font)
    {
        foreach (TTFTable table in font.GetTables())
        {
            if (!table.GetInitialized())
            {
                font.ReadTable(table);
            }
        }

        bool hasCff = font.GetTableMap().ContainsKey(CFFTable.TAG);
        bool isOtf = font is OpenTypeFont;
        bool isPostScript = isOtf ? ((OpenTypeFont)font).IsPostScript : hasCff;

        if (font.GetHeader() == null)
        {
            throw new IOException("'head' table is mandatory");
        }
        if (font.GetHorizontalHeader() == null)
        {
            throw new IOException("'hhea' table is mandatory");
        }
        if (font.GetMaximumProfile() == null)
        {
            throw new IOException("'maxp' table is mandatory");
        }
        if (font.GetPostScript() == null && !_isEmbedded)
        {
            throw new IOException("'post' table is mandatory");
        }
        if (!isPostScript)
        {
            if (font.GetIndexToLocation() == null)
            {
                throw new IOException("'loca' table is mandatory");
            }
            if (font.GetGlyph() == null)
            {
                throw new IOException("'glyf' table is mandatory");
            }
        }
        else if (!isOtf)
        {
            throw new IOException("True Type fonts using CFF outlines are not supported");
        }
        if (font.GetNaming() == null && !_isEmbedded)
        {
            throw new IOException("'name' table is mandatory");
        }
        if (font.GetHorizontalMetrics() == null)
        {
            throw new IOException("'hmtx' table is mandatory");
        }
        if (!_isEmbedded && font.GetCmap() == null)
        {
            throw new IOException("'cmap' table is mandatory");
        }
    }

    internal virtual FontHeaders ParseTableHeaders(TTFDataStream raf)
    {
        FontHeaders outHeaders = new();
        using TrueTypeFont font = CreateFontWithTables(raf);
        {
            font.ReadTableHeaders(NamingTable.TAG, outHeaders);
            font.ReadTableHeaders(HeaderTable.TAG, outHeaders);
            outHeaders.SetOs2Windows(font.GetOS2Windows());

            bool isOTFAndPostScript;
            if (font is OpenTypeFont openTypeFont && openTypeFont.IsPostScript)
            {
                isOTFAndPostScript = true;
                if (openTypeFont.IsSupportedOTF())
                {
                    font.ReadTableHeaders(CFFTable.TAG, outHeaders);
                }
            }
            else if (font is not OpenTypeFont && font.GetTableMap().ContainsKey(CFFTable.TAG))
            {
                outHeaders.SetError("True Type fonts using CFF outlines are not supported");
                return outHeaders;
            }
            else
            {
                isOTFAndPostScript = false;
                if (font.GetTableMap().TryGetValue("gcid", out TTFTable? gcid) && gcid.GetLength() >= FontHeaders.BYTES_GCID)
                {
                    outHeaders.SetNonOtfGcid142(font.GetTableNBytes(gcid, FontHeaders.BYTES_GCID));
                }
            }

            outHeaders.SetIsOTFAndPostScript(isOTFAndPostScript);

            string?[] mandatoryTables =
            [
                HeaderTable.TAG,
                HorizontalHeaderTable.TAG,
                MaximumProfileTable.TAG,
                !_isEmbedded ? PostScriptTable.TAG : null,
                !isOTFAndPostScript ? IndexToLocationTable.TAG : null,
                !isOTFAndPostScript ? GlyphTable.TAG : null,
                !_isEmbedded ? NamingTable.TAG : null,
                HorizontalMetricsTable.TAG,
                !_isEmbedded ? CmapTable.Tag : null,
            ];

            foreach (string? tag in mandatoryTables)
            {
                if (tag != null && !font.GetTableMap().ContainsKey(tag))
                {
                    outHeaders.SetError($"'{tag}' table is mandatory");
                    return outHeaders;
                }
            }
        }

        return outHeaders;
    }

    protected virtual bool AllowCFF() => false;

    private TTFTable? ReadTableDirectory(TTFDataStream raf)
    {
        TTFTable table;
        string tag = raf.ReadString(4);
        switch (tag)
        {
            case CmapTable.Tag:
                table = new CmapTable();
                break;
            case GlyphTable.TAG:
                table = new GlyphTable();
                break;
            case HeaderTable.TAG:
                table = new HeaderTable();
                break;
            case HorizontalHeaderTable.TAG:
                table = new HorizontalHeaderTable();
                break;
            case HorizontalMetricsTable.TAG:
                table = new HorizontalMetricsTable();
                break;
            case IndexToLocationTable.TAG:
                table = new IndexToLocationTable();
                break;
            case MaximumProfileTable.TAG:
                table = new MaximumProfileTable();
                break;
            case NamingTable.TAG:
                table = new NamingTable();
                break;
            case OS2WindowsMetricsTable.TAG:
                table = new OS2WindowsMetricsTable();
                break;
            case PostScriptTable.TAG:
                table = new PostScriptTable();
                break;
            case DigitalSignatureTable.TAG:
                table = new DigitalSignatureTable();
                break;
            case KerningTable.TAG:
                table = new KerningTable();
                break;
            case VerticalHeaderTable.TAG:
                table = new VerticalHeaderTable();
                break;
            case VerticalMetricsTable.TAG:
                table = new VerticalMetricsTable();
                break;
            case VerticalOriginTable.TAG:
                table = new VerticalOriginTable();
                break;
            case GlyphSubstitutionTable.TAG:
                table = new GlyphSubstitutionTable();
                break;
            default:
                table = ReadTable(tag);
                break;
        }

        table.SetTag(tag);
        table.SetCheckSum(raf.ReadUnsignedInt());
        table.SetOffset(raf.ReadUnsignedInt());
        table.SetLength(raf.ReadUnsignedInt());

        if (table.GetLength() == 0 && tag != GlyphTable.TAG && tag != "glyf")
        {
            return null;
        }

        return table;
    }

    protected virtual TTFTable ReadTable(string tag)
    {
        return new TTFTable();
    }
}
