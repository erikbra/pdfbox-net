# Bundled last-resort font

`LiberationSans-Regular.ttf` is copied unchanged from Apache PDFBox's
`pdfbox/src/main/resources/org/apache/pdfbox/resources/ttf/LiberationSans-Regular.ttf`
at commit `046747da99a870902217efabf1c41297de157059`.

SHA-256: `76d04c18ea243f426b7de1f3ad208e927008f961dc5945e5aad352d0dfde8ee8`.

`FontMapperImpl` loads it only when no installed font matches a requested CID font.
The accompanying `LICENSE.txt` contains the Liberation Fonts copyright and
SIL Open Font License 1.1 text from that upstream revision's `pdfbox/LICENSE`.
The font is embedded in the core assembly; the license is also included in the
NuGet package under `licenses/LiberationSans/LICENSE.txt`.
