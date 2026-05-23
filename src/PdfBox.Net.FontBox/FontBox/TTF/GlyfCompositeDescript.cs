/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeDescript.java
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class GlyfCompositeDescript : GlyfDescript
{
    private readonly List<GlyfCompositeComp> _components = [];
    private readonly Dictionary<int, GlyphDescription> _descriptions = [];
    private bool _beingResolved;
    private bool _resolved;
    private int _pointCount = -1;
    private int _contourCount = -1;

    internal GlyfCompositeDescript(TTFDataStream dataStream, GlyphTable glyphTable, int level) : base(-1)
    {
        GlyfCompositeComp component;
        do
        {
            component = new GlyfCompositeComp(dataStream);
            _components.Add(component);
        }
        while ((component.Flags & GlyfCompositeComp.MoreComponents) != 0);

        if ((component.Flags & GlyfCompositeComp.WeHaveInstructions) != 0)
        {
            ReadInstructions(dataStream, dataStream.ReadUnsignedShort());
        }

        foreach (GlyfCompositeComp current in _components)
        {
            GlyphData? glyph = glyphTable.GetGlyph(current.GlyphIndex, level);
            if (glyph is not null)
            {
                _descriptions[current.GlyphIndex] = glyph.Description;
            }
        }
    }

    public override void Resolve()
    {
        if (_resolved || _beingResolved)
        {
            return;
        }

        _beingResolved = true;
        int firstIndex = 0;
        int firstContour = 0;
        foreach (GlyfCompositeComp component in _components)
        {
            component.SetFirstIndex(firstIndex);
            component.SetFirstContour(firstContour);
            if (_descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description))
            {
                description.Resolve();
                firstIndex += description.GetPointCount();
                firstContour += description.GetContourCount();
            }
        }

        _resolved = true;
        _beingResolved = false;
    }

    public override int GetEndPtOfContours(int i)
    {
        GlyfCompositeComp? component = GetCompositeCompByContour(i);
        if (component is null || !_descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description))
        {
            return 0;
        }

        return description.GetEndPtOfContours(i - component.FirstContour) + component.FirstIndex;
    }

    public override byte GetFlags(int i)
    {
        GlyfCompositeComp? component = GetCompositeCompByPoint(i);
        if (component is null || !_descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description))
        {
            return 0;
        }

        return description.GetFlags(i - component.FirstIndex);
    }

    public override short GetXCoordinate(int i)
    {
        GlyfCompositeComp? component = GetCompositeCompByPoint(i);
        if (component is null || !_descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description))
        {
            return 0;
        }

        int index = i - component.FirstIndex;
        int x = description.GetXCoordinate(index);
        int y = description.GetYCoordinate(index);
        return (short)(component.ScaleX(x, y) + component.XTranslate);
    }

    public override short GetYCoordinate(int i)
    {
        GlyfCompositeComp? component = GetCompositeCompByPoint(i);
        if (component is null || !_descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description))
        {
            return 0;
        }

        int index = i - component.FirstIndex;
        int x = description.GetXCoordinate(index);
        int y = description.GetYCoordinate(index);
        return (short)(component.ScaleY(x, y) + component.YTranslate);
    }

    public override bool IsComposite()
    {
        return true;
    }

    public override int GetPointCount()
    {
        if (_pointCount >= 0)
        {
            return _pointCount;
        }

        GlyfCompositeComp component = _components[^1];
        _pointCount = _descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description)
            ? component.FirstIndex + description.GetPointCount()
            : 0;
        return _pointCount;
    }

    public override int GetContourCount()
    {
        if (_contourCount >= 0)
        {
            return _contourCount;
        }

        GlyfCompositeComp component = _components[^1];
        _contourCount = _descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description)
            ? component.FirstContour + description.GetContourCount()
            : 0;
        return _contourCount;
    }

    private GlyfCompositeComp? GetCompositeCompByPoint(int pointIndex)
    {
        return _components.FirstOrDefault(component =>
            _descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description) &&
            component.FirstIndex <= pointIndex &&
            pointIndex < component.FirstIndex + description.GetPointCount());
    }

    private GlyfCompositeComp? GetCompositeCompByContour(int contourIndex)
    {
        return _components.FirstOrDefault(component =>
            _descriptions.TryGetValue(component.GlyphIndex, out GlyphDescription? description) &&
            component.FirstContour <= contourIndex &&
            contourIndex < component.FirstContour + description.GetContourCount());
    }
}
