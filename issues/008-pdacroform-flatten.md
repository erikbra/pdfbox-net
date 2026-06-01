### Title
Implement `PDAcroForm.Flatten()` — flatten form fields to static page content

### Summary

`PDAcroForm.Flatten()` renders all AcroForm field widgets into static page content streams and then removes the AcroForm dictionary from the document catalog. The Java implementation iterates over all fields, calls `PDField.constructAppearances()` as needed, merges each widget's appearance stream into the page content stream, and removes the field annotation from the page.

The .NET `PDAcroForm.Flatten()` method signature exists but is not implemented (it either throws or is a no-op).

### Missing behaviour

- Iterate over all terminal fields in the AcroForm field tree.
- For each widget annotation, merge its appearance stream (`/AP /N`) into the page's content stream at the correct position (as specified by the widget's `/Rect`).
- Remove each widget annotation from `page.GetAnnotations()`.
- After all widgets are processed, clear `acroForm.GetFields()` and remove `/AcroForm` from the document catalog.

### Affected example files (currently stubs)

- `Interactive/Form/FlattenAllFormFields.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java`

### Acceptance criteria

- `PDAcroForm.Flatten()` is implemented end-to-end: widgets are merged into page content and the AcroForm is removed.
- `Interactive/Form/FlattenAllFormFields.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
