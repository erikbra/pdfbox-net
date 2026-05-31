/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/Stream.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Streampane;

/// <summary>Provides decoded and partially decoded views of a COS stream.</summary>
public sealed class Stream
{
    public const string DECODED = "Decoded (Plain Text)";
    public const string IMAGE = "Image";

    private readonly PdfBox.Net.COS.COSStream _stream;
    private readonly bool _isThumb;
    private readonly bool _isImage;
    private readonly bool _isXmlMetadata;
    private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>?> _filters;

    public Stream(PdfBox.Net.COS.COSStream cosStream, bool isThumb)
    {
        _stream = cosStream;
        _isThumb = isThumb;
        _isImage = IsImageStream(cosStream, isThumb);
        _isXmlMetadata = IsXmlMetadataStream(cosStream);
        _filters = CreateFilterList(cosStream);
    }

    public bool IsImage() => _isImage;

    public bool IsXmlMetadata() => _isXmlMetadata;

    public System.Collections.Generic.IReadOnlyList<string> GetFilterList() => _filters.Keys.ToList();

    public System.IO.Stream? GetStream(string key)
    {
        try
        {
            if (DECODED.Equals(key, System.StringComparison.Ordinal))
            {
                return _stream.CreateInputStream();
            }

            if (GetFilteredLabel().Equals(key, System.StringComparison.Ordinal))
            {
                return _stream.CreateRawInputStream();
            }

            return _filters.TryGetValue(key, out System.Collections.Generic.List<string>? stopFilters) && stopFilters is not null
                ? new PdfBox.Net.PDModel.Common.PDStream(_stream).CreateInputStream(stopFilters)
                : null;
        }
        catch
        {
            return null;
        }
    }

    public string? GetDecodedText(System.Text.Encoding? encoding = null)
    {
        using System.IO.Stream? input = GetStream(DECODED);
        if (input is null)
        {
            return null;
        }

        using System.IO.StreamReader reader = new(input, encoding ?? System.Text.Encoding.UTF8, true, leaveOpen: false);
        return reader.ReadToEnd();
    }

    public byte[]? GetImageData(PdfBox.Net.PDModel.Resources.PDResources? resources = null)
    {
        if (!_isImage)
        {
            return null;
        }

        PdfBox.Net.PDModel.Common.PDStream stream = new(_stream);
        PdfBox.Net.PDModel.Graphics.Image.PDImageXObject image = new(stream, resources);
        return image.GetImageData();
    }

    private string GetFilteredLabel()
    {
        System.Text.StringBuilder builder = new();
        builder.Append("Encoded (");
        PdfBox.Net.COS.COSBase? baseFilters = _stream.GetFilters();
        if (baseFilters is PdfBox.Net.COS.COSName cosName)
        {
            builder.Append(cosName.GetName());
        }
        else if (baseFilters is PdfBox.Net.COS.COSArray array)
        {
            for (int i = 0; i < array.Size(); i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(((PdfBox.Net.COS.COSName)array.Get(i)!).GetName());
            }
        }
        builder.Append(')');
        return builder.ToString();
    }

    private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>?> CreateFilterList(PdfBox.Net.COS.COSStream stream)
    {
        System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>?> filterList = new(System.StringComparer.Ordinal);
        if (_isImage)
        {
            filterList[IMAGE] = null;
        }

        filterList[DECODED] = null;
        PdfBox.Net.PDModel.Common.PDStream pdStream = new(stream);
        int filtersSize = pdStream.GetFilters().Count;
        for (int i = filtersSize - 1; i >= 1; i--)
        {
            filterList[GetPartialStreamCommand(i)] = GetStopFilterList(i);
        }

        filterList[GetFilteredLabel()] = null;
        return filterList;
    }

    private string GetPartialStreamCommand(int indexOfStopFilter)
    {
        System.Collections.Generic.IList<PdfBox.Net.COS.COSName> availableFilters = new PdfBox.Net.PDModel.Common.PDStream(_stream).GetFilters();
        return "Keep " + string.Join(" & ", availableFilters.Skip(indexOfStopFilter).Select(static filter => filter.GetName())) + "...";
    }

    private System.Collections.Generic.List<string> GetStopFilterList(int stopFilterIndex)
    {
        System.Collections.Generic.IList<PdfBox.Net.COS.COSName> availableFilters = new PdfBox.Net.PDModel.Common.PDStream(_stream).GetFilters();
        return [availableFilters[stopFilterIndex].GetName()];
    }

    private static bool IsImageStream(PdfBox.Net.COS.COSDictionary dictionary, bool isThumb)
    {
        if (isThumb)
        {
            return true;
        }

        return dictionary.ContainsKey(PdfBox.Net.COS.COSName.SUBTYPE) &&
               Equals(dictionary.GetCOSName(PdfBox.Net.COS.COSName.SUBTYPE), PdfBox.Net.COS.COSName.GetPDFName("Image"));
    }

    private static bool IsXmlMetadataStream(PdfBox.Net.COS.COSDictionary dictionary)
    {
        return dictionary.ContainsKey(PdfBox.Net.COS.COSName.SUBTYPE) &&
               Equals(dictionary.GetCOSName(PdfBox.Net.COS.COSName.SUBTYPE), PdfBox.Net.COS.COSName.GetPDFName("XML"));
    }
}
