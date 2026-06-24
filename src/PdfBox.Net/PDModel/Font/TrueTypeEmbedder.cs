/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/TrueTypeEmbedder.java
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

using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.PDModel.Font;

/// <summary>
/// Common functionality for embedding TrueType fonts.
/// </summary>
/// <remarks>Authors: Ben Litchfield, John Hewson.</remarks>
internal abstract class TrueTypeEmbedder : ISubsetter
{
    private const string Base25 = "BCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static readonly string[] SubsetTables =
    [
        "head",
        "hhea",
        "loca",
        "maxp",
        "cvt ",
        "prep",
        "glyf",
        "hmtx",
        "cmap",
        "name",
        "fpgm",
        "gasp",
        "post"
    ];

    private readonly HashSet<int> _subsetCodePoints = [];
    private readonly HashSet<int> _allGlyphIds = [];
    private bool _embedSubset = true;

    /// <summary>The TrueType font being embedded.</summary>
    protected TrueTypeFont? Ttf;

    /// <summary>The font descriptor populated during construction.</summary>
    protected PDFontDescriptor? FontDescriptor;

    /// <summary>The Unicode cmap lookup for the embedded font.</summary>
    protected CmapLookup? CmapLookup;

    protected TrueTypeEmbedder()
    {
    }

    protected TrueTypeEmbedder(TrueTypeFont ttf, bool embedSubset = true)
    {
        SetTrueTypeFont(ttf, embedSubset);
    }

    /// <inheritdoc/>
    public virtual void AddToSubset(int codePoint)
    {
        _subsetCodePoints.Add(codePoint);
    }

    /// <inheritdoc/>
    public virtual void Subset()
    {
        if (!_embedSubset)
        {
            throw new InvalidOperationException("Subsetting is disabled.");
        }

        TrueTypeFont ttf = Ttf ?? throw new InvalidOperationException("No TrueType font has been configured for subsetting.");
        TTFSubsetter subsetter = new(ttf, SubsetTables);
        subsetter.AddAll(_subsetCodePoints);
        subsetter.ForceInvisible(0x200B);
        subsetter.ForceInvisible(0x200C);
        subsetter.ForceInvisible(0x2060);
        subsetter.ForceInvisible(0xFEFF);

        if (_allGlyphIds.Count > 0)
        {
            subsetter.AddGlyphIds(_allGlyphIds);
        }

        Dictionary<int, int> gidToCid = subsetter.GetGIDMap();
        string tag = GetTag(gidToCid);
        subsetter.SetPrefix(tag);

        using MemoryStream output = new();
        subsetter.WriteToStream(output);
        BuildSubset(new MemoryStream(output.ToArray()), tag, gidToCid);
    }

    /// <summary>
    /// Returns true if the font needs to be subset.
    /// </summary>
    public virtual bool NeedsSubset() => _embedSubset;

    /// <summary>
    /// Returns the font descriptor.
    /// </summary>
    public PDFontDescriptor? GetFontDescriptor() => FontDescriptor;

    /// <summary>
    /// Rebuilds a font subset from the given subsetter output.
    /// </summary>
    protected abstract void BuildSubset(Stream ttfSubset, string tag,
        IDictionary<int, int> gidToCid);

    public IReadOnlyCollection<int> GetSubsetCodePoints() => _subsetCodePoints;

    public void AddGlyphIds(ISet<int> glyphIds)
    {
        ArgumentNullException.ThrowIfNull(glyphIds);
        _allGlyphIds.UnionWith(glyphIds);
    }

    public string GetTag(IDictionary<int, int> gidToCid)
    {
        ArgumentNullException.ThrowIfNull(gidToCid);
        int hash = 0;
        foreach (KeyValuePair<int, int> entry in gidToCid.OrderBy(e => e.Key))
        {
            unchecked
            {
                hash += entry.Key ^ entry.Value;
            }
        }

        long num = Math.Abs((long)hash);
        string tag = string.Empty;
        do
        {
            long div = num / 25;
            int mod = (int)(num % 25);
            tag += Base25[mod];
            num = div;
        }
        while (num != 0 && tag.Length < 6);

        return tag.PadLeft(6, 'A') + "+";
    }

    protected void SetTrueTypeFont(TrueTypeFont ttf, bool embedSubset = true)
    {
        Ttf = ttf ?? throw new ArgumentNullException(nameof(ttf));
        CmapLookup = Ttf.GetUnicodeCmapLookup(false);
        _embedSubset = embedSubset;
    }

    protected static byte[] ReadAllBytes(Stream input)
    {
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        return buffer.ToArray();
    }
}
