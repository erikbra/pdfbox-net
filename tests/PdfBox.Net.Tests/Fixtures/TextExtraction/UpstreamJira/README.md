# Upstream JIRA Text Extraction Fixtures

These fixtures track open Apache PDFBox text-extraction issues without changing
PdfBox.Net behavior ahead of upstream.

| JIRA | Local fixture | Reference attachment | Purpose |
| --- | --- | --- | --- |
| PDFBOX-2138 | `PDFBOX-2138.pdf` | `PDFBOX-2138-java-output.txt` | Corrupted/duplicated words from `PDFTextStripper` output. |
| PDFBOX-2532 | `PDFBOX-2532-PDFBOX2247-701542.pdf` | `PDFBOX-2532-acrobat-reference.txt` | Missing mapping fallback for a Type 1C font without explicit encoding/ToUnicode data. |
| PDFBOX-6188 | `PDFBOX-6188-A151_src.pdf` | `PDFBOX-6188-A151_src-content-stream.txt` | Out-of-order character drawing where default stream-order extraction misses visual search hits. |

The optional runtime parity manifest is
`tools/parity/runtime/upstream-text-jira-manifest.txt`. It is intentionally kept
separate from the default runtime corpus so these still-open upstream tickets do
not slow down or destabilize every CI parity run.
