namespace PdfBox.Net.Layout;

/// <summary>
/// A browser-safe embedded font program extracted from a PDF.
/// </summary>
public sealed class PdfLayoutFontAsset
{
    public PdfLayoutFontAsset(
        string assetId,
        IReadOnlyList<string> fontNames,
        string relativePath,
        string contentType,
        string cssFormat,
        byte[] data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
        ArgumentNullException.ThrowIfNull(fontNames);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(cssFormat);
        ArgumentNullException.ThrowIfNull(data);

        AssetId = assetId;
        FontNames = fontNames.Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        RelativePath = relativePath.Replace('\\', '/');
        ContentType = contentType;
        CssFormat = cssFormat;
        Data = data.ToArray();
    }

    /// <summary>
    /// Gets the stable identifier for this deduplicated font program.
    /// </summary>
    public string AssetId { get; }

    /// <summary>
    /// Gets the PDF base-font names which use this program.
    /// </summary>
    public IReadOnlyList<string> FontNames { get; }

    /// <summary>
    /// Gets the browser-relative output path.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the MIME type of the font program.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the CSS <c>format()</c> value for the font program.
    /// </summary>
    public string CssFormat { get; }

    /// <summary>
    /// Gets the raw OpenType or TrueType program bytes.
    /// </summary>
    public byte[] Data { get; }
}
