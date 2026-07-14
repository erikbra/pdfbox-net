using System.Text;

namespace PdfBox.Net.Util;

/// <summary>
/// Normalizes PDF text while allowing hosts with limited Unicode data to provide a compatibility-normalization implementation.
/// </summary>
public static class PdfStringNormalization
{
    private static Func<string, NormalizationForm, string>? _compatibilityNormalizer;

    /// <summary>
    /// Gets or sets the optional host implementation used when the runtime does not include Unicode compatibility-normalization data.
    /// </summary>
    /// <remarks>
    /// Browser and WASI .NET runtimes omit the data required by <see cref="NormalizationForm.FormKC"/> and
    /// <see cref="NormalizationForm.FormKD"/>. A browser host can set this delegate to an implementation backed by
    /// the browser's Unicode normalizer before processing a document.
    /// </remarks>
    public static Func<string, NormalizationForm, string>? CompatibilityNormalizer
    {
        get => Volatile.Read(ref _compatibilityNormalizer);
        set => Volatile.Write(ref _compatibilityNormalizer, value);
    }

    /// <summary>
    /// Normalizes <paramref name="value"/> using the requested Unicode normalization form.
    /// </summary>
    public static string Normalize(string value, NormalizationForm normalizationForm)
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            return value.Normalize(normalizationForm);
        }
        catch (PlatformNotSupportedException) when (
            normalizationForm is NormalizationForm.FormKC or NormalizationForm.FormKD &&
            CompatibilityNormalizer is { } compatibilityNormalizer)
        {
            return compatibilityNormalizer(value, normalizationForm);
        }
    }
}
