# PdfBox.Net.XmpBox

XMP metadata parsing and serialization ported from Apache XmpBox.

Install:

```sh
dotnet add package PdfBox.Net.XmpBox
```

The package can parse, inspect, modify, and serialize XMP packets independently
of the core PDF model.

```csharp
using PdfBox.Net.XmpBox.Xml;

XMPMetadata metadata = new DomXmpParser().Parse(File.ReadAllBytes("metadata.xmp"));
```

`PdfBox.Net.XmpBox` is published as a sibling package, matching the Java
`xmpbox` artifact.
