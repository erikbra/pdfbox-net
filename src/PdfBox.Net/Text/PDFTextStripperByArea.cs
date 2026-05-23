/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripperByArea.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;
using System.IO;

namespace PdfBox.Net.Text;

public class PDFTextStripperByArea : PDFTextStripper
{
    private readonly List<string> _regions = new();
    private readonly Dictionary<string, Rectangle2D> _regionArea = new();
    private readonly Dictionary<string, List<List<TextPosition>>> _regionCharacterList = new();
    private readonly Dictionary<string, StringWriter> _regionText = new();

    public PDFTextStripperByArea()
    {
        // Java source calls super.setShouldSeparateByBeads(false) to bypass the
        // overridden no-op. We replicate this by calling the base class method.
        base.SetShouldSeparateByBeads(false);
    }

    public sealed override void SetShouldSeparateByBeads(bool aShouldSeparateByBeads)
    {
    }

    public void AddRegion(string regionName, Rectangle2D rect)
    {
        _regions.Add(regionName);
        _regionArea[regionName] = rect;
    }

    public void RemoveRegion(string regionName)
    {
        _regions.Remove(regionName);
        _regionArea.Remove(regionName);
    }

    public List<string> GetRegions()
    {
        return _regions;
    }

    public string GetTextForRegion(string regionName)
    {
        return _regionText.TryGetValue(regionName, out StringWriter? text) ? text.ToString() : string.Empty;
    }

    public void ExtractRegions(PDPage page)
    {
        foreach (string regionName in _regions)
        {
            SetStartPage(GetCurrentPageNo());
            SetEndPage(GetCurrentPageNo());
            List<List<TextPosition>> regionCharactersByArticle = [new List<TextPosition>()];
            _regionCharacterList[regionName] = regionCharactersByArticle;
            _regionText[regionName] = new StringWriter();
        }

        if (page.HasContents())
        {
            ProcessPage(page);
        }
    }

    protected override void ProcessTextPosition(TextPosition text)
    {
        foreach ((string key, Rectangle2D rect) in _regionArea)
        {
            if (rect.Contains(text.GetX(), text.GetY()))
            {
                charactersByArticle = _regionCharacterList[key];
                base.ProcessTextPosition(text);
            }
        }
    }

    protected override void WritePage()
    {
        foreach (string region in _regionArea.Keys)
        {
            charactersByArticle = _regionCharacterList[region];
            output = _regionText[region];
            base.WritePage();
        }
    }
}
