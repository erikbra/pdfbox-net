/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphSubstitutionTable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

using System.IO;
using System.Text.RegularExpressions;
using PdfBox.Net.FontBox.TTF.Table.Common;
using PdfBox.Net.FontBox.TTF.Table.GSub;

namespace PdfBox.Net.FontBox.TTF;

public class GlyphSubstitutionTable() : TTFTable(TAG)
{
    public const string TAG = "GSUB";

    private IReadOnlyDictionary<string, ScriptTable> scriptList = new Dictionary<string, ScriptTable>(StringComparer.Ordinal);
    private FeatureListTable featureListTable = new(0, []);
    private LookupListTable lookupListTable = new(0, []);

    private readonly Dictionary<int, int> lookupCache = [];
    private readonly Dictionary<int, int> reverseLookup = [];
    private string? lastUsedSupportedScript;
    private GsubData gsubData = GsubData.NO_DATA_FOUND;

    private static readonly Regex IS_4_CHAR_WORD = new(@"^\w{4}$", RegexOptions.CultureInvariant);

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        long start = data.GetCurrentPosition();
        _ = data.ReadUnsignedShort();
        int minorVersion = data.ReadUnsignedShort();
        int scriptListOffset = data.ReadUnsignedShort();
        int featureListOffset = data.ReadUnsignedShort();
        int lookupListOffset = data.ReadUnsignedShort();
        long featureVariationsOffset = -1L;
        if (minorVersion == 1)
        {
            featureVariationsOffset = data.ReadUnsignedInt();
        }

