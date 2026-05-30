/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TrueTypeFont.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
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

using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF.Model;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// A TrueType font file.
/// </summary>
public class TrueTypeFont : FontBoxFont, IDisposable
{
    private float _version;
    private int _numberOfGlyphs = -1;
    private int _unitsPerEm = -1;
    private bool _enableGsub = true;
    protected readonly Dictionary<string, TTFTable> tables = new(StringComparer.Ordinal);
    private readonly TTFDataStream _data;
    private volatile Dictionary<string, int>? _postScriptNames;
    private readonly object _lockReadtable = new();
    private readonly object _lockPSNames = new();
    private readonly List<string> _enabledGsubFeatures = [];

    public TrueTypeFont() : this(new MemoryTTFDataStream(Array.Empty<byte>()))
    {
    }

    internal TrueTypeFont(TTFDataStream fontData)
    {
        _data = fontData;
    }

    public float Version => _version;
    public uint SfntVersion { get; internal set; }
    public ushort NumberOfTables { get; internal set; }
    public IReadOnlyDictionary<string, TTFTable> Tables => tables;

    public void Close()
    {
        _data.Close();
    }

    public void Dispose() => Close();

    public float GetVersion() => _version;
    internal virtual void SetVersion(float versionValue) => _version = versionValue;
    public bool IsEnableGsub() => _enableGsub;
    public void SetEnableGsub(bool enableGsub) => _enableGsub = enableGsub;
    internal void AddTable(TTFTable table) => tables[table.GetTag()] = table;
    public ICollection<TTFTable> GetTables() => tables.Values;
    public Dictionary<string, TTFTable> GetTableMap() => tables;

    public byte[] GetTableBytes(TTFTable table)
    {
        lock (_lockReadtable)
        {
            long currentPosition = _data.GetCurrentPosition();
            _data.Seek(table.GetOffset());
            byte[] bytes = _data.Read((int)table.GetLength());
            _data.Seek(currentPosition);
            return bytes;
        }
    }

    public byte[] GetTableNBytes(TTFTable table, int limit)
    {
        lock (_lockReadtable)
        {
            long currentPosition = _data.GetCurrentPosition();
            _data.Seek(table.GetOffset());
            byte[] bytes = _data.Read(Math.Min(limit, (int)table.GetLength()));
            _data.Seek(currentPosition);
            return bytes;
        }
    }

    public virtual TTFTable? GetTable(string tag)
    {
        if (tables.TryGetValue(tag, out TTFTable? table) && table != null && !table.GetInitialized())
        {
            ReadTable(table);
        }

        return table;
    }

    public NamingTable? GetNaming() => GetTable(NamingTable.TAG) as NamingTable;
    public PostScriptTable? GetPostScript() => GetTable(PostScriptTable.TAG) as PostScriptTable;
    public OS2WindowsMetricsTable? GetOS2Windows() => GetTable(OS2WindowsMetricsTable.TAG) as OS2WindowsMetricsTable;
    public MaximumProfileTable? GetMaximumProfile() => GetTable(MaximumProfileTable.TAG) as MaximumProfileTable;
    public HeaderTable? GetHeader() => GetTable(HeaderTable.TAG) as HeaderTable;
    public HorizontalHeaderTable? GetHorizontalHeader() => GetTable(HorizontalHeaderTable.TAG) as HorizontalHeaderTable;
    public HorizontalMetricsTable? GetHorizontalMetrics() => GetTable(HorizontalMetricsTable.TAG) as HorizontalMetricsTable;
    public IndexToLocationTable? GetIndexToLocation() => GetTable(IndexToLocationTable.TAG) as IndexToLocationTable;
    public virtual GlyphTable? GetGlyph() => GetTable(GlyphTable.TAG) as GlyphTable;
    public CmapTable? GetCmap() => GetTable(CmapTable.Tag) as CmapTable ?? GetTable("cmap") as CmapTable;
    public VerticalHeaderTable? GetVerticalHeader() => GetTable(VerticalHeaderTable.TAG) as VerticalHeaderTable;
    public VerticalMetricsTable? GetVerticalMetrics() => GetTable(VerticalMetricsTable.TAG) as VerticalMetricsTable;
    public VerticalOriginTable? GetVerticalOrigin() => GetTable(VerticalOriginTable.TAG) as VerticalOriginTable;
    public KerningTable? GetKerning() => GetTable(KerningTable.TAG) as KerningTable;
    public GlyphSubstitutionTable? GetGsub() => GetTable(GlyphSubstitutionTable.TAG) as GlyphSubstitutionTable;
    public Stream GetOriginalData() => _data.GetOriginalData();
    public long GetOriginalDataSize() => _data.GetOriginalDataSize();

