/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/PDFTreeModel.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>A class to model a PDF document as a tree structure.</summary>
public class PDFTreeModel
{
    private object? _root;

    public PDFTreeModel()
    {
    }

    public PDFTreeModel(PdfBox.Net.PDModel.PDDocument doc)
    {
        _root = doc.GetDocument().GetTrailer();
    }

    public PDFTreeModel(DocumentEntry docEntry)
    {
        _root = docEntry;
    }

    public PDFTreeModel(XrefEntries xrefEntries)
    {
        _root = xrefEntries;
    }

    public object? GetRoot() => _root;

    public object? GetChild(object parent, int index)
    {
        return parent switch
        {
            PdfBox.Net.COS.COSArray array => new ArrayEntry
            {
                Index = index,
                Value = array.GetObject(index),
                Item = array.Get(index)
            },
            PdfBox.Net.COS.COSDictionary dictionary => GetDictionaryChild(dictionary, index),
            MapEntry mapEntry when mapEntry.GetValue() is not null => GetChild(mapEntry.GetValue()!, index),
            ArrayEntry arrayEntry when arrayEntry.GetValue() is not null => GetChild(arrayEntry.GetValue()!, index),
            DocumentEntry documentEntry => documentEntry.GetPage(index),
            XrefEntries xrefEntries => xrefEntries.GetXrefEntry(index),
            XrefEntry xrefEntry => new ArrayEntry
            {
                Index = index,
                Value = xrefEntry.GetObject(),
                Item = xrefEntry.GetCOSObject()
            },
            PageEntry pageEntry => GetChild(pageEntry.GetDict(), index),
            PdfBox.Net.COS.COSObject cosObject => cosObject.GetObject(),
            _ => throw new System.ArgumentException($"Unknown COS type {parent.GetType().FullName}")
        };
    }

    public int GetChildCount(object parent)
    {
        return parent switch
        {
            PdfBox.Net.COS.COSArray array => array.Size(),
            PdfBox.Net.COS.COSDictionary dictionary => dictionary.Size(),
            MapEntry mapEntry when mapEntry.GetValue() is not null => GetChildCount(mapEntry.GetValue()!),
            ArrayEntry arrayEntry when arrayEntry.GetValue() is not null => GetChildCount(arrayEntry.GetValue()!),
            DocumentEntry documentEntry => documentEntry.GetPageCount(),
            XrefEntries xrefEntries => xrefEntries.GetXrefEntryCount(),
            XrefEntry => 1,
            PageEntry pageEntry => GetChildCount(pageEntry.GetDict()),
            PdfBox.Net.COS.COSObject => 1,
            _ => 0
        };
    }

    public int GetIndexOfChild(object? parent, object? child)
    {
        if (parent is null || child is null)
        {
            return -1;
        }

        return parent switch
        {
            PdfBox.Net.COS.COSArray array when child is ArrayEntry arrayEntry => arrayEntry.GetIndex(),
            PdfBox.Net.COS.COSArray array when child is PdfBox.Net.COS.COSBase cosBase => array.IndexOf(cosBase),
            PdfBox.Net.COS.COSDictionary dictionary when child is MapEntry mapEntry => GetDictionaryKeys(dictionary).FindIndex(key => Equals(key, mapEntry.GetKey())),
            MapEntry mapEntry when mapEntry.GetValue() is not null => GetIndexOfChild(mapEntry.GetValue(), child),
            ArrayEntry arrayEntry when arrayEntry.GetValue() is not null => GetIndexOfChild(arrayEntry.GetValue(), child),
            DocumentEntry documentEntry when child is PageEntry pageEntry => documentEntry.IndexOf(pageEntry),
            XrefEntries when child is XrefEntry xrefEntry => xrefEntry.GetIndex(),
            XrefEntry => 0,
            PageEntry pageEntry => GetIndexOfChild(pageEntry.GetDict(), child),
            PdfBox.Net.COS.COSObject => 0,
            _ => throw new System.ArgumentException($"Unknown COS type {parent.GetType().FullName}")
        };
    }

    public bool IsLeaf(object? node)
    {
        if (node is null)
        {
            return true;
        }

        return !(node is PdfBox.Net.COS.COSDictionary ||
                 node is PdfBox.Net.COS.COSArray ||
                 node is PdfBox.Net.COS.COSDocument ||
                 node is DocumentEntry ||
                 node is XrefEntries ||
                 (node is XrefEntry xrefEntry && !IsLeaf(xrefEntry.GetCOSObject())) ||
                 node is PageEntry ||
                 node is PdfBox.Net.COS.COSObject ||
                 (node is MapEntry mapEntry && !IsLeaf(mapEntry.GetValue())) ||
                 (node is ArrayEntry arrayEntry && !IsLeaf(arrayEntry.GetValue())));
    }

    private static MapEntry GetDictionaryChild(PdfBox.Net.COS.COSDictionary dictionary, int index)
    {
        System.Collections.Generic.List<PdfBox.Net.COS.COSName> keys = GetDictionaryKeys(dictionary);
        PdfBox.Net.COS.COSName key = keys[index];
        return new MapEntry
        {
            Key = key,
            Value = dictionary.GetDictionaryObject(key),
            Item = dictionary.GetItem(key)
        };
    }

    private static System.Collections.Generic.List<PdfBox.Net.COS.COSName> GetDictionaryKeys(PdfBox.Net.COS.COSDictionary dictionary)
    {
        System.Collections.Generic.List<PdfBox.Net.COS.COSName> keys = new(dictionary.KeySet());
        keys.Sort(static (a, b) => string.Compare(a.GetName(), b.GetName(), System.StringComparison.Ordinal));
        return keys;
    }
}
