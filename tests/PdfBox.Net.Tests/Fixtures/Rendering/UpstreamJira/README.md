# Upstream JIRA Rendering Fixtures

These fixtures track open Apache PDFBox rendering, form appearance, and complex
mask issues without changing PdfBox.Net behavior ahead of upstream.

| JIRA | Local fixture | Purpose |
| --- | --- | --- |
| PDFBOX-2359 | `PDFBOX-2359-3.pdf` | Lines appearing above an image during PDF-to-image conversion. |
| PDFBOX-5953 | `PDFBOX-5953-mail-test2-repaired.pdf` | AcroForm/table fields that disappear on later rendered pages. |
| PDFBOX-6024 | `PDFBOX-6024-gs-bugzilla689309-reduced-bc1_RGB.pdf` | Reduced complex mask rendering fixture. |
| PDFBOX-6024 | `PDFBOX-6024-gs-bugzilla689931-reduced-Multiply.pdf` | Reduced blend/mask fixture using Multiply. |
| PDFBOX-6024 | `PDFBOX-6024-gs-bugzilla689931-reduced-Screen.pdf` | Reduced blend/mask fixture using Screen. |

The optional runtime parity manifest is
`tools/parity/runtime/upstream-rendering-jira-manifest.txt`. It is intentionally
kept separate from the default runtime corpus so these still-open upstream
tickets do not slow down every CI parity run.

`PDFBOX-6024-gs-bugzilla689931-reduced-Multiply.pdf` and
`PDFBOX-6024-gs-bugzilla689931-reduced-Screen.pdf` exercise transparency-group
soft masks combined with non-normal blend modes. The checked-in test requires
their masked strokes to remain visible.

The following fixtures are covered by the upstream `TestQuality` assertions at
100 DPI, synced through `046747da99a870902217efabf1c41297de157059`. Downloads are
verified against the SHA-512 values in that revision's `pdfbox/pom.xml`.

| Fixture | Apache JIRA source | SHA-512 |
| --- | --- | --- |
| `PDFBOX-6077-example.pdf` | [example.pdf](https://issues.apache.org/jira/secure/attachment/13078553/example.pdf) | `f4faaa68062073ffdfd48119563f1e1a6eff6d9a063188dc2ec59ccc747277fcaa6139936cebf2f3f7b1b99b80ca13934fb4568658a169e0bf25bf4e748de101` |
| `PDFBOX-5842-reduced.pdf` | [PDFBOX-5842-reduced.pdf](https://issues.apache.org/jira/secure/attachment/13078557/PDFBOX-5842-reduced.pdf) | `c66b13230d343d6b0a2de81680f5d1ae7f8d92713acbc453ff36fe085831fa89127e0428d297e7494b87fc932ca8a721744db1fe79a41cc5c3cf7d631fc209fc` |
| `PDFBOX-5403-bad-rendering.pdf` | [bad rendering.pdf](https://issues.apache.org/jira/secure/attachment/13041734/bad+rendering.pdf) | `0d7160a4af04bf2f785bf4155f69876da4b7ff7a196e7eda1eb904c02c35aa03c31041d9339fda90322cb58bde16b87bae8460c617bfe5b8dc0670ddab102f62` |