    internal void ReadTable(TTFTable table)
    {
        long currentPosition = _data.GetCurrentPosition();
        _data.Seek(table.GetOffset());
        table.Read(this, _data);
        _data.Seek(currentPosition);
    }

    internal void ReadTableHeaders(string tag, FontHeaders outHeaders)
    {
        if (tables.TryGetValue(tag, out TTFTable? table) && table != null)
        {
            long currentPosition = _data.GetCurrentPosition();
            _data.Seek(table.GetOffset());
            table.ReadHeaders(this, _data, outHeaders);
            _data.Seek(currentPosition);
        }
    }

    public int GetNumberOfGlyphs()
    {
        if (_numberOfGlyphs == -1)
        {
            MaximumProfileTable? maximumProfile = GetMaximumProfile();
            _numberOfGlyphs = maximumProfile != null ? maximumProfile.GetNumGlyphs() : 0;
        }

        return _numberOfGlyphs;
    }

    public int GetUnitsPerEm()
    {
        if (_unitsPerEm == -1)
        {
            HeaderTable? header = GetHeader();
            _unitsPerEm = header != null ? header.GetUnitsPerEm() : 1000;
        }

        return _unitsPerEm;
    }

    public int GetAdvanceWidth(int gid)
    {
        HorizontalMetricsTable? hmtx = GetHorizontalMetrics();
        return hmtx != null ? hmtx.GetAdvanceWidth(gid) : 250;
    }

    public int GetAdvanceHeight(int gid)
    {
        VerticalMetricsTable? vmtx = GetVerticalMetrics();
        return vmtx != null ? vmtx.GetAdvanceHeight(gid) : 250;
    }

    public virtual string GetName()
    {
        NamingTable? namingTable = GetNaming();
        return namingTable?.GetPostScriptName() ?? string.Empty;
    }

    private void ReadPostScriptNames()
    {
        Dictionary<string, int>? psnames = _postScriptNames;
        if (psnames == null)
        {
            PostScriptTable? post = GetPostScript();
            lock (_lockPSNames)
            {
                psnames = _postScriptNames;
                if (psnames == null)
                {
                    string[]? names = post?.GlyphNames;
                    if (names != null)
                    {
                        psnames = new Dictionary<string, int>(names.Length, StringComparer.Ordinal);
                        for (int i = 0; i < names.Length; i++)
                        {
                            psnames[names[i]] = i;
                        }
                    }
                    else
                    {
                        psnames = new Dictionary<string, int>(StringComparer.Ordinal);
                    }

                    _postScriptNames = psnames;
                }
            }
        }
    }

    public CmapLookup? GetUnicodeCmapLookup() => GetUnicodeCmapLookup(true);

    public CmapLookup? GetUnicodeCmapLookup(bool isStrict)
    {
        CmapSubtable? cmap = GetUnicodeCmapImpl(isStrict);
        if (cmap == null)
        {
            return null;
        }

        if (_enabledGsubFeatures.Count != 0)
        {
            GlyphSubstitutionTable? table = GetGsub();
            if (table != null)
            {
                return new SubstitutingCmapLookup(cmap, table, _enabledGsubFeatures.AsReadOnly());
            }
        }

        return cmap;
    }

