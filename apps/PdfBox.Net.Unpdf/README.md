# unpdf

`unpdf` converts a PDF to HTML from the command line.

```console
unpdf input.pdf --output output-directory
```

The initial executable is a lite build. It converts text, links, vector paths,
semantic forms, and browser-safe embedded fonts without loading
`PdfBox.Net.Rendering`, SkiaSharp, or ImageMagick. Image export and raster
fallbacks are planned as explicit optional capabilities.

Run `unpdf --help` for all options and exit-code behavior.

## Exit codes

| Code | Meaning |
|---:|---|
| 0 | Conversion or informational command succeeded. |
| 2 | Command-line usage was invalid. |
| 3 | The input was missing, unreadable, or not a valid PDF. |
| 4 | The output directory could not be used or written. |
| 5 | PDF conversion failed after the input was opened. |

## Self-contained publish

The baseline executable can already be published without a system .NET runtime:

```console
dotnet publish apps/PdfBox.Net.Unpdf/PdfBox.Net.Unpdf.csproj \
  --configuration Release \
  --runtime osx-arm64 \
  --self-contained true
```

Trimming, compressed single-file output, NativeAOT, and the cross-platform RID
matrix are tracked as separate quality-gated milestones.
