### Title
Implement PDF digital signing and crypto APIs (BouncyCastle equivalent) for the Signature module

### Summary

The entire `Signature/` examples module (14 files) is stubbed because signing a PDF requires CMS/PKCS#7 signature construction, X.509 certificate chain handling, OCSP validation, CRL checking, and TSA timestamping — all of which depend on BouncyCastle in Java. None of these cryptographic APIs are ported to the .NET library.

### Missing subsystems

| Area | Java dependencies | Blocked files |
|---|---|---|
| CMS/PKCS#7 signing | `org.bouncycastle.cms.*` | `CreateSignature`, `CreateSignatureBase`, `CMSProcessableInputStream` |
| Visual signature | BouncyCastle + XObject appearance | `CreateVisibleSignature`, `CreateVisibleSignature2` |
| Embedded timestamp | RFC 3161 TSA client | `CreateEmbeddedTimeStamp`, `CreateSignedTimeStamp`, `TSAClient` |
| Empty signature form | `PDSignature`, `SignatureOptions` | `CreateEmptySignatureForm` |
| Show/verify signature | `PDSignature`, CMS verifier | `ShowSignature`, `SigUtils` |
| OCSP verification | `org.bouncycastle.ocsp.*`, `OCSPReqBuilder` | `Cert/OcspHelper` |
| CRL verification | `java.security.cert.CRL` + BC | `Cert/CRLVerifier` |
| Certificate chain | `CertPathValidator`, PKIX | `Cert/CertificateVerifier`, `Cert/CertificateVerificationResult` |
| LTV validation info | CAdES, PAdES attributes | `Validation/AddValidationInformation`, `Validation/CertInformationCollector`, `Validation/CertInformationHelper`, `ValidationTimeStamp` |

### Recommended .NET equivalents

- **CMS/PKCS#7**: [`System.Security.Cryptography.Pkcs`](https://learn.microsoft.com/dotnet/api/system.security.cryptography.pkcs) (in `System.Security.Cryptography.Pkcs` NuGet package)
- **X.509 / OCSP / CRL**: [`System.Security.Cryptography.X509Certificates`](https://learn.microsoft.com/dotnet/api/system.security.cryptography.x509certificates)
- **TSA / RFC 3161**: BouncyCastle.NET (`Org.BouncyCastle.*`) or `System.Security.Cryptography.Pkcs` extensions

### Upstream Java reference

`examples/src/main/java/org/apache/pdfbox/examples/signature/`

### Acceptance criteria

- `PDSignature` and `SignatureOptions` model classes are ported and integrated with `PDDocument.AddSignature`.
- At minimum `CreateSignature` (basic detached CMS signing) and `ShowSignature` (verify and display) are upgraded to `PORT_MODE: mechanical`.
- OCSP, CRL, and TSA helpers are ported where platform APIs allow.
- All remaining `Signature/` example files are upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical` where the crypto dependencies are satisfied.
