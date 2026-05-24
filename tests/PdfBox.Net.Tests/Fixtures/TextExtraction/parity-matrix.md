| Scenario | Baseline status | Notes |
| --- | --- | --- |
| simple text extraction | ✅ supported | Deterministic fixture validates plain extraction across lines. |
| line breaks | ✅ supported | Separate text lines are emitted with line separators. |
| spacing-sensitive extraction | ✅ supported | Large positive text advance inserts a word separator. |
| marked-content capture | ✅ supported | Tagged marked-content sequences preserve tag + extracted text. |
| multi-page extraction | ✅ supported | Extraction appends content from sequential pages. |
| paragraph boundaries | ⚠️ known gap | Baseline currently emits line breaks only (no paragraph delimiter semantics). |
