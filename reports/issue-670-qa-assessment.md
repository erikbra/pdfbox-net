# Issue 670 QA Assessment

## Sync scope

This batch advances the PDFBox 3.0 upstream baseline from
`0186baba87403f81039d6d5a2f171e1184f9a2bc` to
`10950c29006e36cfba48e74d4031784e31562cbf` (seven commits).

The Maven version changes are not portable behavior: PdfBox.Net versions are managed by the .NET
build and release pipeline. The remaining Java changes were assessed individually below.

## Production and test parity

| Upstream change | Disposition | .NET evidence |
| --- | --- | --- |
| `Type0Font` parent-font mapping (PDFBOX-6175) | converted | `DebuggerType0Font_ReadMap_UsesParentEncodingCMap` verifies that debugger rows honor a non-identity parent CMap. |
| `PDCIDFont#getParent` deprecation | adapted, no API change | The .NET CID font model intentionally does not retain or expose the Java parent reference. |
| Bouncy Castle 1.85 byte-array constructor workaround (PDFBOX-6218) | adapted, no runtime change | The signature examples use native `SignedCms` and `TSAClient` byte-array APIs. Local detached-CMS verification remains covered by `TestCreateSignature`; TSA/OCSP/CRL cases remain explicitly skipped because they require external endpoints. |
| `TTFParser` Javadoc correction (PDFBOX-5660) | documentation-only | Existing `TTFParserTest` coverage remains applicable. |
| `PDFontTest` and `TestFontEmbedding` input closure | already satisfied | Equivalent .NET fixtures use deterministic `using` ownership; no production behavior changed upstream. |
| Maven development version and Bouncy Castle dependency update | not applicable | NuGet dependencies and package versions are managed independently. |

The Type 0 conversion also exposed an older adaptation drift: GID 0 was treated as available when a
`.notdef` glyph object existed. `PDType0Font` and `PDCIDFontType2` now match upstream's `GID != 0`
availability rule, covered by `PDType0Font_WithCIDFontType2_HasGlyph_UsesCodeToGID`.

No upstream test in this batch is missing without an explicit disposition.

## Per-file sync log

| Target | Previous sync | New sync | `conflict_type` | `result_status` | `local_region_count` | Note |
| --- | --- | --- | --- | --- | ---: | --- |
| `Debugger/Fontencodingpane/Type0Font.cs` | `eeb5d611` | `10950c29` | behavioral adaptation | ported | 1 | Parent Type 0 font owns CMap resolution in the .NET model. |
| `PDModel/Font/PDType0Font.cs` | `cab99713` | `10950c29` | semantic drift | repaired | 1 | GID 0 is no longer reported as an available glyph. |
| `PDModel/Font/PDCIDFontType2.cs` | `853e0761` | `10950c29` | semantic drift | repaired | 1 | Matches upstream `hasGlyph` semantics. |
| `Examples/Signature/CreateEmbeddedTimeStamp.cs` | `eeb5d611` | `10950c29` | dependency API | adapted-no-op | 1 | Native TSA client already consumes byte arrays. |
| `Examples/Signature/ShowSignature.cs` | `eeb5d611` | `10950c29` | dependency API | adapted-no-op | 1 | Native `SignedCms` replaces Bouncy Castle. |
| `Examples/Signature/Validation/AddValidationInformation.cs` | `eeb5d611` | `10950c29` | dependency API | adapted-no-op | 1 | Native `SignedCms` replaces Bouncy Castle. |
| `Examples/Signature/Validation/CertInformationCollector.cs` | `eeb5d611` | `10950c29` | dependency API | adapted-no-op | 1 | Native `SignedCms` replaces Bouncy Castle. |
| `FontBox/TTF/TTFParser.cs` | `498c4a01` | `10950c29` | documentation-only | reviewed | 0 | No runtime delta. |
| `FontBox/TTF/TTFSupportStubs.cs` | `7e9effef` | `10950c29` | companion provenance | reviewed | 0 | Shares the parser source mapping. |
| `PDModel/Font/PDCIDFont.cs` | `6bc8c17f` | `10950c29` | API-model adaptation | adapted-no-op | 1 | Java parent accessor is absent by design. |

## Report corrections

Traceability and conversion records for `PDCIDFont`, `PDCIDFontType2`, and `PDType0Font` previously
pointed to the retired `FontStubs.cs`. They now point to their split production files. Production
traceability rows for all changed upstream Java classes are marked in-sync at the new baseline.

## Local verification

- `dotnet build PdfBoxNet.slnx --configuration Release --no-restore`: passed.
- `dotnet test PdfBoxNet.slnx --configuration Release --no-build --nologo`: 1,443 passed,
  7 explicitly skipped, 0 failed.
- `dotnet pack PdfBoxNet.slnx --configuration Release --no-build
  -p:ContinuousIntegrationBuild=true`: passed for all ten packages.
- API surface ratchet gate against upstream `3.0`: passed with no unreviewed API changes.
- Runtime parity harness syntax and command-line smoke checks: passed; the full Java/.NET corpus remains
  a pull-request CI job.
- Canonical upstream inventory: 1,071 mapped, 0 missing at `10950c29006e36cfba48e74d4031784e31562cbf`.
