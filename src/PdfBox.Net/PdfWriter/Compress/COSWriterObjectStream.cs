/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/compress/COSWriterObjectStream.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PdfWriter.Compress;

public sealed class COSWriterObjectStream
{
    private readonly COSWriterCompressionPool _compressionPool;
    private readonly List<COSObjectKey> _preparedKeys = [];
    private readonly List<COSBase> _preparedObjects = [];

    public COSWriterObjectStream(COSWriterCompressionPool compressionPool)
    {
        _compressionPool = compressionPool ?? throw new ArgumentNullException(nameof(compressionPool));
    }

    public void PrepareStreamObject(COSObjectKey key, COSBase obj)
    {
        if (obj is null)
        {
            return;
        }

        _preparedKeys.Add(key);
        _preparedObjects.Add(obj is COSObject indirect && indirect.GetObject() is COSBase inner ? inner : obj);
    }

    public IReadOnlyList<COSObjectKey> GetPreparedKeys() => _preparedKeys;

    public COSStream WriteObjectsToStream(COSStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        stream.SetItem(COSName.TYPE, COSName.GetPDFName("ObjStm"));
        stream.SetInt(COSName.N, _preparedKeys.Count);

        using Stream output = stream.CreateOutputStream(COSName.FLATE_DECODE);
        for (int i = 0; i < _preparedObjects.Count; i++)
        {
            output.Write(COSWriter.Serialize(_preparedObjects[i]));
            output.WriteByte((byte)' ');
        }

        return stream;
    }
}
