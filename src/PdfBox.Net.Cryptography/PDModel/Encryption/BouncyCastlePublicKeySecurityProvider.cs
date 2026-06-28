/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * BouncyCastle-backed public-key PDF security provider.
 *
 * PORT_MODE: native-adapter
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using PdfBox.Net.Cryptography.Certificates;
using PdfBox.Net.PDModel.Encryption;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PdfBox.Net.Cryptography.PDModel.Encryption;

/// <summary>
/// Public-key security provider backed by BouncyCastle.NET CMS primitives.
/// </summary>
public sealed class BouncyCastlePublicKeySecurityProvider : IPublicKeySecurityProvider
{
    public static BouncyCastlePublicKeySecurityProvider Instance { get; } = new();

    private BouncyCastlePublicKeySecurityProvider()
    {
    }

    public static void Register()
    {
        PublicKeySecurityProvider.Register(Instance);
    }

    public PublicKeyDecryptionMaterial LoadDecryptionMaterial(Stream keyStore, string? password, string? alias)
    {
        ArgumentNullException.ThrowIfNull(keyStore);

        X509Certificate2 certificate =
            BouncyCastlePkcs12CertificateLoader.LoadCertificateWithPrivateKey(keyStore, password, alias);
        return new PublicKeyDecryptionMaterial(certificate, password);
    }

    public byte[]? DecryptRecipient(byte[] recipientBytes, X509Certificate2 certificate, AsymmetricAlgorithm privateKey)
    {
        ArgumentNullException.ThrowIfNull(recipientBytes);
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(privateKey);

        try
        {
            BcX509Certificate bcCertificate = DotNetUtilities.FromX509Certificate(certificate);
            AsymmetricKeyParameter bcPrivateKey = DotNetUtilities.GetKeyPair(privateKey).Private;
            CmsEnvelopedData data = new(recipientBytes);
            foreach (RecipientInformation recipient in data.GetRecipientInfos().GetRecipients())
            {
                if (recipient.RecipientID.Match(bcCertificate))
                {
                    return recipient.GetContent(bcPrivateKey);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Could not decrypt CMS recipient field.", ex);
        }
    }

    public byte[] CreateRecipientField(byte[] seed, X509Certificate2 certificate, int permissionBytes)
    {
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(certificate);
        if (seed.Length != 20)
        {
            throw new ArgumentException("Public-key security seed must contain 20 bytes.", nameof(seed));
        }

        try
        {
            byte[] pkcs7Input = new byte[24];
            Array.Copy(seed, 0, pkcs7Input, 0, seed.Length);
            pkcs7Input[20] = (byte)(permissionBytes >> 24);
            pkcs7Input[21] = (byte)(permissionBytes >> 16);
            pkcs7Input[22] = (byte)(permissionBytes >> 8);
            pkcs7Input[23] = (byte)permissionBytes;

            BcX509Certificate bcCertificate = DotNetUtilities.FromX509Certificate(certificate);
            CmsEnvelopedDataGenerator generator = new();
            generator.AddKeyTransRecipient(bcCertificate);
            CmsEnvelopedData data = generator.Generate(
                new CmsProcessableByteArray(pkcs7Input),
                CmsEnvelopedGenerator.RC2Cbc,
                128);
            return data.GetEncoded();
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Could not create CMS recipient field.", ex);
        }
    }

}
