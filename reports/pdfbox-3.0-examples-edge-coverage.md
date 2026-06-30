# PDFBox 3.0 Examples Edge Coverage

Issue #603 reviewed the remaining skipped or partial Apache PDFBox 3.0 example
tests after the non-Preflight module review.

## Result

Result: **closed for deterministic local coverage, with external validation
boundaries accepted and documented**.

The .NET examples suite now covers the local PDF/A and signature behavior that
can be verified without external services:

- `CreatePDFA` creates a loadable one-page document, writes PDF/A XMP
  identification metadata, embeds the TrueType font through the Type 0/CIDFont
  path, and writes an output intent when an ICC profile path is supplied.
- `PDFMergerExample` merges PDF/A-shaped inputs into a loadable two-page output.
- Detached CMS signing is verified with an in-memory PKCS#12 certificate.
- `CreateVisibleSignature` and `CreateVisibleSignature2` are covered for both
  regular and external-signing paths with deterministic generated input PDFs and
  stamp images.
- Empty signature form creation remains covered.

## Product Adaptations

The remaining skipped example tests are accepted external-service or
external-validator adaptations, not unreviewed source/API/runtime gaps:

| Test | Reason |
|---|---|
| `TestDetachedSha256WithTSA` | Requires either a live TSA endpoint or a checked-in RFC 3161 response fixture with nonce/date behavior aligned to the request. |
| `TestCreateEmbeddedTimeStamp` | Requires a TSA endpoint to create the unsigned timestamp attribute inserted into an existing CMS signature. |
| `TestCreateSignedTimeStamp` | Requires a TSA endpoint to create a document timestamp signature. |
| `TestAddValidationInformation` | Requires OCSP/CRL responders or a larger local revocation fixture with matching certificate chain and DSS expectations. |
| PDF/A Preflight/VeraPDF validation | Preflight and `preflight-app` are intentionally excluded from the initial `release/3.0` branch. VeraPDF remains an external validator choice rather than an in-process .NET dependency. |

## Implementation Notes

`PDType0Font.Load()` now persists the loaded TrueType bytes as
`/FontDescriptor /FontFile2` on the CIDFontType2 descendant. This keeps the
mechanical Type 0 font path Java-shaped while making saved and reloaded
documents report the font as embedded. `CreatePDFA` now enforces that embedded
font precondition before writing text, matching the intent of the Apache example.

The output-intent test uses deterministic local bytes to cover the example's
optional ICC-profile plumbing. It does not claim those bytes are a real sRGB ICC
profile and does not replace external PDF/A conformance validation.
