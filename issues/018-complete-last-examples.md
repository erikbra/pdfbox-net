# Issue 018 — Complete the last examples: upgrade all remaining adapted stubs to mechanical

## Summary

This is the completion task that depends on issues #001–#017. Once all prerequisite issues are
resolved, upgrade every remaining `PORT_MODE: adapted` file in `src/PdfBox.Net.Examples/` and
`tests/PdfBox.Net.Examples.Tests/` to `PORT_MODE: mechanical`, enable all currently-skipped
tests, and update the traceability report.

This task directly corresponds to GitHub issue [#350](https://github.com/erikbra/pdfbox-net/issues/350).

## Current state (at time of writing)

All example source files are now `PORT_MODE: mechanical`. The remaining adapted stubs are in the
**test** files, where the underlying library APIs are not yet complete:

| Test file | Reason skipped | Blocking issue |
|---|---|---|
| `TestCreateSignature.cs` (8 tests) | BouncyCastle signing APIs + keystore fixture | #014 |
| `TestRubberStampWithImage.cs` (1 test) | PDImageXObject fixture + appearance-stream draw | #006, #002 |
| `TestFieldRemover.cs` (1 test) | `FieldRemover.Remove()` instance method + fixture PDF | #008 |
| `MergePDFATest.cs` (1 test) | VeraPDF Java-only — may remain skipped permanently | N/A |

## Work items

Execute in dependency order:

1. **Resolve #014** — Implement PDF digital signing crypto APIs (BouncyCastle.NET).
   Enable `TestCreateSignature` tests.

2. **Resolve #006 and #002** — Implement `PDImageXObject.CreateFromFile` and appearance-stream
   drawing. Provide fixture PDF (`document.pdf`) and image (`stamp.jpg`).
   Enable `TestRubberStampWithImage`.

3. **Resolve #008** — Implement `FieldRemover.Remove(string, string, string)` instance method.
   Provide fixture PDF (`PDFBOX-2469-1-AcroForm-AES128.pdf`).
   Enable `TestFieldRemover`.

4. **Assess MergePDFATest** — Determine if a .NET-compatible PDF/A compliance validator exists.
   If none is available, document the permanent skip rationale and close this item.

5. **Update traceability** — After all enabled tests pass, update
   `reports/traceability-parity-report.json` for all upgraded source paths.

6. **Upgrade PORT_MODE headers** — Change `PORT_MODE: adapted` → `PORT_MODE: mechanical` in all
   test files that have been fully enabled.

## Files to upgrade (test layer)

- `tests/PdfBox.Net.Examples.Tests/PDModel/TestCreateSignature.cs`
- `tests/PdfBox.Net.Examples.Tests/PDModel/TestRubberStampWithImage.cs`
- `tests/PdfBox.Net.Examples.Tests/Interactive/Form/TestFieldRemover.cs`
- `tests/PdfBox.Net.Examples.Tests/PDFA/MergePDFATest.cs` (if enableable)

## Additional files to review (from GitHub issue #350)

The following example source files were previously identified in issue #350 as needing review
even after port-mode upgrades; confirm each is fully functional (no silent stubs remain):

- `PDModel/AddAnnotations.cs` — annotation-specific APIs
- `PDModel/AddJavascript.cs` — document-level JavaScript action APIs
- `PDModel/CreatePortableCollection.cs` — PDF portable collection APIs
- `PDModel/EmbeddedFiles.cs` — file attachment annotation APIs
- `PDModel/EmbeddedMultipleFonts.cs` — multiple TrueType font embedding
- `PDModel/EmbeddedVerticalFonts.cs` — vertical writing mode font embedding
- `PDModel/RubberStamp.cs` — rubber stamp annotation appearance stream
- `Interactive/Form/CreateCheckBox.cs` — check-box widget appearance stream
- `Interactive/Form/CreateComboBox.cs` — combo-box field
- `Interactive/Form/CreateListBox.cs` — list-box field
- `Interactive/Form/CreatePushButton.cs` — push-button appearance stream
- `Interactive/Form/CreateRadioButtons.cs` — radio-button group appearance stream
- `Interactive/Form/FieldRemover.cs` — field removal
- `Interactive/Form/FieldTriggers.cs` — JavaScript trigger actions
- `Interactive/Form/UpdateFieldOnDocumentOpen.cs` — document open JavaScript action
- `Rendering/ConvertPDFPagesToImages.cs` — platform image save after `RenderImageWithDPI`
- `Util/ConnectedInputStream.cs` — IO utility class

## Acceptance criteria

- No `PORT_MODE: adapted` remains in any file under `src/PdfBox.Net.Examples/` or
  `tests/PdfBox.Net.Examples.Tests/`.
- No `[Fact(Skip = …)]` annotation remains for a reason that has been resolved by a
  prerequisite issue.
- All previously-skipped tests that can be enabled are passing in CI.
- `reports/traceability-parity-report.json` is updated so all upgraded files show
  `PORT_MODE: mechanical` and status `in-sync`.
- The build is green: `dotnet build PdfBoxNet.slnx` — 0 errors; all tests pass.
