# Text Semantic Parity Review

Issue: #540

Source: GitHub Actions runtime-parity artifact from PR #556 run 28329962771
Runtime parity output: `artifacts/runtime-parity-issue-540-download/runtime-parity-28329962771-1`
Runtime comparison generated UTC: `2026-06-28T17:35:20Z`
Manifest: `tools/parity/runtime/corpus-manifest.txt`
Fixture ledger: `tools/parity/runtime/text-semantic-fixtures.json`

## Summary

- Text semantic rows: 2
- Reviewed rows: 2
- Unreviewed rows: 0

| Category | Rows |
|---|---:|
| `text-semantic-math-linewrap-match` | 1 |
| `text-semantic-punctuation-spacing-match` | 1 |

## Ratchet Decision

Do not lower the two text semantic ratchet ceilings in this PR. The current rows remain accepted semantic equivalence, not exact text matches. The ratchet already fails new text semantic categories because unknown categories default to zero, and it fails additional rows in these categories because both current ceilings are one.

## Reviewed Fixtures

### `arxiv-sample.pdf`

- Category: `text-semantic-math-linewrap-match`
- Reviewed: yes
- Decision: keep accepted semantic equivalence
- Root cause: PDFTextStripper line grouping differs around math formula fragments with superscript/subscript-style positioning. Java emits a line break between the exponent and the following model label; PdfBox.Net keeps the same text on one line. The stripped text content is identical after the reviewed math-linewrap whitespace normalization.

Java fixture:

```text
lrate = d−0.5
model ·min(step_num−0.5, step_num · warmup_steps−1.5) (3)
```

.NET fixture:

```text
lrate = d−0.5model ·min(step_num−0.5, step_num · warmup_steps−1.5) (3)
```

First diff:

```diff
--- java
+++ dotnet
@@ -328,6 +328,5 @@
 We used the Adam optimizer [20] with β1 = 0.9, β2 = 0.98 and ϵ = 10−9. We varied the learning
 rate over the course of training, according to the formula:
-lrate = d−0.5
-model ·min(step_num−0.5, step_num · warmup_steps−1.5) (3)
+lrate = d−0.5model ·min(step_num−0.5, step_num · warmup_steps−1.5) (3)
 This corresponds to increasing the learning rate linearly for the first warmup_steps training steps,
 and decreasing it thereafter proportionally to the inverse square root of the step number. We used
```

### `cweb.pdf`

- Category: `text-semantic-punctuation-spacing-match`
- Reviewed: yes
- Decision: keep accepted semantic equivalence
- Root cause: PDFTextStripper word-separator heuristics differ at a punctuation operator boundary in typeset C code. Java inserts a separator before the increment operator while PdfBox.Net keeps the operator adjacent. The stripped text content is identical after the reviewed punctuation-spacing normalization.

Java fixture:

```text
change line ++;
```

.NET fixture:

```text
change line++;
```

First diff:

```diff
--- java
+++ dotnet
@@ -802,5 +802,5 @@
 〈Skip over comment lines in the change file; return if end of file 13 〉 ≡
 while (1) {
-change line ++;
+change line++;
 if (¬input ln (change file )) return;
 if (limit < buffer + 2) continue;
```

