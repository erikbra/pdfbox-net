/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GlyphSubstitutionDataExtractor.java
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

using PdfBox.Net.FontBox.TTF.Model;
using PdfBox.Net.FontBox.TTF.Table.Common;
using PdfBox.Net.FontBox.TTF.Table.GSub;

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// This class has utility methods to extract meaningful GsubData from the highly obfuscated GSUB
/// Tables. This GsubData is then used to determine which combination of glyphs or words have to be
/// replaced.
/// </summary>
public class GlyphSubstitutionDataExtractor
{
    public IGsubData GetGsubData(Dictionary<string, ScriptTable> scriptList,
        FeatureListTable featureListTable, LookupListTable lookupListTable)
    {
        ScriptTableDetails? scriptTableDetails = GetSupportedLanguage(scriptList);

        if (scriptTableDetails == null)
        {
            return IGsubData.NoDataFound;
        }
        return BuildMapBackedGsubData(featureListTable, lookupListTable, scriptTableDetails);
    }

    /// <summary>
    /// Unlike <see cref="GetGsubData(Dictionary{string,ScriptTable},FeatureListTable,LookupListTable)"/>,
    /// this method doesn't iterate over supported Language's searching for the first match with the
    /// scripts of the font. Instead, it unconditionally creates ScriptTableDetails instance with
    /// language left Unspecified.
    /// </summary>
    public IGsubData GetGsubData(string scriptName, ScriptTable scriptTable,
        FeatureListTable featureListTable, LookupListTable lookupListTable)
    {
        var scriptTableDetails = new ScriptTableDetails(Language.Unspecified, scriptName, scriptTable);
        return BuildMapBackedGsubData(featureListTable, lookupListTable, scriptTableDetails);
    }

    private MapBackedGsubData BuildMapBackedGsubData(FeatureListTable featureListTable,
        LookupListTable lookupListTable, ScriptTableDetails scriptTableDetails)
    {
        ScriptTable scriptTable = scriptTableDetails.GetScriptTable();

        var gsubData = new Dictionary<string, Dictionary<IList<int>, IList<int>>>();

        if (scriptTable.GetDefaultLangSysTable() != null)
        {
            PopulateGsubData(gsubData, scriptTable.GetDefaultLangSysTable()!, featureListTable,
                lookupListTable);
        }
        foreach (var langSysTable in scriptTable.GetLangSysTables().Values)
        {
            PopulateGsubData(gsubData, langSysTable, featureListTable, lookupListTable);
        }

        return new MapBackedGsubData(scriptTableDetails.GetLanguage(),
            scriptTableDetails.GetFeatureName(), gsubData);
    }

    private static ScriptTableDetails? GetSupportedLanguage(Dictionary<string, ScriptTable> scriptList)
    {
        foreach (Language lang in Enum.GetValues<Language>())
        {
            foreach (string scriptName in lang.GetScriptNames())
            {
                if (scriptList.TryGetValue(scriptName, out var value))
                {
                    return new ScriptTableDetails(lang, scriptName, value);
                }
            }
        }
        return null;
    }

    private void PopulateGsubData(Dictionary<string, Dictionary<IList<int>, IList<int>>> gsubData,
        LangSysTable langSysTable, FeatureListTable featureListTable,
        LookupListTable lookupListTable)
    {
        FeatureRecord[] featureRecords = featureListTable.GetFeatureRecords();
        foreach (int featureIndex in langSysTable.GetFeatureIndices())
        {
            if (featureIndex < featureRecords.Length)
            {
                PopulateGsubData(gsubData, featureRecords[featureIndex], lookupListTable);
            }
        }
    }

    private void PopulateGsubData(Dictionary<string, Dictionary<IList<int>, IList<int>>> gsubData,
        FeatureRecord featureRecord, LookupListTable lookupListTable)
    {
        LookupTable[] lookups = lookupListTable.GetLookups();
        var glyphSubstitutionMap = new Dictionary<IList<int>, IList<int>>(GlyphIdListComparer.Instance);
        foreach (int lookupIndex in featureRecord.GetFeatureTable().GetLookupListIndices())
        {
            if (lookupIndex < lookups.Length)
            {
                ExtractData(glyphSubstitutionMap, lookups[lookupIndex]);
            }
        }
        gsubData[featureRecord.GetFeatureTag()] = glyphSubstitutionMap;
    }

    private void ExtractData(Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTable lookupTable)
    {
        foreach (var lookupSubTable in lookupTable.GetSubTables())
        {
            if (lookupSubTable is LookupTypeLigatureSubstitutionSubstFormat1 ligSub)
            {
                ExtractDataFromLigatureSubstitutionSubstFormat1Table(glyphSubstitutionMap, ligSub);
            }
            else if (lookupSubTable is LookupTypeAlternateSubstitutionFormat1 altSub)
            {
                ExtractDataFromAlternateSubstitutionSubstFormat1Table(glyphSubstitutionMap, altSub);
            }
            else if (lookupSubTable is LookupTypeSingleSubstFormat1 single1)
            {
                ExtractDataFromSingleSubstTableFormat1Table(glyphSubstitutionMap, single1);
            }
            else if (lookupSubTable is LookupTypeSingleSubstFormat2 single2)
            {
                ExtractDataFromSingleSubstTableFormat2Table(glyphSubstitutionMap, single2);
            }
            else if (lookupSubTable is LookupTypeMultipleSubstitutionFormat1 multSub)
            {
                ExtractDataFromMultipleSubstitutionFormat1Table(glyphSubstitutionMap, multSub);
            }
        }
    }

