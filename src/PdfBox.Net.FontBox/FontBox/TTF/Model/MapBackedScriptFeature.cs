/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/model/MapBackedScriptFeature.java
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

namespace PdfBox.Net.FontBox.TTF.Model;

/// <summary>
/// A Dictionary-based simple implementation of the <see cref="IScriptFeature"/>
/// </summary>
public class MapBackedScriptFeature : IScriptFeature
{
    private readonly string _name;
    private readonly Dictionary<IList<int>, IList<int>> _featureMap;

    public MapBackedScriptFeature(string name, Dictionary<IList<int>, IList<int>> featureMap)
    {
        _name = name;
        _featureMap = new Dictionary<IList<int>, IList<int>>(featureMap, GlyphIdListComparer.Instance);
    }

    public string GetName() => _name;

    public ISet<IList<int>> GetAllGlyphIdsForSubstitution()
    {
        return new HashSet<IList<int>>(_featureMap.Keys, GlyphIdListComparer.Instance);
    }

    public bool CanReplaceGlyphs(IList<int> glyphIds)
    {
        return _featureMap.ContainsKey(glyphIds);
    }

    public IList<int> GetReplacementForGlyphs(IList<int> glyphIds)
    {
        if (!CanReplaceGlyphs(glyphIds))
        {
            throw new NotSupportedException("The glyphs " + string.Join(",", glyphIds) + " cannot be replaced");
        }
        return _featureMap[glyphIds];
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_name, _featureMap.Count);
    }

    public override bool Equals(object? obj)
    {
        if (this == obj) return true;
        if (obj is not MapBackedScriptFeature other) return false;
        return _name == other._name && DictionaryEquals(_featureMap, other._featureMap);
    }

    private static bool DictionaryEquals(Dictionary<IList<int>, IList<int>> a, Dictionary<IList<int>, IList<int>> b)
    {
        if (a.Count != b.Count) return false;
        var comparer = GlyphIdListComparer.Instance;
        foreach (var kv in a)
        {
            bool found = false;
            foreach (var kv2 in b)
            {
                if (comparer.Equals(kv.Key, kv2.Key) && comparer.Equals(kv.Value, kv2.Value))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }
}
