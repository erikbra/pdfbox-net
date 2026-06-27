/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDStructureTreeRoot.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDStructureTreeRoot
{
    public PDNameTreeNode<PDStructureElement>? IDTree
    {
        get => GetIDTree();
        set => SetIDTree(value!);
    }

    public COSBase? K
    {
        get => GetK();
        set => SetK(value!);
    }

    public int ParentTreeNextKey
    {
        get => GetParentTreeNextKey();
        set => SetParentTreeNextKey(value);
    }
}
