# NuGet Publishing

PdfBox.Net publishes one package per packable project. This mirrors Apache
PDFBox's Maven split where `pdfbox-io`, `fontbox`, `xmpbox`, and `pdfbox` are
separate artifacts, while .NET-specific backends such as SkiaSharp and
cryptography remain optional packages.

## Version Lines

- `release/3.0` publishes the `3.0.x` package line.
- `main` publishes the `4.x` package line and should use prerelease versions
  such as `4.0.0-preview.1` until Apache PDFBox 4.0 is released.
- Snapshot-aligned package builds should use prerelease versions, for example
  `3.0.8-preview.1`, rather than stable upstream-looking versions.

Keep assembly versions stable per major line and use the NuGet package version
for patch and prerelease identity.

## Workflow

`.github/workflows/publish-nuget.yml` is manually triggered with
`workflow_dispatch`.

Inputs:

- `version`: full SemVer package version to produce.
- `publish`: defaults to `false`; when false the workflow performs a dry-run
  package build and uploads the `.nupkg` artifacts.
- `nuget_source`: defaults to NuGet.org.
- `skip_duplicate`: passes `--skip-duplicate` during publishing.

The workflow restores, builds, tests, and packs `PdfBoxNet.slnx`, then uploads
all produced `.nupkg` files. When `publish` is true, it pushes those packages
using the `NUGET_API_KEY` repository or `nuget.org` environment secret. The
publish job refuses to run from branches other than `main` and `release/3.0`.
