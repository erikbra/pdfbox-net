# Issue 875 QA assessment

Upstream range: `fc00e427de8a1046efe6348d64d5529b479aea13..ddef86fcb1a5407035fdd1c8587832c3d1c761b9`

## Production parity

- `Type0Font`: converted. The debugger now performs glyph availability, code-to-CID,
  and code-to-GID lookups through `PDType0Font`, matching the upstream composite-font
  encoding behavior. The path column remains an intentional UI adaptation in the .NET port.
- `CreateEmbeddedTimeStamp`, `ShowSignature`, `AddValidationInformation`, and
  `CertInformationCollector`: converted. Upstream changed BouncyCastle byte-array
  constructors to stream constructors. These ports use `SignedCms` or isolated DER
  helpers whose byte-based decode APIs already consume the complete CMS value, so no
  runtime code change is required.
- `parent/pom.xml`: not applicable. The BouncyCastle Java dependency bump has no direct
  .NET package mapping; .NET dependencies remain centrally managed and use different
  package/version semantics.

## Test parity

- `TestCreateSignature`: converted. Its deterministic .NET CMS validation already decodes
  complete byte arrays with `SignedCms`; external-TSA cases remain explicitly skipped.
- No new upstream test directly exercises the `Type0Font` debugger change. Existing build
  and test suites provide compile and regression coverage.

## Traceability

- All five changed production source mappings and the changed test provenance now point to
  `ddef86fcb1a5407035fdd1c8587832c3d1c761b9`.
- No new or deleted upstream Java files occur in this range, and there are no report-row gaps.
