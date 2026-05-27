/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/FieldUtils.java
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

namespace PdfBox.Net.PDModel.Interactive.Form;

internal static class FieldUtils
{
    internal sealed record KeyValue(string Key, string Value);

    public static List<string> GetPairableItems(COSBase? values, int pairIndex)
    {
        List<string> result = [];
        if (values is COSString single)
        {
            result.Add(single.GetString());
            return result;
        }

        if (values is not COSArray array)
        {
            return result;
        }

        for (int i = 0; i < array.Size(); i++)
        {
            COSBase? entry = array.GetObject(i);
            if (entry is COSString text)
            {
                result.Add(text.GetString());
                continue;
            }

            if (entry is COSArray pair && pair.Size() >= 2)
            {
                int index = Math.Clamp(pairIndex, 0, 1);
                if (pair.GetObject(index) is COSString pairValue)
                {
                    result.Add(pairValue.GetString());
                }
            }
        }

        return result;
    }

    public static List<KeyValue> ToKeyValueList(IList<string> keys, IList<string> values)
    {
        List<KeyValue> pairs = new(keys.Count);
        for (int i = 0; i < keys.Count; i++)
        {
            pairs.Add(new KeyValue(keys[i], values[i]));
        }

        return pairs;
    }

    public static void SortByValue(List<KeyValue> pairs)
    {
        pairs.Sort((a, b) => StringComparer.Ordinal.Compare(a.Value, b.Value));
    }
}
