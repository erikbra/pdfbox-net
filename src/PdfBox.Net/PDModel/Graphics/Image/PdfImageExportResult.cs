namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Browser-safe exported image bytes.
/// </summary>
public sealed class PdfImageExportResult
{
    public PdfImageExportResult(string contentType, string fileExtension, byte[] data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);
        ArgumentNullException.ThrowIfNull(data);

        ContentType = contentType;
        FileExtension = fileExtension.TrimStart('.');
        Data = data.ToArray();
    }

    /// <summary>
    /// Gets the exported image MIME type.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the exported image file extension without a leading dot.
    /// </summary>
    public string FileExtension { get; }

    /// <summary>
    /// Gets the exported image bytes.
    /// </summary>
    public byte[] Data { get; }
}
