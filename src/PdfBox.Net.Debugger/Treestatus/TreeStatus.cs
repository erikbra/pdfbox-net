/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/treestatus/TreeStatus.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Treestatus;

/// <summary>Tracks the current tree path as a navigable string.</summary>
public sealed class TreeStatus
{
    private readonly object _rootNode;

    public TreeStatus(object rootNode)
    {
        _rootNode = rootNode;
    }

    public string GetStringForPath(object[] path)
    {
        if (path.Length <= 1)
        {
            return string.Empty;
        }

        System.Collections.Generic.List<string> nodes = new();
        for (int i = 1; i < path.Length; i++)
        {
            nodes.Add(GetObjectName(path[i]));
        }
        return string.Join("/", nodes);
    }

    public object[]? GetPathForString(string statusString)
    {
        System.Collections.Generic.List<string>? nodes = ParsePathString(statusString);
        if (nodes is null)
        {
            return null;
        }

        object current = _rootNode;
        System.Collections.Generic.List<object> path = new() { current };
        foreach (string node in nodes)
        {
            object? next = SearchNode(current, node);
            if (next is null)
            {
                return null;
            }

            path.Add(next);
            current = next;
        }

        return path.ToArray();
    }

    private static string GetObjectName(object treeNode)
    {
        return treeNode switch
        {
            PdfBox.Net.Debugger.Ui.MapEntry entry => entry.GetKey()?.GetName() ?? "(null)",
            PdfBox.Net.Debugger.Ui.ArrayEntry entry => $"[{entry.GetIndex()}]",
            PdfBox.Net.Debugger.Ui.PageEntry entry => entry.GetPath(),
            PdfBox.Net.Debugger.Ui.XrefEntry entry => entry.GetPath(),
            _ => throw new System.ArgumentException($"Unknown treeNode type: {treeNode.GetType().FullName}")
        };
    }

    private static System.Collections.Generic.List<string>? ParsePathString(string path)
    {
        System.Collections.Generic.List<string> nodes = new();
        foreach (string part in path.Split('/'))
        {
            string node = part.Trim();
            if (node.StartsWith("[", System.StringComparison.Ordinal))
            {
                node = node.Replace("]", string.Empty, System.StringComparison.Ordinal)
                           .Replace("[", string.Empty, System.StringComparison.Ordinal)
                           .Trim();
            }

            if (node.Length == 0)
            {
                return null;
            }

            nodes.Add(node);
        }
        return nodes;
    }

    private static object? SearchNode(object obj, string searchStr)
    {
        if (obj is PdfBox.Net.Debugger.Ui.MapEntry mapEntry)
        {
            obj = mapEntry.GetValue()!;
        }
        else if (obj is PdfBox.Net.Debugger.Ui.ArrayEntry arrayEntry)
        {
            obj = arrayEntry.GetValue()!;
        }
        else if (obj is PdfBox.Net.Debugger.Ui.XrefEntry xrefEntry)
        {
            obj = xrefEntry.GetObject()!;
        }

        if (obj is PdfBox.Net.COS.COSObject cosObject)
        {
            obj = cosObject.GetObject()!;
        }

        if (obj is PdfBox.Net.COS.COSDictionary dictionary)
        {
            if (dictionary.ContainsKey(searchStr))
            {
                PdfBox.Net.COS.COSName key = PdfBox.Net.COS.COSName.GetPDFName(searchStr);
                return new PdfBox.Net.Debugger.Ui.MapEntry
                {
                    Key = key,
                    Value = dictionary.GetDictionaryObject(key),
                    Item = dictionary.GetItem(key)
                };
            }
        }
        else if (obj is PdfBox.Net.COS.COSArray array)
        {
            if (!int.TryParse(searchStr, out int index) || index > array.Size() - 1)
            {
                return null;
            }

            return new PdfBox.Net.Debugger.Ui.ArrayEntry
            {
                Index = index,
                Value = array.GetObject(index),
                Item = array.Get(index)
            };
        }
        else if (obj is PdfBox.Net.Debugger.Ui.XrefEntries xrefEntries)
        {
            for (int i = 0; i < xrefEntries.GetXrefEntryCount(); i++)
            {
                PdfBox.Net.Debugger.Ui.XrefEntry entry = xrefEntries.GetXrefEntry(i);
                if (entry.ToString() == searchStr)
                {
                    return entry;
                }
            }
        }

        return null;
    }
}
