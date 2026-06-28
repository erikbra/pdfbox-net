/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * BouncyCastle-backed PKCS#12 certificate loading helpers.
 *
 * PORT_MODE: native-adapter
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PdfBox.Net.Cryptography.Certificates;

/// <summary>
/// Loads PKCS#12 certificates with BouncyCastle without relying on platform key-store support.
/// </summary>
public static class BouncyCastlePkcs12CertificateLoader
{
    public static X509Certificate2Collection LoadPkcs12CollectionFromFile(
        string path,
        string? password,
        string? alias = null)
    {
        ArgumentNullException.ThrowIfNull(path);

        using FileStream input = File.OpenRead(path);
        return LoadPkcs12Collection(input, password, alias);
    }

    public static X509Certificate2Collection LoadPkcs12Collection(
        byte[] data,
        string? password,
        string? alias = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream input = new(data, writable: false);
        return LoadPkcs12Collection(input, password, alias);
    }

    public static X509Certificate2Collection LoadPkcs12Collection(
        Stream keyStore,
        string? password,
        string? alias = null)
    {
        ArgumentNullException.ThrowIfNull(keyStore);

        try
        {
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(keyStore, (password ?? string.Empty).ToCharArray());

            string selectedAlias = SelectKeyAlias(store, alias);
            AsymmetricKeyEntry keyEntry = store.GetKey(selectedAlias)
                ?? throw new IOException($"PKCS#12 key store alias '{selectedAlias}' does not contain a private key.");
            X509CertificateEntry certificateEntry = store.GetCertificate(selectedAlias)
                ?? store.GetCertificateChain(selectedAlias).FirstOrDefault()
                ?? throw new IOException($"PKCS#12 key store alias '{selectedAlias}' does not contain a certificate.");

            X509Certificate2Collection collection = [];
            AddIfMissing(
                collection,
                CreateCertificateWithPrivateKey(certificateEntry.Certificate, keyEntry.Key));

            foreach (X509CertificateEntry chainEntry in store.GetCertificateChain(selectedAlias))
            {
                AddIfMissing(collection, X509CertificateLoader.LoadCertificate(chainEntry.Certificate.GetEncoded()));
            }

            foreach (string storeAlias in store.Aliases)
            {
                X509CertificateEntry? entry = store.GetCertificate(storeAlias);
                if (entry is not null)
                {
                    AddIfMissing(collection, X509CertificateLoader.LoadCertificate(entry.Certificate.GetEncoded()));
                }
            }

            return collection;
        }
        catch (Exception ex) when (ex is not IOException)
        {
            throw new IOException("Could not load certificates from the PKCS#12 key store.", ex);
        }
    }

    public static X509Certificate2 LoadCertificateWithPrivateKey(
        Stream keyStore,
        string? password,
        string? alias = null)
    {
        X509Certificate2Collection collection = LoadPkcs12Collection(keyStore, password, alias);
        return collection
            .OfType<X509Certificate2>()
            .FirstOrDefault(certificate => certificate.HasPrivateKey)
            ?? throw new IOException("PKCS#12 key store does not contain a certificate with a private key.");
    }

    private static string SelectKeyAlias(Pkcs12Store store, string? alias)
    {
        if (!string.IsNullOrWhiteSpace(alias))
        {
            if (!store.ContainsAlias(alias))
            {
                throw new IOException($"PKCS#12 key store does not contain alias '{alias}'.");
            }

            if (!store.IsKeyEntry(alias))
            {
                throw new IOException($"PKCS#12 key store alias '{alias}' is not a key entry.");
            }

            return alias;
        }

        return store.Aliases.FirstOrDefault(store.IsKeyEntry)
            ?? throw new IOException("PKCS#12 key store does not contain a private-key entry.");
    }

    private static X509Certificate2 CreateCertificateWithPrivateKey(
        BcX509Certificate certificate,
        AsymmetricKeyParameter privateKey)
    {
        X509Certificate2 x509 = X509CertificateLoader.LoadCertificate(certificate.GetEncoded());
        return privateKey switch
        {
            RsaPrivateCrtKeyParameters rsaPrivateKey => x509.CopyWithPrivateKey(ToRSA(rsaPrivateKey)),
            _ => throw new IOException("PKCS#12 loading currently supports RSA private keys.")
        };
    }

    private static RSA ToRSA(RsaPrivateCrtKeyParameters privateKey)
    {
        RSA rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
            Exponent = privateKey.PublicExponent.ToByteArrayUnsigned(),
            D = privateKey.Exponent.ToByteArrayUnsigned(),
            P = privateKey.P.ToByteArrayUnsigned(),
            Q = privateKey.Q.ToByteArrayUnsigned(),
            DP = privateKey.DP.ToByteArrayUnsigned(),
            DQ = privateKey.DQ.ToByteArrayUnsigned(),
            InverseQ = privateKey.QInv.ToByteArrayUnsigned()
        });
        return rsa;
    }

    private static void AddIfMissing(X509Certificate2Collection collection, X509Certificate2 certificate)
    {
        foreach (X509Certificate2 existing in collection)
        {
            if (existing.RawDataMemory.Span.SequenceEqual(certificate.RawDataMemory.Span))
            {
                return;
            }
        }

        collection.Add(certificate);
    }
}
