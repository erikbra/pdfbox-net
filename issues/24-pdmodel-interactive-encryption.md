### Title
Port `pdmodel/interactive`, `pdmodel/encryption`, and related navigation/security layers

### Depends on
- #23 PDModel graphics/common (PDStream, PDNameTreeNode, PDDestination required)
- Chunk 3 minimal document pipeline (PDDocument, PDPage, PDPageTree)

### Background
The interactive and encryption layers cover:
- **Encryption**: Loading password-protected PDFs is completely blocked. No security handler,
  permission check, or decryption filter is ported.
- **Annotations**: PDF annotations (text notes, links, highlights, stamps, etc.) are absent except
  for the `AnnotationFilter.cs` stub. Used for accessibility, form filling, and redaction.
- **Outlines/bookmarks**: `PDOutlineItem` is a stub. Real navigation trees require full outline
  node hierarchy.
- **Actions**: Navigation links, URI actions, JavaScript triggers, form submission — all absent.
- **Document navigation**: Named destinations, embedded files, threads/beads.

Without these layers:
- Password-protected PDFs cannot be opened
- PDF form fields and interactive widgets cannot be read or written
- Navigation links in rendered output cannot be produced
- Bookmark trees are empty

### Scope

**`pdmodel/encryption`** (~12 classes, enables password-protected PDF reading):
- `PDEncryption.java` — encryption dictionary
- `AccessPermission.java` — permission flags
- `SecurityHandler.java` (abstract) — decrypt/encrypt dispatch
- `StandardSecurityHandler.java` — password-based encryption (RC4 40/128-bit, AES 128/256-bit)
- `StandardDecryptionMaterial.java` — password material holder
- `PublicKeySecurityHandler.java` — certificate-based encryption (can stub initially)
- `PublicKeyDecryptionMaterial.java` — certificate material holder
- `CryptFilterDictionary.java` / `PDCryptFilterDictionary.java`
- `MessageDigests.java` — MD5/SHA digest helpers
- `AESKeyLength.java` — AES key length enum

**`pdmodel/interactive/documentnavigation/outline`** (~3 classes):
- `PDDocumentOutline.java` — root outline node
- `PDOutlineNode.java` — abstract outline node (parent)
- `PDOutlineItem.java` — real implementation (replaces stub)

**`pdmodel/interactive/action`** (~12 classes):
- `PDActionGoTo.java` — go-to-page action
- `PDActionGoToR.java` — remote go-to action
- `PDActionGoToE.java` — embedded go-to
- `PDActionLaunch.java` — launch-application action
- `PDActionURI.java` — URI open action
- `PDActionNamed.java` — named action
- `PDActionResetForm.java` / `PDActionSubmitForm.java`
- `PDActionJavaScript.java`
- `PDActionFactory.java` — factory for constructing action from COSDictionary
- `PDAction.java` — abstract base

**`pdmodel/interactive/annotation`** (~15 classes):
- `PDAnnotation.java` — abstract annotation base
- `PDAnnotationText.java` — sticky note
- `PDAnnotationLink.java` — hyperlink
- `PDAnnotationMarkup.java` — markup annotation base
- `PDAnnotationTextMarkup.java` — highlight/underline/strikeout
- `PDAnnotationWidget.java` — form widget
- `PDAnnotationFreeText.java`, `PDAnnotationLine.java`, `PDAnnotationSquare.java`, etc.
- `PDAnnotationFactory.java` — factory

**`pdmodel/interactive/pagenavigation`** (~3 classes):
- `PDThread.java` — article thread
- `PDThreadBead.java` — real implementation (replaces stub)
- `PDTransition.java` — page transition

**Document-level interactive features** (~5 classes):
- `PDDocumentNameDictionary.java` — Names dictionary
- `PDEmbeddedFilesNameTreeNode.java` — embedded files tree
- `PDViewerPreferences.java` — viewer preferences
- `PDPageLabels.java` integration (if not covered in #23)

### Expected test scope
- Add `tests/PdfBox.Net.Tests/EncryptionTest.cs`:
  - `AccessPermission` flag read/write tests
  - `PDEncryption` dictionary access tests
  - Stub test for StandardSecurityHandler (compile + basic instantiation)
- Add `tests/PdfBox.Net.Tests/PDAnnotationTest.cs`:
  - PDAnnotationLink and PDAnnotationText round-trip tests
  - PDAnnotationFactory dispatch test
- Add `tests/PdfBox.Net.Tests/PDOutlineTest.cs`:
  - PDDocumentOutline tree traversal test

### Entry criteria
- #23 landed (PDStream, PDNameTreeNode available)
- `dotnet build` and `dotnet test` green

### Exit criteria
- `PDEncryption` dictionary is accessible and `StandardSecurityHandler` compiles
- PDF password verification for RC4/AES standard security handler works against a fixture
- `PDDocumentOutline` tree traversal works
- `PDAnnotationLink` and `PDAnnotationText` have real accessors
- `PDActionURI` and `PDActionGoTo` have real accessors
- `PDOutlineItem` stub replaced with real implementation
- `PDThreadBead` stub replaced with real implementation
- `reports/conversion-records.json` and traceability updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- RC4 encryption is not in .NET BCL; use `System.Security.Cryptography` where available and
  add a managed RC4 for legacy 40/128-bit PDF encryption
- AES-256 PDF encryption uses a non-standard key derivation; match Java's PDFBox AES256 exactly
- `PDActionJavaScript` parsing is straightforward but execution is out of scope (stub with warning)
- Form widgets (PDAnnotationWidget) depend on AcroForm model not yet ported; limit to
  constructor + dictionary accessor level initially

### PR slicing rule
- First PR: `pdmodel/encryption` — `PDEncryption` + `AccessPermission` + `StandardSecurityHandler`
  (RC4/AES decryption to unblock password-protected PDF loading)
- Second PR: `pdmodel/interactive/documentnavigation/outline` — real outline tree
- Third PR: `pdmodel/interactive/action` — all action types + factory
- Fourth PR: `pdmodel/interactive/annotation` — annotation hierarchy + factory
- Fifth PR: remaining interactive features (threads, viewer prefs, name dictionary, embedded files)

### Definition of done
- `dotnet build` passes
- StandardSecurityHandler correctly decrypts a password-protected fixture PDF
- Outline tree traversal test passes
- Annotation factory dispatches to correct type
- All stubs replaced
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~40 files, estimated 5–7 engineer-days
