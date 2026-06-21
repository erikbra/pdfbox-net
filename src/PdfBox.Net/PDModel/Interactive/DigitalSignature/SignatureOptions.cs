/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/SignatureOptions.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed class SignatureOptions : IDisposable
{
    public const int DEFAULT_SIGNATURE_SIZE = 0x2500;

    private COSDocument? _visualSignature;
    private int _preferredSignatureSize = DEFAULT_SIGNATURE_SIZE;
    private int _pageNo;

    public void SetPage(int pageNo) => _pageNo = pageNo;

    public int GetPage() => _pageNo;

    public void SetVisualSignature(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using Stream input = file.OpenRead();
        SetVisualSignature(input);
    }

    public void SetVisualSignature(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        PDFParser parser = new(input);
        _visualSignature = parser.Parse().Document;
    }

    public void SetVisualSignature(PDVisibleSigProperties visSignatureProperties)
    {
        ArgumentNullException.ThrowIfNull(visSignatureProperties);
        using Stream input = visSignatureProperties.GetVisibleSignature();
        SetVisualSignature(input);
    }

    public COSDocument? GetVisualSignature() => _visualSignature;

    public int GetPreferredSignatureSize() => _preferredSignatureSize;

    public void SetPreferredSignatureSize(int size)
    {
        if (size > 0)
        {
            _preferredSignatureSize = size;
        }
    }

    public void Dispose()
    {
        _visualSignature?.Dispose();
    }
}