    private CmapSubtable? GetUnicodeCmapImpl(bool isStrict)
    {
        CmapTable? cmapTable = GetCmap();
        if (cmapTable == null)
        {
            if (isStrict)
            {
                throw new IOException($"The TrueType font {GetName()} does not contain a 'cmap' table");
            }

            return null;
        }

        CmapSubtable? cmap = cmapTable.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode20Full);
        cmap ??= cmapTable.GetSubtable(CmapTable.PlatformWindows, CmapTable.EncodingWinUnicodeFull);
        cmap ??= cmapTable.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode20Bmp);
        cmap ??= cmapTable.GetSubtable(CmapTable.PlatformWindows, CmapTable.EncodingWinUnicodeBmp);
        cmap ??= cmapTable.GetSubtable(CmapTable.PlatformWindows, CmapTable.EncodingWinSymbol);
        cmap ??= cmapTable.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode11);
        if (cmap == null)
        {
            CmapSubtable[]? cmaps = cmapTable.GetCmaps();
            if (isStrict)
            {
                throw new IOException("The TrueType font does not contain a Unicode cmap");
            }

            if (cmaps != null && cmaps.Length > 0)
            {
                cmap = cmaps[0];
            }
        }

        return cmap;
    }

    public int NameToGID(string name)
    {
        ReadPostScriptNames();
        if (_postScriptNames != null && _postScriptNames.TryGetValue(name, out int gid) && gid > 0 && gid < GetMaximumProfile()!.GetNumGlyphs())
        {
            return gid;
        }

        int uni = ParseUniName(name);
        if (uni > -1)
        {
            CmapLookup? cmap = GetUnicodeCmapLookup(false);
            return cmap?.GetGlyphId(uni) ?? 0;
        }

        if (name.Length > 1 && name[0] == 'g' && int.TryParse(name[1..], out int parsed))
        {
            return parsed;
        }

        return 0;
    }

    public IGsubData GetGsubData()
    {
        if (!_enableGsub)
        {
            return IGsubData.NoDataFound;
        }

        GlyphSubstitutionTable? table = GetGsub();
        if (table == null)
        {
            return IGsubData.NoDataFound;
        }

        return table.GetGsubData();
    }

    private static int ParseUniName(string name)
    {
        if (name.StartsWith("uni", StringComparison.Ordinal) && name.Length == 7)
        {
            try
            {
                int codePoint = Convert.ToInt32(name.Substring(3, 4), 16);
                if (codePoint <= 0xD7FF || codePoint >= 0xE000)
                {
                    return codePoint;
                }
            }
            catch (FormatException)
            {
                return -1;
            }
        }

        return -1;
    }

    public virtual GeneralPath GetPath(string name)
    {
        int gid = NameToGID(name);
        GlyphData? glyph = GetGlyph()?.GetGlyph(gid);
        return glyph == null ? new GeneralPath() : glyph.GetPath();
    }

    public float GetWidth(string name)
    {
        int gid = NameToGID(name);
        int advance = GetAdvanceWidth(gid);
        int unitsPerEm = GetUnitsPerEm();
        return unitsPerEm != 1000 ? advance * 1000f / unitsPerEm : advance;
    }

    public bool HasGlyph(string name)
    {
        return NameToGID(name) != 0;
    }

    public BoundingBox GetFontBBox()
    {
        HeaderTable? headerTable = GetHeader();
        if (headerTable == null)
        {
            return new BoundingBox();
        }

        short xMin = headerTable.GetXMin();
        short xMax = headerTable.GetXMax();
        short yMin = headerTable.GetYMin();
        short yMax = headerTable.GetYMax();
        float scale = 1000f / Math.Max(1, GetUnitsPerEm());
        return new BoundingBox(xMin * scale, yMin * scale, xMax * scale, yMax * scale);
    }

    public IList<float> GetFontMatrix()
    {
        float scale = 1000f / Math.Max(1, GetUnitsPerEm());
        return [0.001f * scale, 0, 0, 0.001f * scale, 0, 0];
    }

    public void EnableGsubFeature(string featureTag) => _enabledGsubFeatures.Add(featureTag);
    public void DisableGsubFeature(string featureTag) => _enabledGsubFeatures.Remove(featureTag);

    public void EnableVerticalSubstitutions()
    {
        EnableGsubFeature("vrt2");
        EnableGsubFeature("vert");
    }

    public override string ToString()
    {
        try
        {
            NamingTable? namingTable = GetNaming();
            return namingTable?.GetPostScriptName() ?? "(null)";
        }
        catch (IOException e)
        {
            return $"(null - {e.Message})";
        }
    }
}
