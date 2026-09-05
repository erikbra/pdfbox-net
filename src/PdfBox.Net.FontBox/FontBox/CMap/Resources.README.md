# Predefined CMaps

The files in `Resources/` are copied without modifications from Apache PDFBox
`fontbox/src/main/resources/org/apache/fontbox/cmap` at commit
`046747da99a870902217efabf1c41297de157059`. Per-file SHA-256 values are in
`reports/issue-954-cmap-resources.json`.

These Adobe CMaps retain their embedded copyright, redistribution conditions and
warranty disclaimers. The repository LICENSE includes the CMap license; that
LICENSE is included in the FontBox NuGet package as `licenses/AdobeCMaps/LICENSE`.

They supply predefined encoding and CID-to-Unicode mappings. In particular,
nonembedded CJK fonts need the encoding CMap plus the `Adobe-*-UCS2` map before
Unicode can select a glyph in a substitute with a different character collection.