    private static void ExtractDataFromSingleSubstTableFormat1Table(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTypeSingleSubstFormat1 singleSubstTableFormat1)
    {
        CoverageTable coverageTable = singleSubstTableFormat1.GetCoverageTable();
        for (int i = 0; i < coverageTable.GetSize(); i++)
        {
            int coverageGlyphId = coverageTable.GetGlyphId(i);
            int substituteGlyphId = coverageGlyphId + singleSubstTableFormat1.GetDeltaGlyphID();
            PutNewSubstitutionEntry(glyphSubstitutionMap,
                new List<int> { substituteGlyphId },
                new List<int> { coverageGlyphId });
        }
    }

    private static void ExtractDataFromSingleSubstTableFormat2Table(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTypeSingleSubstFormat2 singleSubstTableFormat2)
    {
        CoverageTable coverageTable = singleSubstTableFormat2.GetCoverageTable();

        if (coverageTable.GetSize() != singleSubstTableFormat2.GetSubstituteGlyphIDs().Length)
        {
            return;
        }

        for (int i = 0; i < coverageTable.GetSize(); i++)
        {
            int coverageGlyphId = coverageTable.GetGlyphId(i);
            int substituteGlyphId = singleSubstTableFormat2.GetSubstituteGlyphIDs()[i];
            PutNewSubstitutionEntry(glyphSubstitutionMap,
                new List<int> { substituteGlyphId },
                new List<int> { coverageGlyphId });
        }
    }

    private static void ExtractDataFromMultipleSubstitutionFormat1Table(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTypeMultipleSubstitutionFormat1 multipleSubstFormat1Subtable)
    {
        CoverageTable coverageTable = multipleSubstFormat1Subtable.GetCoverageTable();

        if (coverageTable.GetSize() != multipleSubstFormat1Subtable.GetSequenceTables().Length)
        {
            return;
        }

        for (int i = 0; i < coverageTable.GetSize(); i++)
        {
            int coverageGlyphId = coverageTable.GetGlyphId(i);
            SequenceTable sequenceTable = multipleSubstFormat1Subtable.GetSequenceTables()[i];
            int[] substituteGlyphIDArray = sequenceTable.GetSubstituteGlyphIDs();
            var substituteGlyphIDList = new List<int>(substituteGlyphIDArray);
            PutNewSubstitutionEntry(glyphSubstitutionMap,
                substituteGlyphIDList,
                new List<int> { coverageGlyphId });
        }
    }

    private static void ExtractDataFromLigatureSubstitutionSubstFormat1Table(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTypeLigatureSubstitutionSubstFormat1 ligatureSubstitutionTable)
    {
        foreach (var ligatureSetTable in ligatureSubstitutionTable.GetLigatureSetTables())
        {
            foreach (var ligatureTable in ligatureSetTable.GetLigatureTables())
            {
                ExtractDataFromLigatureTable(glyphSubstitutionMap, ligatureTable);
            }
        }
    }

    /// <summary>
    /// Extracts data from the AlternateSubstitutionFormat1 (lookuptype 3) table and puts it in the
    /// glyphSubstitutionMap.
    /// </summary>
    private static void ExtractDataFromAlternateSubstitutionSubstFormat1Table(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LookupTypeAlternateSubstitutionFormat1 alternateSubstitutionFormat1)
    {
        CoverageTable coverageTable = alternateSubstitutionFormat1.GetCoverageTable();

        if (coverageTable.GetSize() != alternateSubstitutionFormat1.GetAlternateSetTables().Length)
        {
            return;
        }

        for (int i = 0; i < coverageTable.GetSize(); i++)
        {
            int coverageGlyphId = coverageTable.GetGlyphId(i);
            AlternateSetTable sequenceTable = alternateSubstitutionFormat1.GetAlternateSetTables()[i];

            foreach (int alternateGlyphId in sequenceTable.GetAlternateGlyphIDs())
            {
                if (alternateGlyphId != coverageGlyphId)
                {
                    PutNewSubstitutionEntry(glyphSubstitutionMap,
                        new List<int> { alternateGlyphId },
                        new List<int> { coverageGlyphId });
                    break;
                }
            }
        }
    }

    private static void ExtractDataFromLigatureTable(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        LigatureTable ligatureTable)
    {
        int[] componentGlyphIDs = ligatureTable.GetComponentGlyphIDs();
        var glyphsToBeSubstituted = new List<int>(componentGlyphIDs);

        PutNewSubstitutionEntry(glyphSubstitutionMap,
            new List<int> { ligatureTable.GetLigatureGlyph() },
            glyphsToBeSubstituted);
    }

    private static void PutNewSubstitutionEntry(
        Dictionary<IList<int>, IList<int>> glyphSubstitutionMap,
        IList<int> newGlyphList, IList<int> glyphsToBeSubstituted)
    {
        glyphSubstitutionMap[glyphsToBeSubstituted] = newGlyphList;
    }

    private sealed class ScriptTableDetails
    {
        private readonly Language _language;
        private readonly string _featureName;
        private readonly ScriptTable _scriptTable;

        internal ScriptTableDetails(Language language, string featureName, ScriptTable scriptTable)
        {
            _language = language;
            _featureName = featureName;
            _scriptTable = scriptTable;
        }

        public Language GetLanguage() => _language;
        public string GetFeatureName() => _featureName;
        public ScriptTable GetScriptTable() => _scriptTable;
    }
}
