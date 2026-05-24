/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDPageLabels.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public class PDPageLabels : COSObjectable
{
    private readonly SortedDictionary<int, PDPageLabelRange> _labels;
    private readonly PDDocument _doc;

    public PDPageLabels(PDDocument document)
    {
        _doc = document ?? throw new ArgumentNullException(nameof(document));
        _labels = new SortedDictionary<int, PDPageLabelRange>();
        PDPageLabelRange defaultRange = new();
        defaultRange.SetStyle(PDPageLabelRange.STYLE_DECIMAL);
        _labels[0] = defaultRange;
    }

    public PDPageLabels(PDDocument document, COSDictionary? dict)
        : this(document)
    {
        if (dict is null)
        {
            return;
        }

        PDNumberTreeNode root = new(dict, typeof(PDPageLabelRange));
        FindLabels(root);
    }

    public int GetPageRangeCount() => _labels.Count;

    public PDPageLabelRange? GetPageLabelRange(int startPage) => _labels.TryGetValue(startPage, out PDPageLabelRange? value) ? value : null;

    public void SetLabelItem(int startPage, PDPageLabelRange item)
    {
        if (startPage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startPage), "startPage parameter of SetLabelItem may not be < 0");
        }

        _labels[startPage] = item ?? throw new ArgumentNullException(nameof(item));
    }

    public COSBase GetCOSObject()
    {
        COSArray arr = new();
        foreach (KeyValuePair<int, PDPageLabelRange> kvp in _labels)
        {
            arr.Add(COSInteger.Get(kvp.Key));
            arr.Add(kvp.Value);
        }

        COSDictionary dict = new();
        dict.SetItem(COSName.GetPDFName("Nums"), arr);
        return dict;
    }

    public IDictionary<string, int> GetPageIndicesByLabels()
    {
        int numberOfPages = _doc.GetNumberOfPages();
        Dictionary<string, int> labelMap = new(StringComparer.Ordinal);
        ComputeLabels((pageIndex, label) => labelMap[label] = pageIndex, numberOfPages);
        return labelMap;
    }

    public string[] GetLabelsByPageIndices()
    {
        int numberOfPages = _doc.GetNumberOfPages();
        string[] map = new string[numberOfPages];
        ComputeLabels((pageIndex, label) =>
        {
            if (pageIndex < numberOfPages)
            {
                map[pageIndex] = label;
            }
        }, numberOfPages);
        return map;
    }

    public ISet<int> GetPageIndices() => new SortedSet<int>(_labels.Keys);

    private void FindLabels(PDNumberTreeNode node)
    {
        IList<PDNumberTreeNode>? kids = node.GetKids();
        if (kids is not null)
        {
            foreach (PDNumberTreeNode kid in kids)
            {
                FindLabels(kid);
            }
        }
        else
        {
            IReadOnlyDictionary<int, COSObjectable?>? numbers = node.GetNumbers();
            if (numbers is not null)
            {
                foreach (KeyValuePair<int, COSObjectable?> kvp in numbers)
                {
                    if (kvp.Key >= 0 && kvp.Value is PDPageLabelRange pageLabelRange)
                    {
                        _labels[kvp.Key] = pageLabelRange;
                    }
                }
            }
        }
    }

    private void ComputeLabels(Action<int, string> handler, int numberOfPages)
    {
        using IEnumerator<KeyValuePair<int, PDPageLabelRange>> iterator = _labels.GetEnumerator();
        if (!iterator.MoveNext())
        {
            return;
        }

        int pageIndex = 0;
        KeyValuePair<int, PDPageLabelRange> lastEntry = iterator.Current;
        while (iterator.MoveNext())
        {
            KeyValuePair<int, PDPageLabelRange> entry = iterator.Current;
            int numPages = entry.Key - lastEntry.Key;
            LabelGenerator gen = new(lastEntry.Value, numPages);
            while (gen.HasNext())
            {
                handler(pageIndex, gen.Next());
                pageIndex++;
            }

            lastEntry = entry;
        }

        LabelGenerator finalGenerator = new(lastEntry.Value, numberOfPages - lastEntry.Key);
        while (finalGenerator.HasNext())
        {
            handler(pageIndex, finalGenerator.Next());
            pageIndex++;
        }
    }

    private sealed class LabelGenerator
    {
        private static readonly string[][] Romans =
        [
            ["", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix"],
            ["", "x", "xx", "xxx", "xl", "l", "lx", "lxx", "lxxx", "xc"],
            ["", "c", "cc", "ccc", "cd", "d", "dc", "dcc", "dccc", "cm"]
        ];

        private readonly PDPageLabelRange _labelInfo;
        private readonly int _numPages;
        private int _currentPage;

        public LabelGenerator(PDPageLabelRange label, int pages)
        {
            _labelInfo = label;
            _numPages = pages;
        }

        public bool HasNext() => _currentPage < _numPages;

        public string Next()
        {
            if (!HasNext())
            {
                throw new InvalidOperationException();
            }

            string label = _labelInfo.GetPrefix() ?? string.Empty;
            int nullIndex = label.IndexOf('\0');
            if (nullIndex >= 0)
            {
                label = label[..nullIndex];
            }

            string? style = _labelInfo.GetStyle();
            string suffix = style is null ? string.Empty : GetNumber(_labelInfo.GetStart() + _currentPage, style);
            _currentPage++;
            return label + suffix;
        }

        private static string GetNumber(int pageIndex, string style)
        {
            return style switch
            {
                PDPageLabelRange.STYLE_DECIMAL => pageIndex.ToString(),
                PDPageLabelRange.STYLE_LETTERS_LOWER => MakeLetterLabel(pageIndex),
                PDPageLabelRange.STYLE_LETTERS_UPPER => MakeLetterLabel(pageIndex).ToUpperInvariant(),
                PDPageLabelRange.STYLE_ROMAN_LOWER => MakeRomanLabel(pageIndex),
                PDPageLabelRange.STYLE_ROMAN_UPPER => MakeRomanLabel(pageIndex).ToUpperInvariant(),
                _ => pageIndex.ToString()
            };
        }

        private static string MakeRomanLabel(int pageIndex)
        {
            System.Text.StringBuilder buf = new();
            int power = 0;
            while (power < 3 && pageIndex > 0)
            {
                buf.Insert(0, Romans[power][pageIndex % 10]);
                pageIndex /= 10;
                power++;
            }

            for (int i = 0; i < pageIndex; i++)
            {
                buf.Insert(0, 'm');
            }

            return buf.ToString();
        }

        private static string MakeLetterLabel(int num)
        {
            System.Text.StringBuilder buf = new();
            int numLetters = (num / 26) + Math.Sign(num % 26);
            int letter = (num % 26) + (26 * (1 - Math.Sign(num % 26))) + 'a' - 1;
            for (int i = 0; i < numLetters; i++)
            {
                buf.Append((char)letter);
            }

            return buf.ToString();
        }
    }
}
