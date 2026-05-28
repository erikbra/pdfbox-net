/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDXFAResource.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDXFAResource : COSObjectable
{
    private readonly COSBase _xfa;

    public PDXFAResource(COSBase xfaBase)
    {
        _xfa = xfaBase ?? throw new ArgumentNullException(nameof(xfaBase));
    }

    public COSBase GetCOSObject() => _xfa;

    public byte[] GetBytes()
    {
        if (_xfa is COSArray array)
        {
            return GetBytesFromPacket(array);
        }

        if (_xfa is COSStream stream)
        {
            return GetBytesFromStream(stream);
        }

        return [];
    }

    private static byte[] GetBytesFromPacket(COSArray cosArray)
    {
        using MemoryStream output = new();
        for (int i = 1; i < cosArray.Size(); i += 2)
        {
            if (cosArray.GetObject(i) is COSStream stream)
            {
                output.Write(GetBytesFromStream(stream));
            }
        }

        return output.ToArray();
    }

    private static byte[] GetBytesFromStream(COSStream stream)
    {
        using Stream input = stream.CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }

    public XmlDocument GetDocument() => XMLUtil.Parse(new MemoryStream(GetBytes()), true);
}
