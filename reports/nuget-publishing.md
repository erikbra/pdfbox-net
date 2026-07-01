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
- `nuget_user`: optional nuget.org username/profile-name override; when omitted,
  the workflow uses the `NUGET_USER` repository or environment variable.
- `skip_duplicate`: passes `--skip-duplicate` during publishing.

The workflow restores, builds, tests, and packs `PdfBoxNet.slnx`, then uploads
all produced `.nupkg` files. When `publish` is true, it pushes those packages
to NuGet.org using NuGet trusted publishing. The publish job requests a GitHub
OIDC token, exchanges it through `NuGet/login@v1` for a short-lived NuGet API
key, and refuses to run from branches other than `main` and `release/3.0`.

Configure a trusted publishing policy on nuget.org with:

- Repository owner: `erikbra`
- Repository: `pdfbox-net`
- Workflow file: `publish-nuget.yml`
- Environment: `nuget.org` if you want the policy restricted to the protected
  GitHub Actions environment used by this workflow.

Set the GitHub repository or `nuget.org` environment variable `NUGET_USER` to
the nuget.org username/profile name for the publishing account. Do not use an
email address.

## Package Metadata

Every packable project should define package-specific NuGet front matter in its
`.csproj`:

- `Title`
- `Description`
- `PackageTags`
- `PackageReadmeFile`

Each package should include a sibling `README.md` packed to the package root.
Shared metadata such as license, project URL, repository URL, release notes,
and version line belongs in `Directory.Build.props`.
