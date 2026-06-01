### Title
Upgrade all remaining `PORT_MODE: adapted` example stubs to `PORT_MODE: mechanical` once blocking issues are resolved

### Summary

65 files in `src/PdfBox.Net.Examples/` and several in `tests/PdfBox.Net.Examples.Tests/` are currently marked `PORT_MODE: adapted` with `throw new NotSupportedException(...)` stubs because the .NET library is missing the APIs they depend on. Once those missing APIs are implemented (tracked in issues #001–#017), each blocked example must be upgraded to a full `PORT_MODE: mechanical` port.

This issue tracks the work of completing those upgrades after the blocking issues are closed.

### Files to upgrade (grouped by blocking issue)

#### Issue #001 — `PDPageContentStream` color operators
- `PDModel/ShowColorBoxes.cs`
- `PDModel/AddMessageToEachPage.cs`
- `Util/AddWatermarkText.cs`
- `Util/PDFHighlighter.cs`
- `Util/PrintTextColors.cs`

#### Issue #002 — `PDPageContentStream.DrawImage`
- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

#### Issue #003 — `PDPageContentStream.ShadingFill`
- `PDModel/CreateGradientShadingPDF.cs`

#### Issue #004 — Tiling pattern fill operators
- `PDModel/CreatePatternsPDF.cs`

#### Issue #005 — `SetTextMatrix`, `SetCharacterSpacing`, `SetWordSpacing`
- `PDModel/ShowTextWithPositioning.cs`
- `PDModel/UsingTextMatrix.cs`
- `PDModel/BengaliPdfGenerationHelloWorld.cs`

#### Issue #006 — `PDImageXObject.CreateFromFile`
- `PDModel/AddImageToPDF.cs`
- `PDModel/ImageToPDF.cs`
- `PDModel/RubberStampWithImage.cs`

#### Issue #007 — `PDType1Font(PDDocument, Stream)` constructor
- `PDModel/HelloWorldType1.cs`

#### Issue #008 — `PDAcroForm.Flatten()`
- `Interactive/Form/FlattenAllFormFields.cs`

#### Issue #009 — `PDSeparation` / `PDDeviceN` color spaces
- `PDModel/CreateSeparationColorBox.cs`

#### Issue #010 — `PDFGraphicsStreamEngine` abstract callback API
- `Rendering/CustomGraphicsStreamEngine.cs`
- `Rendering/CustomPageDrawer.cs`

#### Issue #011 — `PDFTextStripper` subclassing hooks
- `Util/DrawPrintTextLocations.cs`

#### Issue #012 — Content stream token-level editing API
- `Util/RemoveAllText.cs`

#### Issue #013 — `PDTrueTypeFont.ExportFont()`
- `PDModel/ExtractTTFFonts.cs`

#### Issue #014 — PDF digital signing & crypto APIs
- `Signature/CMSProcessableInputStream.cs`
- `Signature/CreateEmbeddedTimeStamp.cs`
- `Signature/CreateEmptySignatureForm.cs`
- `Signature/CreateSignature.cs`
- `Signature/CreateSignatureBase.cs`
- `Signature/CreateSignedTimeStamp.cs`
- `Signature/CreateVisibleSignature.cs`
- `Signature/CreateVisibleSignature2.cs`
- `Signature/ShowSignature.cs`
- `Signature/SigUtils.cs`
- `Signature/TSAClient.cs`
- `Signature/ValidationTimeStamp.cs`
- `Signature/Cert/CRLVerifier.cs`
- `Signature/Cert/CertificateVerificationException.cs`
- `Signature/Cert/CertificateVerificationResult.cs`
- `Signature/Cert/CertificateVerifier.cs`
- `Signature/Cert/OcspHelper.cs`
- `Signature/Cert/RevokedCertificateException.cs`
- `Signature/Validation/AddValidationInformation.cs`
- `Signature/Validation/CertInformationCollector.cs`
- `Signature/Validation/CertInformationHelper.cs`
- `Signature/Validation/CertificateProccessingException.cs`

#### Issue #015 — Platform printing integration
- `Printing/Printing.cs`
- `Printing/OpaquePDFRenderer.cs`

#### Issue #016 — Lucene.Net integration
- `Lucene/IndexPDFFiles.cs`
- `Lucene/LucenePDFDocument.cs`

#### Issue #017 — Ant/MSBuild task integration
- `Ant/PDFToTextTask.cs`

#### Additional stubs not directly covered by #001–#017
The following files were stubbed for additional API gaps that were not separately tracked but should also be upgraded:
- `PDModel/AddAnnotations.cs` — annotation-specific APIs (e.g. `PDAnnotationHighlight`, `PDAnnotationSquiggly`)
- `PDModel/AddJavascript.cs` — document-level JavaScript action APIs
- `PDModel/CreatePortableCollection.cs` — PDF portable collection (PDF portfolio) dictionary APIs
- `PDModel/EmbeddedFiles.cs` — file attachment annotation and embedded file stream APIs
- `PDModel/EmbeddedMultipleFonts.cs` — loading and embedding multiple TrueType fonts
- `PDModel/EmbeddedVerticalFonts.cs` — vertical writing mode TrueType font embedding
- `PDModel/RubberStamp.cs` — rubber stamp annotation appearance stream
- `Interactive/Form/CreateCheckBox.cs` — check-box widget appearance stream
- `Interactive/Form/CreateComboBox.cs` — combo-box field (verify no remaining stubs after widget-fix)
- `Interactive/Form/CreateListBox.cs` — list-box field (verify no remaining stubs after widget-fix)
- `Interactive/Form/CreatePushButton.cs` — push-button appearance stream
- `Interactive/Form/CreateRadioButtons.cs` — radio-button group appearance stream
- `Interactive/Form/FieldRemover.cs` — field removal (verify no remaining stubs)
- `Interactive/Form/FieldTriggers.cs` — JavaScript trigger actions
- `Interactive/Form/UpdateFieldOnDocumentOpen.cs` — document open JavaScript action
- `Rendering/ConvertPDFPagesToImages.cs` — platform image save after `RenderImageWithDPI`
- `Util/ConnectedInputStream.cs` — IO utility class (review whether any stub remains)

### Acceptance criteria

- Every file listed above is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
- No file contains `throw new NotSupportedException(...)` for a reason that has been resolved by the blocking issues.
- All previously-skipped tests in `tests/PdfBox.Net.Examples.Tests/` that were skipped due to these gaps are enabled and pass.
- The traceability report (`reports/traceability-parity-report.json`) is updated so all upgraded files show `PORT_MODE: mechanical`.
