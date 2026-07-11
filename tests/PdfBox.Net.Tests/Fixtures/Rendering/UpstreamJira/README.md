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
