/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeDescript.java
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

namespace PdfBox.Net.FontBox.TTF;

public class GlyfCompositeDescript : GlyfDescript
{
    private readonly List<GlyfCompositeComp> _components = [];
    private readonly Dictionary<int, GlyphDescription> _descriptions = [];
    private readonly GlyphTable? _glyphTable;
    private bool _beingResolved;
    private bool _resolved;
    private int _pointCount = -1;
    private int _contourCount = -1;

    public GlyfCompositeDescript(TTFDataStream data, GlyphTable glyphTable, int level) : base((short)-1)
    {
        _glyphTable = glyphTable;
        GlyfCompositeComp comp;
        do
        {
            comp = new GlyfCompositeComp(data);
            _components.Add(comp);
        }
        while ((comp.Flags & GlyfCompositeComp.MoreComponents) != 0);

        if ((comp.Flags & GlyfCompositeComp.WeHaveInstructions) != 0)
        {
            ReadInstructions(data, data.ReadUnsignedShort());
        }

        InitDescriptions(level);
    }

    public override void Resolve()
    {
        if (_resolved)
        {
            return;
        }

        if (_beingResolved)
        {
            Console.Error.WriteLine("Circular reference in GlyfCompositeDesc");
            return;
        }

        _beingResolved = true;
        int firstIndex = 0;
        int firstContour = 0;
        foreach (GlyfCompositeComp comp in _components)
        {
            comp.FirstIndex = firstIndex;
            comp.FirstContour = firstContour;
            if (_descriptions.TryGetValue(comp.GlyphIndex, out GlyphDescription? desc))
            {
                desc.Resolve();
                firstIndex += desc.GetPointCount();
                firstContour += desc.GetContourCount();
            }
        }

        _resolved = true;
        _beingResolved = false;
    }

    public override int GetEndPtOfContours(int i)
    {
        GlyfCompositeComp? c = GetCompositeCompEndPt(i);
        if (c is not null && _descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
        {
            return gd.GetEndPtOfContours(i - c.FirstContour) + c.FirstIndex;
        }

        return 0;
    }

    public override byte GetFlags(int i)
    {
        GlyfCompositeComp? c = GetCompositeComp(i);
        if (c is not null && _descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
        {
            return gd.GetFlags(i - c.FirstIndex);
        }

        return 0;
    }

    public override short GetXCoordinate(int i)
    {
        GlyfCompositeComp? c = GetCompositeComp(i);
        if (c is not null && _descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
        {
            int n = i - c.FirstIndex;
            int x = gd.GetXCoordinate(n);
            int y = gd.GetYCoordinate(n);
            return (short)(c.ScaleX(x, y) + c.XTranslate);
        }

        return 0;
    }

    public override short GetYCoordinate(int i)
    {
        GlyfCompositeComp? c = GetCompositeComp(i);
        if (c is not null && _descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
        {
            int n = i - c.FirstIndex;
            int x = gd.GetXCoordinate(n);
            int y = gd.GetYCoordinate(n);
            return (short)(c.ScaleY(x, y) + c.YTranslate);
        }

        return 0;
    }

    public override bool IsComposite() => true;

    public override int GetPointCount()
    {
        if (!_resolved)
        {
            Console.Error.WriteLine("getPointCount called on unresolved GlyfCompositeDescript");
        }

        if (_pointCount < 0 && _components.Count > 0)
        {
            GlyfCompositeComp c = _components[^1];
            if (_descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
            {
                _pointCount = c.FirstIndex + gd.GetPointCount();
            }
            else
            {
                Console.Error.WriteLine($"GlyphDescription for index {c.GlyphIndex} is null, returning 0");
                _pointCount = 0;
            }
        }

        return _pointCount < 0 ? 0 : _pointCount;
    }

    public override int GetContourCount()
    {
        if (!_resolved)
        {
            Console.Error.WriteLine("getContourCount called on unresolved GlyfCompositeDescript");
        }

        if (_contourCount < 0 && _components.Count > 0)
        {
            GlyfCompositeComp c = _components[^1];
            if (_descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd))
            {
                _contourCount = c.FirstContour + gd.GetContourCount();
            }
            else
            {
                Console.Error.WriteLine($"missing glyph description for index {c.GlyphIndex}");
                _contourCount = 0;
            }
        }

        return _contourCount < 0 ? 0 : _contourCount;
    }

    public int GetComponentCount() => _components.Count;

    public IReadOnlyList<GlyfCompositeComp> GetComponents() => _components;

    private GlyfCompositeComp? GetCompositeComp(int i)
    {
        foreach (GlyfCompositeComp c in _components)
        {
            if (_descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd) && c.FirstIndex <= i && i < c.FirstIndex + gd.GetPointCount())
            {
                return c;
            }
        }

        return null;
    }

    private GlyfCompositeComp? GetCompositeCompEndPt(int i)
    {
        foreach (GlyfCompositeComp c in _components)
        {
            if (_descriptions.TryGetValue(c.GlyphIndex, out GlyphDescription? gd) && c.FirstContour <= i && i < c.FirstContour + gd.GetContourCount())
            {
                return c;
            }
        }

        return null;
    }

    private void InitDescriptions(int level)
    {
        if (_glyphTable is null)
        {
            return;
        }

        foreach (GlyfCompositeComp component in _components)
        {
            try
            {
                int index = component.GlyphIndex;
                GlyphData? glyph = _glyphTable.GetGlyph(index, level);
                if (glyph?.Description is not null)
                {
                    _descriptions[index] = glyph.Description;
                }
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
