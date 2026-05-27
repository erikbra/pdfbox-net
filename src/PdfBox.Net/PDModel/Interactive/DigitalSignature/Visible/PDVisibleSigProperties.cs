/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSigProperties.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDVisibleSigProperties
{
    private string? _signerName;
    private string? _signerLocation;
    private string? _signatureReason;
    private bool _visualSignEnabled;
    private int _page;
    private int _preferredSize;

    private Stream? _visibleSignature;
    private PDVisibleSignDesigner? _pdVisibleSignature;

    public void BuildSignature()
    {
        PDFTemplateBuilder builder = new PDVisibleSigBuilder();
        PDFTemplateCreator creator = new(builder);
        SetVisibleSignature(creator.BuildPDF(GetPdVisibleSignature()));
    }

    public string? GetSignerName() => _signerName;
    public PDVisibleSigProperties SignerName(string signerName) { _signerName = signerName; return this; }

    public string? GetSignerLocation() => _signerLocation;
    public PDVisibleSigProperties SignerLocation(string signerLocation) { _signerLocation = signerLocation; return this; }

    public string? GetSignatureReason() => _signatureReason;
    public PDVisibleSigProperties SignatureReason(string signatureReason) { _signatureReason = signatureReason; return this; }

    public int GetPage() => _page;
    public PDVisibleSigProperties Page(int page) { _page = page; return this; }

    public int GetPreferredSize() => _preferredSize;
    public PDVisibleSigProperties PreferredSize(int preferredSize) { _preferredSize = preferredSize; return this; }

    public bool IsVisualSignEnabled() => _visualSignEnabled;
    public PDVisibleSigProperties VisualSignEnabled(bool visualSignEnabled) { _visualSignEnabled = visualSignEnabled; return this; }

    public PDVisibleSignDesigner GetPdVisibleSignature() => _pdVisibleSignature ?? throw new InvalidOperationException("Visible signature designer is not set.");

    public PDVisibleSigProperties SetPdVisibleSignature(PDVisibleSignDesigner pdVisibleSignature)
    {
        _pdVisibleSignature = pdVisibleSignature;
        return this;
    }

    public Stream GetVisibleSignature() => _visibleSignature ?? throw new InvalidOperationException("Visible signature stream is not set.");

    public void SetVisibleSignature(Stream visibleSignature)
    {
        _visibleSignature = visibleSignature;
    }
}
