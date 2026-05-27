/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/SigningSupport.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed class SigningSupport : ExternalSigningSupport, IDisposable
{
    private readonly Func<Stream> _contentFactory;
    private readonly Action<byte[]> _signatureWriter;

    public SigningSupport(Func<Stream> contentFactory, Action<byte[]> signatureWriter)
    {
        _contentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        _signatureWriter = signatureWriter ?? throw new ArgumentNullException(nameof(signatureWriter));
    }

    public Stream GetContent() => _contentFactory();

    public void SetSignature(byte[] signature)
    {
        ArgumentNullException.ThrowIfNull(signature);
        _signatureWriter(signature);
    }

    public void Dispose()
    {
    }
}
