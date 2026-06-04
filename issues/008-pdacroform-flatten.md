# Issue 008 — `PDAcroForm.Flatten()`

## Summary

Implement `PDAcroForm.Flatten()` so that examples which flatten interactive form fields to
static content compile and produce correct output.

## Required API surface

- `PDAcroForm.Flatten()` — flattens all fields: copies each field's current appearance stream
  onto the page as a regular content stream annotation and removes the interactive form widget
- `PDAcroForm.Flatten(IList<PDField> fields)` — partial flatten of a specified field list
  (optional, but present in upstream Java API)

## Affected example files

- `Interactive/Form/FlattenAllFormFields.cs`

## Acceptance criteria

- `Flatten()` produces a PDF where all field widgets are removed and their visual appearance is
  preserved as page content.
- `FlattenAllFormFields` compiles without stubs and produces a valid flattened PDF.
- Integration test verifies that the output PDF has no AcroForm fields after flattening.
- Traceability row for the affected source path is `in-sync`.

## Notes

- Flattening requires appearance stream rendering support (painting the AP stream into a form
  XObject on the page), so this may depend on Issue #002 or a rendering layer.
- The `FieldRemover.Remove()` instance method (see separate issue) is a related but independent
  concern.
