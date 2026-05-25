| Feature package | Scenario | Baseline status | Notes |
| --- | --- | --- | --- |
| metadata completeness | string metadata fields | ✅ supported | `TestPDDocumentInformation.StringFieldsRoundtrip` validates title/author/subject/keywords/creator/producer. |
| metadata completeness | date metadata fields | ✅ supported | `TestPDDocumentInformation.CreationDateRoundtrip` and `ModificationDateRoundtrip` cover creation and modification dates. |
| metadata completeness | trapped/custom metadata | ✅ supported | `TrappedValidValues`, `TrappedInvalidValueThrows`, and `CustomMetadataRoundtrip` validate the remaining v1 metadata baseline. |
| outlines/forms | document outline tree operations | ✅ supported | `PDModelInteractiveTest` covers add/insert/sibling traversal and catalog roundtrip behaviors. |
| outlines/forms | acroform field roundtrip | ✅ supported | `PDModelInteractiveTest.PDAcroFormFieldsRoundtrip` verifies text/check fields via catalog `GetAcroForm()`. |
| text baseline | simple text extraction | ✅ supported | Deterministic fixture validates plain extraction across lines. |
| text baseline | line breaks | ✅ supported | Separate text lines are emitted with line separators. |
| text baseline | spacing-sensitive extraction | ✅ supported | Large positive text advance inserts a word separator. |
| text baseline | marked-content capture | ✅ supported | Tagged marked-content sequences preserve tag + extracted text. |
| text baseline | multi-page extraction | ✅ supported | Extraction appends content from sequential pages. |
| text baseline | paragraph boundaries | ⚠️ known gap | Baseline currently emits line breaks only (no paragraph delimiter semantics). |
