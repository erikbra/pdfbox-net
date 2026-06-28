# JPX/JPEG 2000 Runtime Provider Policy

Issue: #542

Generated UTC: 2026-06-28

## Decision

Do not install an optional Java JPEG 2000 provider in the default runtime
parity CI job.

Keep `render-java-optional-jpx-reader-missing-match` as an accepted
optional-runtime category, but make it fixture-scoped to:

- `JPXTestCMYK.pdf`
- `JPXTestGrey.pdf`
- `JPXTestRGB.pdf`

The ratchet ceiling remains `3`.

## Rationale

Apache PDFBox decodes JPX through Java ImageIO and reports that JPEG 2000
support requires the Java Advanced Imaging Image I/O tools. In the current
Apache PDFBox poms, `jai-imageio-core` and `jai-imageio-jpeg2000` are
test-scoped and called out as legally incompatible for distribution. The
PdfBox.Net CI runtime parity job builds `pdfbox-app` and runs the Java probe
against that app jar; the shaded app jar does not include the optional JPX
provider.

Installing that provider in the default parity job would make the comparison
less representative of a normal Java PDFBox app-jar runtime and would add a
license-sensitive dependency to the CI path. It would also turn a Java optional
runtime configuration question into a .NET renderer gate.

## Current Evidence

The #541 render bucket report from the PR #557 runtime artifact records three
JPX rows in `render-java-optional-jpx-reader-missing-match`:

| File | Java behavior | .NET behavior | Policy |
|---|---|---|---|
| `JPXTestCMYK.pdf` | Near-blank render because no Java JPX reader is available. | Visible render. | Accepted optional-runtime difference. |
| `JPXTestGrey.pdf` | Near-blank render because no Java JPX reader is available. | Visible render. | Accepted optional-runtime difference. |
| `JPXTestRGB.pdf` | Near-blank render because no Java JPX reader is available. | Visible render. | Accepted optional-runtime difference. |

## Future Strict Testing

A separate manual or experimental workflow may install a Java JPEG 2000 ImageIO
provider and compare these rows under a strict provider-enabled configuration.
That workflow should not replace the default ratchet until:

1. The provider choice and license constraints are documented.
2. The workflow proves stable on hosted CI.
3. Any Java-provider-enabled differences are reviewed as renderer gaps or
   accepted provider-specific raster drift.

If such a workflow proves useful, it should produce a separate report rather
than raising the default `render-java-optional-jpx-reader-missing-match`
ceiling.
