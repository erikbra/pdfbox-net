# Issue 014 — PDF digital signing and crypto APIs

## Summary

Implement PDF digital signing support using .NET cryptographic primitives (BouncyCastle.NET or
`System.Security.Cryptography`) so that the Signature example suite compiles and passes its
tests.

## Background

The upstream Java `examples/signature` package uses:
- `java.security.KeyStore` (PKCS#12 / JKS keystores) → .NET: `X509Certificate2` / `Pkcs12Store`
- BouncyCastle Java (`org.bouncycastle.*`) → .NET: `BouncyCastle.Cryptography` NuGet package
- `javax.net.ssl` / TSA HTTP requests → .NET: `HttpClient`
- CRL / OCSP verification → .NET: `BouncyCastle.Cryptography` OCSP/CRL APIs

## Required API surface

### Signing infrastructure
- `PDSignature` — PDF signature dictionary model (already present in PDModel)
- `SignatureInterface` (or equivalent) — callback for signing byte ranges
- `PDDocument.AddSignature(PDSignature, SignatureInterface)` — wires the signature into the PDF
- `PDDocument.SaveIncrementalForExternalSigning(Stream)` — produces an incremental update ready
  for an external signature value to be injected

### CMS / PKCS#7 construction
- Assemble a DER-encoded `CMS SignedData` object for detached SHA-256 signatures
- Support TSA (Time Stamp Authority) embedding via HTTP RFC 3161 request/response

### Certificate verification
- CRL fetching and verification (`Cert/CRLVerifier.cs`)
- OCSP querying (`Cert/OcspHelper.cs`)
- Chain building and validation (`Cert/CertificateVerifier.cs`)

### Test fixture
- A self-signed PKCS#12 keystore (`.p12`) for CI-safe signing tests
- A fixture PDF for signing tests

## Affected example files

- `Signature/CMSProcessableInputStream.cs`
- `Signature/CreateEmbeddedTimeStamp.cs`
- `Signature/CreateEmptySignatureForm.cs`
- `Signature/CreateSignature.cs`
- `Signature/CreateSignatureBase.cs`
- `Signature/CreateSignedTimeStamp.cs`
- `Signature/CreateVisibleSignature.cs`
- `Signature/CreateVisibleSignature2.cs`
- `Signature/ShowSignature.cs`
- `Signature/SigUtils.cs`
- `Signature/TSAClient.cs`
- `Signature/ValidationTimeStamp.cs`
- `Signature/Cert/CRLVerifier.cs`
- `Signature/Cert/CertificateVerificationException.cs`
- `Signature/Cert/CertificateVerificationResult.cs`
- `Signature/Cert/CertificateVerifier.cs`
- `Signature/Cert/OcspHelper.cs`
- `Signature/Cert/RevokedCertificateException.cs`
- `Signature/Validation/AddValidationInformation.cs`
- `Signature/Validation/CertInformationCollector.cs`
- `Signature/Validation/CertInformationHelper.cs`
- `Signature/Validation/CertificateProccessingException.cs`

## Affected test files

- `tests/PdfBox.Net.Examples.Tests/PDModel/TestCreateSignature.cs` (all 8 tests currently skipped)

## Acceptance criteria

- All 22 signature source files compile without stubs and without `NotSupportedException`.
- At minimum the following tests in `TestCreateSignature` pass:
  - `TestDetachedSha256` — basic detached SHA-256 signature
  - `TestCreateEmptySignatureForm` — empty signature form creation
- TSA and OCSP tests may remain skipped if they require external network access.
- Traceability rows for all affected source paths are `in-sync`.
- The `PORT_MODE` of `tests/…/TestCreateSignature.cs` is upgraded from `adapted` to `mechanical`.

## Dependencies

- NuGet package `BouncyCastle.Cryptography` (or `Portable.BouncyCastle`) must be added to
  `PdfBox.Net.Examples.csproj` and `PdfBox.Net.Examples.Tests.csproj`.