        scriptList = ReadScriptList(data, start + scriptListOffset);
        featureListTable = ReadFeatureList(data, start + featureListOffset);
        lookupListTable = lookupListOffset > 0 ? ReadLookupList(data, start + lookupListOffset) : new LookupListTable(0, []);
        _ = featureVariationsOffset;
        gsubData = GsubData.NO_DATA_FOUND;
        initialized = true;
    }

    private IReadOnlyDictionary<string, ScriptTable> ReadScriptList(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int scriptCount = data.ReadUnsignedShort();
        int[] scriptOffsets = new int[scriptCount];
        string[] scriptTags = new string[scriptCount];
        Dictionary<string, ScriptTable> resultScriptList = new(scriptCount, StringComparer.Ordinal);
        for (int i = 0; i < scriptCount; i++)
        {
            scriptTags[i] = data.ReadString(4);
            scriptOffsets[i] = data.ReadUnsignedShort();
            if (scriptOffsets[i] < data.GetCurrentPosition() - offset)
            {
                return resultScriptList;
            }
        }

        for (int i = 0; i < scriptCount; i++)
        {
            if (resultScriptList.ContainsKey(scriptTags[i]))
            {
                continue;
            }

            ScriptTable scriptTable = ReadScriptTable(data, offset + scriptOffsets[i]);
            resultScriptList[scriptTags[i]] = scriptTable;
        }

        return resultScriptList;
    }

    private static ScriptTable ReadScriptTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int defaultLangSysOffset = data.ReadUnsignedShort();
        int langSysCount = data.ReadUnsignedShort();
        string[] langSysTags = new string[langSysCount];
        int[] langSysOffsets = new int[langSysCount];
        for (int i = 0; i < langSysCount; i++)
        {
            langSysTags[i] = data.ReadString(4);
            langSysOffsets[i] = data.ReadUnsignedShort();
            if (langSysOffsets[i] < data.GetCurrentPosition() - offset)
            {
                return new ScriptTable(null, new Dictionary<string, LangSysTable>(StringComparer.Ordinal));
            }
            if (i > 0 && string.CompareOrdinal(langSysTags[i], langSysTags[i - 1]) < 0)
            {
                return new ScriptTable(null, new Dictionary<string, LangSysTable>(StringComparer.Ordinal));
            }
        }

        LangSysTable? defaultLangSysTable = defaultLangSysOffset != 0 ? ReadLangSysTable(data, offset + defaultLangSysOffset) : null;
        Dictionary<string, LangSysTable> langSysTables = new(langSysCount, StringComparer.Ordinal);
        for (int i = 0; i < langSysCount; i++)
        {
            LangSysTable langSysTable = ReadLangSysTable(data, offset + langSysOffsets[i]);
            langSysTables[langSysTags[i]] = langSysTable;
        }

        return new ScriptTable(defaultLangSysTable, langSysTables);
    }

    private static LangSysTable ReadLangSysTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int lookupOrder = data.ReadUnsignedShort();
        int requiredFeatureIndex = data.ReadUnsignedShort();
        int featureIndexCount = data.ReadUnsignedShort();
        int[] featureIndices = new int[featureIndexCount];
        for (int i = 0; i < featureIndexCount; i++)
        {
            featureIndices[i] = data.ReadUnsignedShort();
        }

        return new LangSysTable(lookupOrder, requiredFeatureIndex, featureIndexCount, featureIndices);
    }

    private static FeatureListTable ReadFeatureList(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int featureCount = data.ReadUnsignedShort();
        FeatureRecord[] featureRecords = new FeatureRecord[featureCount];
        int[] featureOffsets = new int[featureCount];
        string[] featureTags = new string[featureCount];
        for (int i = 0; i < featureCount; i++)
        {
            featureTags[i] = data.ReadString(4);
            if (i > 0 && string.CompareOrdinal(featureTags[i], featureTags[i - 1]) < 0)
            {
                if (!IS_4_CHAR_WORD.IsMatch(featureTags[i]) || !IS_4_CHAR_WORD.IsMatch(featureTags[i - 1]))
                {
                    return new FeatureListTable(0, []);
                }
            }
            featureOffsets[i] = data.ReadUnsignedShort();
        }

        for (int i = 0; i < featureCount; i++)
        {
            FeatureTable featureTable = ReadFeatureTable(data, offset + featureOffsets[i]);
            featureRecords[i] = new FeatureRecord(featureTags[i], featureTable);
        }

        return new FeatureListTable(featureCount, featureRecords);
    }

    private static FeatureTable ReadFeatureTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int featureParams = data.ReadUnsignedShort();
        int lookupIndexCount = data.ReadUnsignedShort();
        int[] lookupListIndices = new int[lookupIndexCount];
        for (int i = 0; i < lookupIndexCount; i++)
        {
            lookupListIndices[i] = data.ReadUnsignedShort();
        }

        return new FeatureTable(featureParams, lookupIndexCount, lookupListIndices);
    }

    private LookupListTable ReadLookupList(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int lookupCount = data.ReadUnsignedShort();
        int[] lookups = new int[lookupCount];
        for (int i = 0; i < lookupCount; i++)
        {
            lookups[i] = data.ReadUnsignedShort();
        }

        LookupTable[] lookupTables = new LookupTable[lookupCount];
        Dictionary<int, LookupTable> lookupTableMap = [];
        for (int i = 0; i < lookupCount; i++)
        {
            if (!lookupTableMap.TryGetValue(lookups[i], out LookupTable? lookupTable))
            {
                lookupTable = ReadLookupTable(data, offset + lookups[i]);
                lookupTableMap[lookups[i]] = lookupTable;
            }
            lookupTables[i] = lookupTable;
        }

        return new LookupListTable(lookupCount, lookupTables);
    }

    private LookupSubTable? ReadLookupSubtable(TTFDataStream data, long offset, int lookupType)
    {
        return lookupType switch
        {
            1 => ReadSingleLookupSubTable(data, offset),
            2 => ReadMultipleSubstitutionSubtable(data, offset),
            3 => ReadAlternateSubstitutionSubtable(data, offset),
            4 => ReadLigatureSubstitutionSubtable(data, offset),
            _ => null,
        };
    }

    private LookupTable ReadLookupTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int lookupType = data.ReadUnsignedShort();
        int lookupFlag = data.ReadUnsignedShort();
        int subTableCount = data.ReadUnsignedShort();
        int[] subTableOffsets = new int[subTableCount];
        for (int i = 0; i < subTableCount; i++)
        {
            subTableOffsets[i] = data.ReadUnsignedShort();
            if (subTableOffsets[i] == 0 || offset + subTableOffsets[i] > data.GetOriginalDataSize())
            {
                return new LookupTable(lookupType, lookupFlag, 0, []);
            }
        }

        int markFilteringSet = (lookupFlag & 0x0010) != 0 ? data.ReadUnsignedShort() : 0;
        List<LookupSubTable> subTables = [];
        switch (lookupType)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                for (int i = 0; i < subTableCount; i++)
                {
                    LookupSubTable? subTable = ReadLookupSubtable(data, offset + subTableOffsets[i], lookupType);
                    if (subTable != null)
                    {
                        subTables.Add(subTable);
                    }
                }
                break;
            case 7:
                for (int i = 0; i < subTableCount; i++)
                {
                    data.Seek(offset + subTableOffsets[i]);
                    int substFormat = data.ReadUnsignedShort();
                    if (substFormat != 1)
                    {
                        continue;
                    }
                    int extensionLookupType = data.ReadUnsignedShort();
                    lookupType = extensionLookupType;
                    long extensionOffset = data.ReadUnsignedInt();
                    long extensionLookupTableAddress = offset + subTableOffsets[i] + extensionOffset;
                    LookupSubTable? subTable = ReadLookupSubtable(data, extensionLookupTableAddress, extensionLookupType);
                    if (subTable != null)
                    {
                        subTables.Add(subTable);
                    }
                }
                break;
        }

        return new LookupTable(lookupType, lookupFlag, markFilteringSet, [.. subTables]);
    }

    private LookupSubTable? ReadSingleLookupSubTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int substFormat = data.ReadUnsignedShort();
        switch (substFormat)
        {
            case 1:
            {
                int coverageOffset = data.ReadUnsignedShort();
                short deltaGlyphID = data.ReadSignedShort();
                CoverageTable coverageTable = ReadCoverageTable(data, offset + coverageOffset);
                return new LookupTypeSingleSubstFormat1(substFormat, coverageTable, deltaGlyphID);
            }
            case 2:
            {
                int coverageOffset = data.ReadUnsignedShort();
                int glyphCount = data.ReadUnsignedShort();
                int[] substituteGlyphIDs = new int[glyphCount];
                for (int i = 0; i < glyphCount; i++)
                {
                    substituteGlyphIDs[i] = data.ReadUnsignedShort();
                }
                CoverageTable coverageTable = ReadCoverageTable(data, offset + coverageOffset);
                return new LookupTypeSingleSubstFormat2(substFormat, coverageTable, substituteGlyphIDs);
            }
            default:
                return null;
        }
    }

    private LookupSubTable ReadMultipleSubstitutionSubtable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int substFormat = data.ReadUnsignedShort();
        if (substFormat != 1)
        {
            throw new IOException("The expected SubstFormat for LigatureSubstitutionTable is 1");
        }

        int coverage = data.ReadUnsignedShort();
        int sequenceCount = data.ReadUnsignedShort();
        int[] sequenceOffsets = new int[sequenceCount];
        for (int i = 0; i < sequenceCount; i++)
        {
            sequenceOffsets[i] = data.ReadUnsignedShort();
        }

        CoverageTable coverageTable = ReadCoverageTable(data, offset + coverage);
        if (sequenceCount != coverageTable.GetSize())
        {
            throw new IOException("According to the OpenTypeFont specifications, the coverage count should be equal to the no. of SequenceTables");
        }

        SequenceTable[] sequenceTables = new SequenceTable[sequenceCount];
        for (int i = 0; i < sequenceCount; i++)
        {
            data.Seek(offset + sequenceOffsets[i]);
            int glyphCount = data.ReadUnsignedShort();
            int[] substituteGlyphIDs = data.ReadUnsignedShortArray(glyphCount);
            sequenceTables[i] = new SequenceTable(glyphCount, substituteGlyphIDs);
        }

        return new LookupTypeMultipleSubstitutionFormat1(substFormat, coverageTable, sequenceTables);
    }

    private LookupSubTable ReadAlternateSubstitutionSubtable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int substFormat = data.ReadUnsignedShort();
        if (substFormat != 1)
        {
            throw new IOException("The expected SubstFormat for AlternateSubstitutionTable is 1");
        }

        int coverage = data.ReadUnsignedShort();
        int altSetCount = data.ReadUnsignedShort();
        int[] alternateOffsets = new int[altSetCount];
        for (int i = 0; i < altSetCount; i++)
        {
            alternateOffsets[i] = data.ReadUnsignedShort();
        }

        CoverageTable coverageTable = ReadCoverageTable(data, offset + coverage);
        if (altSetCount != coverageTable.GetSize())
        {
            throw new IOException("According to the OpenTypeFont specifications, the coverage count should be equal to the no. of AlternateSetTable");
        }

        AlternateSetTable[] alternateSetTables = new AlternateSetTable[altSetCount];
        for (int i = 0; i < altSetCount; i++)
        {
            data.Seek(offset + alternateOffsets[i]);
            int glyphCount = data.ReadUnsignedShort();
            int[] alternateGlyphIDs = data.ReadUnsignedShortArray(glyphCount);
            alternateSetTables[i] = new AlternateSetTable(glyphCount, alternateGlyphIDs);
        }

        return new LookupTypeAlternateSubstitutionFormat1(substFormat, coverageTable, alternateSetTables);
    }

    private LookupSubTable ReadLigatureSubstitutionSubtable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int substFormat = data.ReadUnsignedShort();
        if (substFormat != 1)
        {
            throw new IOException("The expected SubstFormat for LigatureSubstitutionTable is 1");
        }

        int coverage = data.ReadUnsignedShort();
        int ligSetCount = data.ReadUnsignedShort();
        int[] ligatureOffsets = new int[ligSetCount];
        for (int i = 0; i < ligSetCount; i++)
        {
            ligatureOffsets[i] = data.ReadUnsignedShort();
        }

        CoverageTable coverageTable = ReadCoverageTable(data, offset + coverage);
        if (ligSetCount != coverageTable.GetSize())
        {
            throw new IOException("According to the OpenTypeFont specifications, the coverage count should be equal to the no. of LigatureSetTables");
        }

        LigatureSetTable[] ligatureSetTables = new LigatureSetTable[ligSetCount];
        for (int i = 0; i < ligSetCount; i++)
        {
            int coverageGlyphId = coverageTable.GetGlyphId(i);
            ligatureSetTables[i] = ReadLigatureSetTable(data, offset + ligatureOffsets[i], coverageGlyphId);
        }

        return new LookupTypeLigatureSubstitutionSubstFormat1(substFormat, coverageTable, ligatureSetTables);
    }

    private static LigatureSetTable ReadLigatureSetTable(TTFDataStream data, long ligatureSetTableLocation, int coverageGlyphId)
    {
        data.Seek(ligatureSetTableLocation);
        int ligatureCount = data.ReadUnsignedShort();
        int[] ligatureOffsets = new int[ligatureCount];
        LigatureTable[] ligatureTables = new LigatureTable[ligatureCount];
        for (int i = 0; i < ligatureOffsets.Length; i++)
        {
            ligatureOffsets[i] = data.ReadUnsignedShort();
        }
        for (int i = 0; i < ligatureOffsets.Length; i++)
        {
            ligatureTables[i] = ReadLigatureTable(data, ligatureSetTableLocation + ligatureOffsets[i], coverageGlyphId);
        }

        return new LigatureSetTable(ligatureCount, ligatureTables);
    }

    private static LigatureTable ReadLigatureTable(TTFDataStream data, long ligatureTableLocation, int coverageGlyphId)
    {
        data.Seek(ligatureTableLocation);
        int ligatureGlyph = data.ReadUnsignedShort();
        int componentCount = data.ReadUnsignedShort();
        if (componentCount > 100)
        {
            throw new IOException($"componentCount in ligature table is {componentCount}, font likely corrupt");
        }

        int[] componentGlyphIDs = new int[componentCount];
        if (componentCount > 0)
        {
            componentGlyphIDs[0] = coverageGlyphId;
        }
        for (int i = 1; i <= componentCount - 1; i++)
        {
            componentGlyphIDs[i] = data.ReadUnsignedShort();
        }

        return new LigatureTable(ligatureGlyph, componentCount, componentGlyphIDs);
    }

    private CoverageTable ReadCoverageTable(TTFDataStream data, long offset)
    {
        data.Seek(offset);
        int coverageFormat = data.ReadUnsignedShort();
        switch (coverageFormat)
        {
            case 1:
            {
                int glyphCount = data.ReadUnsignedShort();
                int[] glyphArray = new int[glyphCount];
                for (int i = 0; i < glyphCount; i++)
                {
                    glyphArray[i] = data.ReadUnsignedShort();
                }
                return new CoverageTableFormat1(coverageFormat, glyphArray);
            }
            case 2:
            {
                int rangeCount = data.ReadUnsignedShort();
                RangeRecord[] rangeRecords = new RangeRecord[rangeCount];
                for (int i = 0; i < rangeCount; i++)
                {
                    rangeRecords[i] = ReadRangeRecord(data);
                }
                return new CoverageTableFormat2(coverageFormat, rangeRecords);
            }
            default:
                throw new IOException($"Unknown coverage format: {coverageFormat}");
        }
    }

    private string SelectScriptTag(string[] tags)
    {
        if (tags.Length == 1)
        {
            string tag = tags[0];
            if (OpenTypeScript.INHERITED.Equals(tag, StringComparison.Ordinal) ||
                (OpenTypeScript.TAG_DEFAULT.Equals(tag, StringComparison.Ordinal) && !scriptList.ContainsKey(tag)))
            {
                if (lastUsedSupportedScript == null)
                {
                    if (scriptList.Count == 0)
                    {
                        return tag;
                    }
                    lastUsedSupportedScript = scriptList.Keys.First();
                }
                return lastUsedSupportedScript;
            }
        }

        foreach (string tag in tags)
        {
            if (scriptList.ContainsKey(tag))
            {
                lastUsedSupportedScript = tag;
                return lastUsedSupportedScript;
            }
        }

        return tags[0];
    }

    private ICollection<LangSysTable> GetLangSysTables(string scriptTag)
    {
        List<LangSysTable> result = [];
        if (scriptList.TryGetValue(scriptTag, out ScriptTable? scriptTable))
        {
            result.AddRange(scriptTable.GetLangSysTables().Values);
            if (scriptTable.GetDefaultLangSysTable() != null)
            {
                result.Add(scriptTable.GetDefaultLangSysTable()!);
            }
        }
        return result;
    }

    private List<FeatureRecord> GetFeatureRecords(ICollection<LangSysTable> langSysTables, IReadOnlyList<string>? enabledFeatures)
    {
        if (langSysTables.Count == 0)
        {
            return [];
        }

        List<FeatureRecord> result = [];
        foreach (LangSysTable langSysTable in langSysTables)
        {
            int required = langSysTable.GetRequiredFeatureIndex();
            FeatureRecord[] featureRecords = featureListTable.GetFeatureRecords();
            if (required != 0xffff && required < featureRecords.Length)
            {
                result.Add(featureRecords[required]);
            }
            foreach (int featureIndex in langSysTable.GetFeatureIndices())
            {
                if (featureIndex < featureRecords.Length &&
                    (enabledFeatures == null || enabledFeatures.Contains(featureRecords[featureIndex].GetFeatureTag())))
                {
                    result.Add(featureRecords[featureIndex]);
                }
            }
        }

        if (ContainsFeature(result, "vrt2"))
        {
            RemoveFeature(result, "vert");
        }

        if (enabledFeatures != null && result.Count > 1)
        {
            result.Sort((a, b) => IndexOf(enabledFeatures, a.GetFeatureTag()).CompareTo(IndexOf(enabledFeatures, b.GetFeatureTag())));
        }

        return result;
    }

    private static int IndexOf(IReadOnlyList<string> values, string featureTag)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == featureTag)
            {
                return i;
            }
        }
        return -1;
    }

    private static bool ContainsFeature(List<FeatureRecord> featureRecords, string featureTag)
    {
        return featureRecords.Any(featureRecord => featureRecord.GetFeatureTag().Equals(featureTag, StringComparison.Ordinal));
    }

    private static void RemoveFeature(List<FeatureRecord> featureRecords, string featureTag)
    {
        featureRecords.RemoveAll(featureRecord => featureRecord.GetFeatureTag().Equals(featureTag, StringComparison.Ordinal));
    }

    private int ApplyFeature(FeatureRecord featureRecord, int gid)
    {
        int lookupResult = gid;
        LookupTable[] lookups = lookupListTable.GetLookups();
        foreach (int lookupListIndex in featureRecord.GetFeatureTable().GetLookupListIndices())
        {
            if (lookupListIndex < 0 || lookupListIndex >= lookups.Length)
            {
                continue;
            }
            LookupTable lookupTable = lookups[lookupListIndex];
            if (lookupTable.GetLookupType() != 1)
            {
                continue;
            }
            lookupResult = DoLookup(lookupTable, lookupResult);
        }
        return lookupResult;
    }

    private static int DoLookup(LookupTable lookupTable, int gid)
    {
        foreach (LookupSubTable lookupSubtable in lookupTable.GetSubTables())
        {
            int coverageIndex = lookupSubtable.GetCoverageTable().GetCoverageIndex(gid);
            if (coverageIndex >= 0)
            {
                return lookupSubtable.DoSubstitution(gid, coverageIndex);
            }
        }
        return gid;
    }

    public int GetSubstitution(int gid, string[] scriptTags, IReadOnlyList<string> enabledFeatures)
    {
        if (gid == -1)
        {
            return -1;
        }
        if (lookupCache.TryGetValue(gid, out int cached))
        {
            return cached;
        }

        string scriptTag = SelectScriptTag(scriptTags);
        ICollection<LangSysTable> langSysTables = GetLangSysTables(scriptTag);
        List<FeatureRecord> featureRecords = GetFeatureRecords(langSysTables, enabledFeatures);
        int sgid = gid;
        foreach (FeatureRecord featureRecord in featureRecords)
        {
            sgid = ApplyFeature(featureRecord, sgid);
        }
        lookupCache[gid] = sgid;
        reverseLookup[sgid] = gid;
        return sgid;
    }

    public int GetUnsubstitution(int sgid)
    {
        return reverseLookup.TryGetValue(sgid, out int gid) ? gid : sgid;
    }

    public GsubData GetGsubData() => gsubData;

    public GsubData? GetGsubData(string scriptTag)
    {
        return scriptList.ContainsKey(scriptTag) ? GsubData.NO_DATA_FOUND : null;
    }

    public ISet<string> GetSupportedScriptTags()
    {
        return new HashSet<string>(scriptList.Keys, StringComparer.Ordinal);
    }

    private static RangeRecord ReadRangeRecord(TTFDataStream data)
    {
        int startGlyphID = data.ReadUnsignedShort();
        int endGlyphID = data.ReadUnsignedShort();
        int startCoverageIndex = data.ReadUnsignedShort();
        return new RangeRecord(startGlyphID, endGlyphID, startCoverageIndex);
    }
}
