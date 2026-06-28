# PdfBox.Net.Cryptography

Optional cryptography backend for PdfBox.Net public-key PDF security.

This package depends on `BouncyCastle.Cryptography` and registers a provider for
the core `PublicKeySecurityHandler`. The core `PdfBox.Net` assembly keeps the
Java-shaped public-key API but does not reference BouncyCastle directly.

Currently covered:

- PKCS#12 key-store loading for Java-compatible `Loader.LoadPDF(..., keyStore, alias)`
  overloads
- PKCS#12 certificate loading fallback for platforms where native
  `X509KeyStorageFlags.EphemeralKeySet` loading is unavailable
- CMS EnvelopedData recipient creation for `PublicKeyProtectionPolicy`
- CMS EnvelopedData recipient decryption for public-key encrypted PDFs
- OCSP request/response helpers that preserve raw OCSP response bytes for
  DSS/LTV embedding

Usage:

```csharp
using PdfBox.Net.Cryptography.PDModel.Encryption;

BouncyCastlePublicKeySecurityProvider.Register();
```
