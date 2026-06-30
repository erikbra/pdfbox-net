# PDFBox 3.0 CI And Package Metadata

Issue #591 aligns the `release/3.0` branch with Apache PDFBox `origin/3.0`
without changing the trunk/main package story.

## CI Branch Settings

The `CI Build` workflow selects 3.0 inputs when the pull request targets
`release/3.0`, the pushed branch is `release/3.0`, or the branch name starts
with `codex/3-0-`.

| Setting | 3.0 branch value | Main/trunk value |
|---|---|---|
| `PDFBOX_PARITY_PDFBOX_REF` | `3.0` | pinned trunk commit |
| `PDFBOX_PARITY_RATCHET_BASELINE` | `tools/parity/runtime/ratchet-baseline-3.0.json` | `tools/parity/runtime/ratchet-baseline.json` |
| `PDFBOX_API_SURFACE_PDFBOX_REF` | `3.0` | pinned trunk commit |
| `PDFBOX_API_SURFACE_ENFORCE` | `true` | `true` |

Runtime parity runs for all pull requests plus pushes to `main` and
`release/3.0`.

## Package Versioning

`Directory.Build.props` on this branch defines the default 3.0 package line:

| Property | Default |
|---|---|
| `PdfBoxNetPackageLine` | `3.0` |
| `PdfBoxNetUpstreamRef` | `origin/3.0` |
| `PdfBoxNetUpstreamVersion` | `3.0.8-SNAPSHOT` |
| `VersionPrefix` | `3.0.8` |
| `VersionSuffix` | `preview` |
| `AssemblyVersion` | `3.0.0.0` |
| `FileVersion` | `3.0.8.0` |

The default NuGet version is therefore `3.0.8-preview`, matching the living
Apache `origin/3.0` branch while it is at `3.0.8-SNAPSHOT`. Release automation
can override the full package version explicitly for a stable release, for
example:

```bash
dotnet pack PdfBoxNet.slnx \
  --configuration Release \
  -p:Version=3.0.8
```

## Packable Projects

The package smoke is:

```bash
dotnet pack PdfBoxNet.slnx --configuration Release --no-build --output artifacts/packages
```

The intended package set is:

| Package ID | Role |
|---|---|
| `PdfBox.Net` | Core PDFBox library |
| `PdfBox.Net.IO` | Random-access IO primitives |
| `PdfBox.Net.FontBox` | FontBox port |
| `PdfBox.Net.XmpBox` | XmpBox port |
| `PdfBox.Net.Cryptography` | BouncyCastle-backed cryptography provider |
| `PdfBox.Net.SkiaSharp` | SkiaSharp rendering backend |
| `PdfBox.Net.SystemDrawing` | Windows System.Drawing helpers and print backend |
| `PdfBox.Net.ImageSharp` | ImageSharp rendering/backend experiment |
| `PdfBox.Net.MauiGraphics` | Microsoft.Maui.Graphics rendering/backend experiment |
| `PdfBox.Net.Tools` | `pdfbox` .NET global tool facade for Apache PDFBox 3.0 CLI commands |

The following projects are intentionally non-packable: examples, benchmarks,
debugger app/library, test projects, and the runtime parity probe.

## Validation

CI validates branch/package alignment with:

- `dotnet restore PdfBoxNet.slnx`
- `dotnet build PdfBoxNet.slnx --configuration Release --no-restore`
- `dotnet test PdfBoxNet.slnx --configuration Release --no-build --nologo`
- `dotnet pack PdfBoxNet.slnx --configuration Release --no-build --output artifacts/packages -p:ContinuousIntegrationBuild=true`
- API surface gate against Apache PDFBox `3.0`
- Runtime parity against Apache PDFBox `3.0` and `ratchet-baseline-3.0.json`
