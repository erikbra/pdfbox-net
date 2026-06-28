/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Optional provider seam for public-key PDF security CMS operations.
 *
 * PORT_MODE: native-adapter
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.PDModel.Encryption;

/// <summary>
/// Provides CMS recipient operations required by the public-key PDF security handler.
/// </summary>
public interface IPublicKeySecurityProvider
{
    /// <summary>
    /// Loads public-key decryption material from a PKCS#12 key store.
    /// </summary>
    PublicKeyDecryptionMaterial LoadDecryptionMaterial(Stream keyStore, string? password, string? alias);

    /// <summary>
    /// Decrypts a CMS recipient field for the supplied certificate/private-key pair.
    /// </summary>
    /// <returns>The 24-byte public-key envelope payload, or <see langword="null"/> when the recipient does not match.</returns>
    byte[]? DecryptRecipient(byte[] recipientBytes, X509Certificate2 certificate, AsymmetricAlgorithm privateKey);

    /// <summary>
    /// Creates a CMS recipient field containing the public-key seed and permission bytes.
    /// </summary>
    byte[] CreateRecipientField(byte[] seed, X509Certificate2 certificate, int permissionBytes);
}

/// <summary>
/// Registry for the optional public-key security provider.
/// </summary>
public static class PublicKeySecurityProvider
{
    private static IPublicKeySecurityProvider? _current;

    public static bool IsRegistered => _current is not null;

    internal static IPublicKeySecurityProvider Current =>
        _current ?? throw new NotSupportedException(
            "Public-key encrypted PDF support requires a registered public-key security provider. " +
            "Reference the optional PdfBox.Net.Cryptography package and call " +
            "BouncyCastlePublicKeySecurityProvider.Register() before loading or saving public-key encrypted PDFs.");

    public static void Register(IPublicKeySecurityProvider provider)
    {
        _current = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    internal static void ResetForTesting()
    {
        _current = null;
    }
}
