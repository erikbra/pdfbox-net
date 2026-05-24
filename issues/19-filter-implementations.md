### Title
Port all concrete filter implementations in `filter/**`

### Depends on
- Chunk 2 parser/writer bridge (stable baseline)
- COS stream infrastructure (COSInputStream, COSOutputStream — already ported)

### Background
The filter layer is a critical blocker for all real-world PDF processing. The current code has
only an abstract `Filter` base class (`src/PdfBox.Net/Filter/Filter.cs`) plus `DecodeOptions`
and `DecodeResult`. No concrete filter is implemented.

Every compressed PDF stream — including content streams, image data, cross-reference streams, and
embedded font programs — uses one or more filters. Without `FlateFilter` (deflate/zlib), virtually
no real PDF can be decoded. Without `DCTFilter`, JPEG images cannot be extracted. Without
`ASCIIHexFilter` and `ASCII85Filter`, classic PostScript-embedded data streams are unreadable.

### Scope
Port the following filter classes from `pdfbox/src/main/java/org/apache/pdfbox/filter/`:
- `FlateFilter.java` — deflate/zlib compression (used in ~95% of modern PDFs) ← highest priority
- `ASCIIHexFilter.java` — hexadecimal ASCII encoding
- `ASCII85Filter.java` — base-85 ASCII encoding
- `DCTFilter.java` — JPEG image compression/decompression
- `LZWFilter.java` — LZW encoding (used in older PDFs and TIFF images)
- `RunLengthDecodeFilter.java` — simple run-length encoding
- `CCITTFaxDecodeFilter.java` — Group 3/4 fax compression (used for monochrome images)
- `JBIG2Filter.java` — JBIG2 bi-level image compression
- `JPXFilter.java` — JPEG 2000 image compression
- `CryptFilter.java` — encryption-layer filter (integrate with encryption module when ready)
- `FilterFactory.java` — filter registry, factory, and COSName→Filter lookup
- `Predictor.java` — PNG predictor post-processing used after Flate/LZW decoding
- `IdentityFilter.java` — passthrough filter (no transformation)
- `FilterMaker.java` or equivalent initialization support

For .NET-specific adaptation notes:
- Use `System.IO.Compression.DeflateStream` / `ZLibStream` for Flate
- Use `System.Drawing.Imaging` or SkiaSharp for DCT/JPEG if needed; alternatively use
  `System.IO.Compression` + managed JPEG library
- Maintain `Filter.Decode(Stream, Stream, COSDictionary, int, DecodeOptions)` / `Encode` contract

### Expected test scope
- Add filter-focused tests in `tests/PdfBox.Net.Tests/FilterTest.cs` (new file)
- Test each filter with small deterministic encode/decode round-trips
- Add Flate regression test using a real compressed content-stream byte fixture
- Test `FilterFactory` filter lookup by COSName (FlateDecode, ASCIIHexDecode, etc.)

### Entry criteria
- `dotnet build` and `dotnet test` green with current baseline

### Exit criteria
- All listed filter classes are ported and compile
- `FilterFactory` resolves COSName→Filter for all standard PDF filter names
- Flate encode/decode roundtrip test passes
- ASCII85 and ASCIIHex roundtrip tests pass
- `reports/conversion-records.json` and `reports/traceability-parity-report.json` updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- `DeflateStream` in .NET has different header-byte behavior than Java's `Inflater/Deflater` —
  PDF Flate streams omit the 2-byte zlib header in some cases; handle the `nowrap` equivalent
- `CCITTFaxDecodeFilter` and `JBIG2Filter` may require external managed libraries (e.g., iText7
  or custom implementations); consider stub-with-exception path if no suitable library is found
- `CryptFilter` depends on the encryption module which is not yet ported; implement a pass-through
  that throws `NotSupportedException` and log a TODO until #24 lands

### PR slicing rule
- First PR: `FlateFilter` + `FilterFactory` + `Predictor` (covers ~95% of real-world PDFs)
- Second PR: `ASCIIHexFilter` + `ASCII85Filter` + `IdentityFilter` + `RunLengthDecodeFilter`
- Third PR: `DCTFilter` + `LZWFilter` + `CCITTFaxDecodeFilter`
- Fourth PR: `JBIG2Filter` + `JPXFilter` + `CryptFilter`

### Definition of done
- `dotnet build` passes
- Encode/decode roundtrip tests pass for each ported filter
- `FilterFactory` correctly maps all standard PDF filter COSNames
- Provenance headers present on all ported files
- Conversion and traceability records updated
- Size: ~15 files, estimated 2–3 engineer-days
