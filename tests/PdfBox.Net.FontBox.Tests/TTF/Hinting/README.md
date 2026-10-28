# Upstream TrueType hinting checks

These xUnit tests port the Apache FontBox hinting checks through
`5206dee5bfe0f435609f6506e3fccccbc46a95cc`. The six bundled fonts and the exact
FreeType 2.13.2 point dumps are under `Fixtures/TTF`; their upstream license text
is retained there. The reference-generating Python/C tools remain unchanged in
`Fixtures/TTF/hinting`. FreeType is not a build or runtime dependency.

Run the suite with:

```sh
dotnet test tests/PdfBox.Net.FontBox.Tests/PdfBox.Net.FontBox.Tests.csproj
```

The upstream Maven build downloads five additional smoke fixtures. To include
those eleven-font checks, set `PDFBOX_FONTBOX_TEST_FONTS` to the upstream
`fontbox/target/fonts` directory before running the suite. It must contain
`DejaVuSansMono.ttf`, `Keyboard.ttf`, `NotoEmoji-Regular.ttf`,
`NotoMono-Regular.ttf`, and `ipag00303/ipag.ttf`. Without the environment variable,
those five cases explicitly skip; when it is configured, a missing file fails.
The Arial test conditionally skips with an explicit reason on machines without
Arial, and checks the exact upstream point arrays when Arial 2.82 is installed.

The Java trace/dump command-line `main` classes are development tools rather
than automated tests. The .NET interpreter retains the internal `ExecutionTracer`
with `TextWriter` output; golden-point and concurrency tests exercise its runtime
hinting machinery without adding public CLI entry points to the library.
