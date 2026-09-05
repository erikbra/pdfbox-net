# Issue 954 Upstream Sync Assessment

## Scope and acceptance

- Apache PDFBox branch: `trunk`.
- Previous tracked commit: `1187c45f9dcee38ed5ac12bc15df04913b348875`.
- Assessed through commit: `046747da99a870902217efabf1c41297de157059`.
- Upstream commits reviewed: 104. Changed upstream paths: 67.
- Explicitly excluded module: `pdfbox-layout-fop`.

This batch implements the applicable upstream production changes and records platform adaptations and test gaps explicitly. Synchronizing the changed production slice does not establish whole-library parity. In particular, this batch does not implement the unchanged full appearance handlers or the pre-existing Java `JPEGFactory.createFromImage` surface. Their upstream tests remain visible with specific deferred coverage below.

The debugger continues the existing headless inspection/model product in `apps/PdfBox.Net.Debugger.App` and `src/PdfBox.Net.Debugger`. Certificate decoding and PANOSE inspection data are implemented; Swing popup/window/dialog and URI-dispatch interactions have no counterpart in that product. The established release/3.0 decision is documented in issue #602; the current main CLI/model code independently establishes the same implementation boundary.

## Result

The applicable production changes are implemented and focused regression tests pass. The sync log has 63 in-sync rows, two needs-manual-sync rows for explicitly deferred tests, one new-upstream LDAP test and one deleted-upstream compatibility type. Independent final gates passed as recorded below. The tracked sync commit and both CI comparison references advance to `046747da99a870902217efabf1c41297de157059`; the upstream watcher continues to follow `trunk`. No runtime or API ratchet allowance is increased.

## Material implementation changes

- Direct streaming PFB parsing and JPEG embedding avoid redundant full-buffer copies; malformed/truncated inputs are checked without allocating based on forged segment lengths. TTF parse lifetimes and CCITT changing-element memory accounting follow upstream.
- CID font mapping now uses real font-header inventories, character-collection/code-page/PANOSE scoring, cached font loading and a licensed deterministic Liberation Sans fallback. CID-keyed substitutes with a different Identity collection resolve outlines and widths through Unicode.
- All 92 predefined Adobe CMaps are bundled unchanged, restoring the missing encoding/CID-to-Unicode prerequisite exposed by the real Noto/CNS regression. SHA-256 inventory is in `issue-954-cmap-resources.json`; source and binary redistribution notices are retained.
- Stencils use the current paint/pattern and soft mask. Tiling-pattern graphics state isolation, device-coordinate handling, reflected patterns and retention of the pattern name across q/Q are covered by synthetic and upstream regression fixtures.
- FDF/XML escaping, certificate URL routing and redirect allowlists, extraction symlink checks, annotation color normalization, text page traversal and literal Markdown fencing are synchronized through the existing .NET entry points.
- Certificate inspection decodes actual X.509 bytes from PDF strings/streams; PANOSE inspection normalizes malformed byte arrays and shows invalid values as text.

## Per-file sync log

The machine-readable log is `reports/issue-954-upstream-sync-log.json`. “in-sync” describes the reviewed upstream delta in the adapted target; the separate test-parity column identifies deferred behavior rather than implying complete Java API/fixture equivalence.

| Source path | Target path | Previous sync | New sync | Conflict type | Result | Local regions | Sync note |
|---|---|---|---|---|---|---:|---|
| `SECURITY.md` | — | `1187c45` | `046747d` | none | in-sync | 0 | Upstream project security policy; PdfBox.Net has no mapped SECURITY.md. Apache reporting contacts/policy are not asserted as a policy for this independent port. |
| `debugger/pom.xml` | — | — | `046747d` | none | in-sync | 0 | Maven dependency/test configuration only; the net10.0 debugger requires no new package for X.509 decoding. |
| `debugger/src/main/java/org/apache/pdfbox/debugger/PDFDebugger.java` | `src/PdfBox.Net.Debugger/PDFDebugger.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Added X.509 details for direct Cert strings and Cert/Certs arrays during COS tree inspection. Swing window centering, file dialogs, renderer-menu/tooltips and certificate tab widgets are accepted headless adaptations: apps/PdfBox.Net.Debugger.App calls InspectDocument; no desktop dispatcher or Swing event loop exists. |
| `debugger/src/main/java/org/apache/pdfbox/debugger/certificatepane/CertificatePane.java` | `src/PdfBox.Net.Debugger/Certificatepane/CertificatePane.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | New certificate data model uses X509CertificateLoader for decoded COSStream/COSString DER bytes and exposes certificate details or decoding diagnostic as inert text. Swing pane replaced by GetText for current headless inspector. |
| `debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/PanoseFlag.java` | `src/PdfBox.Net.Debugger/Flagbitspane/PanoseFlag.cs` | `fee11b4` | `046747d` | semantic-divergence | in-sync | 0 | Added actual upstream byte normalization, missing-string handling, hexadecimal display and classification table data while preserving IFlag compatibility. Invalid family-kind values also yield a diagnostic, extending the upstream guard to all columns. |
| `debugger/src/main/java/org/apache/pdfbox/debugger/pagepane/PagePane.java` | `src/PdfBox.Net.Debugger/Pagepane/PagePane.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | The .NET PagePane is a page-selection data model with no hover windows, mouse handlers, Desktop.browse or link launching. Hover popup placement/lifetime, render-time rectangle-map rebuild, scheme confirmation and OS dispatcher guards are not applicable to the existing headless product. Existing PDAnnotation GetContents/GetTitlePopup expose reusable annotation text. |
| `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/FontToolTip.java` | `src/PdfBox.Net.Debugger/Streampane/Tooltip/FontToolTip.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Escapes document-supplied font-name markup including both quote forms before HTML tooltip output. |
| `debugger/src/test/java/org/apache/pdfbox/debugger/pagepane/PagePaneTest.java` | — | — | `046747d` | none | in-sync | 0 | New isBrowsableScheme test exercises Java Desktop link launching, absent in the .NET headless inspector. No dormant allowlist is added to a model that never opens URIs. |
| `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddAnnotations.java` | `src/PdfBox.Net.Examples/PDModel/AddAnnotations.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Adds blue square interior and blue caret at upstream rectangle. .NET example remains smaller than Java baseline; changed sample behaviors applied. |
| `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/BengaliPdfGenerationHelloWorld.java` | `src/PdfBox.Net.Examples/PDModel/BengaliPdfGenerationHelloWorld.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Missing font bounding box uses 1500-unit line-height fallback; .NET also tolerates absent descriptor. |
| `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ExtractEmbeddedFiles.java` | `src/PdfBox.Net.Examples/PDModel/ExtractEmbeddedFiles.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Restores upstream input-directory-relative extraction, file-spec preferred names/alternate streams and page attachments; canonicalizes every path component and final file symlink before directory containment check. .NET FileSystemInfo adapter retained in PORT-LOCAL region. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/CreateEmbeddedTimeStamp.java` | `src/PdfBox.Net.Examples/Signature/CreateEmbeddedTimeStamp.cs` | `ddef86f` | `046747d` | semantic-divergence | in-sync | 0 | Validates 4-element ByteRange before contents/TSA work; ports structured informational logging. Existing .NET TSA DER-splicing adapter retained. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java` | `src/PdfBox.Net.Examples/Signature/SigUtils.cs` | `1187c45` | `046747d` | semantic-divergence | in-sync | 0 | Adds exact upstream URL allowlist and CheckAccess before certificate reads. .NET auto-redirect disabled; only one 301/302/303 exact HTTP-to-HTTPS upgrade followed. Intentional stricter adaptation versus Java implicit same-scheme redirects is documented; no arbitrary redirect chains. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CRLVerifier.java` | `src/PdfBox.Net.Examples/Signature/Cert/CRLVerifier.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Removes FTP and routes HTTP(S) download through SigUtils guarded helper. LDAP remains explicitly unsupported by existing .NET adapter; underlying CRL cryptographic validation remains baseline platform adaptation. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CertificateVerificationResult.java` | `src/PdfBox.Net.Examples/Signature/Cert/CertificateVerificationResult.cs` | `eeb5d61` | `eeb5d61` | semantic-divergence | deleted-upstream | 0 | The class deleted upstream remains locally because .NET CertificateVerifier returns this public type. Preserved as a compatibility API; deletion does not remove any live behavior or claim new upstream provenance. |
| `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/OcspHelper.java` | `src/PdfBox.Net.Examples/Signature/Cert/OcspHelper.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Checks upstream URL allowlist immediately before optional BouncyCastle OCSP transport call; existing API adapter retained. |
| `examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateSignature.java` | `tests/PdfBox.Net.Examples.Tests/PDModel/TestCreateSignature.cs` | `ddef86f` | `ddef86f` | semantic-divergence | needs-manual-sync | 0 | Changed online LTV invocation and new TSA-certificate test require live TSA/CRL/OCSP; existing .NET deterministic signing test retained without empty success placeholder. |
| `examples/src/test/java/org/apache/pdfbox/examples/signature/cert/CRLVerifierTest.java` | — | — | — | none | new-upstream | 0 | New LDAP CRL download/signature-validation test cannot run against .NET adapter because LDAP retrieval is explicitly unsupported. No success-shaped placeholder added. |
| `fontbox/pom.xml` | — | `1187c45` | `046747d` | none | in-sync | 0 | Maven test-download additions do not change .NET build dependencies. Relevant parser tests are separately accounted for. |
| `fontbox/src/main/java/org/apache/fontbox/cmap/CMapParser.java` | `src/PdfBox.Net.FontBox/FontBox/CMap/CMapParser.cs` | `746cf4e` | `046747d` | semantic-divergence | in-sync | 0 | Ported external predefined CMap name validation before resource lookup, preserving exception and accepted-name semantics. Restored all 92 upstream predefined CMap resources (3.3 MiB), absent in the baseline package, to make actual CJK font substitution reachable. Adobe licenses remain embedded and shipped with FontBox. |
| `fontbox/src/main/java/org/apache/fontbox/pfb/PfbParser.java` | `src/PdfBox.Net.FontBox/FontBox/Pfb/PfbParser.cs` | `7e9effe` | `046747d` | semantic-divergence | in-sync | 0 | Parse input directly, read segment bytes incrementally, stop at PFB EOF marker, retain caller ownership, and validate minimum parsed length. .NET bounded chunk accumulation substitutes InputStream.readNBytes. |
| `fontbox/src/main/java/org/apache/fontbox/ttf/HeaderTable.java` | `src/PdfBox.Net.FontBox/FontBox/TTF/HeaderTable.cs` | `7e9effe` | `046747d` | semantic-divergence | in-sync | 0 | Removed Java explicit super() is already represented by the implicit C# base constructor; no behavior change. |
| `fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java` | `src/PdfBox.Net.FontBox/FontBox/TTF/TTFParser.cs` | `fc00e42` | `046747d` | semantic-divergence | in-sync | 0 | Centralized source lifetime and error cleanup for random access and embedded inputs, matching upstream shared parse helper; memory-backed font data remains owned by returned font. |
| `fontbox/src/test/java/org/apache/fontbox/pfb/PfbParserTest.java` | `tests/PdfBox.Net.FontBox.Tests/PfbAndType1FontTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | Ported negative/huge/short input validation and getters via deterministic synthetic Type1 fixture. External OpenSans/DejaVu/KIX full-glyph assertions are deferred because those fixture downloads are not part of .NET test assets. |
| `fontbox/src/test/java/org/apache/fontbox/ttf/TestTTFParser.java` | `tests/PdfBox.Net.FontBox.Tests/TTFParserTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | Ported header-vs-full-font comparison and source closure checks with repository-owned synthetic font. Exact LiberationSans kerning/IPA vertical-property corpus assertions are deferred; no corresponding production metric change exists in this batch. |
| `fontbox/src/test/java/org/apache/fontbox/ttf/TrueTypeFontCollectionTest.java` | `tests/PdfBox.Net.FontBox.Tests/TTFTrueTypeFontCollectionTest.cs` | `eeb5d61` | `046747d` | semantic-divergence | in-sync | 0 | Added exact missing-header/invalid-font-count exceptions. OS-installed-font assertions are deferred to provider tests using deterministic temporary collections rather than requiring Windows/macOS bundled fonts in CI. |
| `pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutFontLoaderAwt.java` | `src/PdfBox.Net/GlyphLayout/Awt/GlyphLayoutFontLoaderAwt.cs` | `56575fd` | `046747d` | semantic-divergence | in-sync | 0 | Eliminated the extra byte-array copy by exposing the existing PDType0Font byte[] loader internally and loading the already-buffered font directly. The AWT registration proxy and full-font-only embedding remain baseline limitations. Supplemental file: src/PdfBox.Net/PDModel/Font/PDType0Font.cs. |
| `pdfbox-layout-fop/src/main/java/org/apache/pdfbox/glyphlayout/fop/GlyphLayoutFontLoaderFop.java` | — | `1187c45` | `046747d` | none | in-sync | 0 | Explicitly excluded upstream pdfbox-layout-fop module. |
| `pdfbox/pom.xml` | — | `1187c45` | `046747d` | none | in-sync | 0 | Maven test fixtures/Byte Buddy dependency changes have no .NET package mapping; adopted fixtures are accounted for in their test rows. |
| `pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDFStreamEngine.java` | `src/PdfBox.Net/ContentStream/PDFStreamEngine.cs` | `aba4428` | `046747d` | semantic-divergence | in-sync | 0 | Java available() call reduction only. .NET uses MemoryStream.Position/Length and computes code length from before/after position without virtual available() calls. Existing decoding semantics already match. Also restored isolated graphics state and pattern BBox clipping for tiling-pattern processing, required by the synchronized stencil masking path. |
| `pdfbox/src/main/java/org/apache/pdfbox/cos/COSName.java` | `src/PdfBox.Net/COS/COSName.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Added canonical COSName.OPEN constant. Also fixed existing cache test lifetime race by asserting cache membership while the strong reference remains alive; release assertions are unchanged. |
| `pdfbox/src/main/java/org/apache/pdfbox/filter/CCITTFaxFilter.java` | `src/PdfBox.Net/Filter/CCITTFaxDecodeFilter.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Include both changing-element int rows in the CCITT decode memory bound before any decoder allocation/read; preserved exact upstream explanatory diagnostic. |
| `pdfbox/src/main/java/org/apache/pdfbox/filter/FlateFilterDecoderStream.java` | `src/PdfBox.Net/Filter/FlateFilterDecoderStream.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Java removal of synchronized on unsupported mark/reset has no .NET equivalent: existing nonseekable Stream methods already use no locks. |
| `pdfbox/src/main/java/org/apache/pdfbox/multipdf/PDFMergerUtility.java` | `src/PdfBox.Net/MultiPdf/PDFMergerUtility.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Ported documentation requiring save before append when source contains subset fonts. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java` | `src/PdfBox.Net/PdfParser/COSParser.cs` | `a71c567` | `046747d` | semantic-divergence | in-sync | 0 | Upstream null pool-object guard is covered by .NET PDFParser.GetOrCreateIndirectObject, which returns an existing non-null object or creates one for a non-null key. No nullable GetObjectFromPool call exists in the adapted parser. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSWriter.java` | `src/PdfBox.Net/PdfWriter/COSWriter.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream incremental write removes final ByteArrayOutputStream.toByteArray allocation. Actual .NET PDDocument.SaveIncremental already resets MemoryStream.Position and CopyTo(output) without that final duplicate array. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDUserAttributeObject.java` | `src/PdfBox.Net/PDModel/DocumentInterchange/LogicalStructure/PDUserAttributeObject.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | All three added upstream missing-P guards already exist: empty getter, lazily created array on add, no-op removal. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/RC4Cipher.java` | `src/PdfBox.Net/PDModel/Encryption/RC4Cipher.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream removes an always-zero private offset argument. Local byte-array Write already iterates from its first byte and has no offset argument. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/StandardSecurityHandler.java` | `src/PdfBox.Net/PDModel/Encryption/StandardSecurityHandler.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream RC4 input wrapper allocations are absent in .NET RC4Apply(byte[],byte[]) and owner/user password loops, which already pass arrays directly. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDictionary.java` | `src/PdfBox.Net/PDModel/Fdf/FDFDictionary.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Escapes XML10 file href and omits absent file names. Existing .NET model remains adapted. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFField.java` | `src/PdfBox.Net/PDModel/Fdf/FDFField.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Requires field name before output; shared XML10 escaping for field names, scalar/list values and rich text. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFUtils.java` | `src/PdfBox.Net/PDModel/Fdf/FDFUtils.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | New XML10 codepoint utility preserves surrogate pairs, replaces invalid characters and logs replacement count. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/processor/AcroFormOrphanWidgetsProcessor.java` | `src/PdfBox.Net/PDModel/Fixup/Processor/AcroFormOrphanWidgetsProcessor.cs` | `fee11b4` | `046747d` | semantic-divergence | in-sync | 0 | Upstream new null default-appearance return already covered by existing string.IsNullOrWhiteSpace guard; no behavior change required. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FontMapperImpl.java` | `src/PdfBox.Net/PDModel/Font/FontMapperImpl.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Implements functional GetCIDFont chain with actual lazy system FontInfo inventory/parsed-font cache, named/Standard14 substitutions, CJK ROS/Panose/weight scoring and licensed bundled fallback. Ports Identity ROS fallback, misleading NotoSans/Malgun codepage masks, regular-weight preference and DroidSans filter. Adopted nullable default interface preserves custom .NET mappers; full dependency/adaptation notes in issue954-font-dependency-notes.md. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java` | `src/PdfBox.Net/PDModel/Font/PDCIDFontType0.cs` | `bf37c60` | `046747d` | semantic-divergence | in-sync | 0 | Implemented CFF embedded/substitute loading and parent-aware CID glyph/width routing; mismatched Identity ROS resolves glyphs through parent ToUnicode plus substitute cmap, with original same-ROS CFF charset behavior retained. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1FontEmbedder.java` | `src/PdfBox.Net/PDModel/Font/PDType1FontEmbedder.cs` | `7e9effe` | `046747d` | semantic-divergence | in-sync | 0 | The mapped helper remains an internal encoding holder; its stale deferred-embedding comment is corrected. The actual embedding path in PDType1Font.CreateEmbeddedType1FontData now uses one PfbParser(stream) and Type1Font.CreateWithSegments, eliminating a full copy and duplicate parse. Both implementation and helper are recorded. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/CCITTFactory.java` | `src/PdfBox.Net/PDModel/Graphics/Image/CCITTFactory.cs` | `fee11b4` | `046747d` | semantic-divergence | in-sync | 0 | Audited equivalent adapted path: TIFF interpretation, including T4Options bits 1/2, is delegated to the TIFF decoder; output is re-encoded as Group 4. There is no local raw-TIFF extractFromTiff bit test or borrowed output stream to fix. Caller input remains open, local output writer closes. Documented provider boundary; no artificial unsupported-option rejection was added. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactory.java` | `src/PdfBox.Net/PDModel/Graphics/Image/JPEGFactory.cs` | `1187c45` | `046747d` | semantic-divergence | in-sync | 0 | Directly copies the caller stream into its COSStream, reads JPEG metadata through a bounded stream parser, and wraps the populated stream without a full-size byte-array copy. Added CreateFromByteArray delegation. Existing adapted metadata parser and absence of createFromImage/encoding overloads remain baseline API differences. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotation.java` | `src/PdfBox.Net/PDModel/Interactive/Annotation/PDAnnotation.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Equal RGB components resolve to DeviceGray. Added shared protected GetColor(COSName) and routed existing Markup/SquareCircle interior color through it to preserve upstream call semantics. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationText.java` | `src/PdfBox.Net/PDModel/Interactive/Annotation/PDAnnotationText.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Uses COSName.OPEN cached constant for getter and setter; equivalent name. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/AppearanceGeneratorHelper.java` | `src/PdfBox.Net/PDModel/Interactive/Form/AppearanceGeneratorHelper.cs` | `fee11b4` | `046747d` | semantic-divergence | in-sync | 0 | Missing rectangle throws IOException in ComputeBBox; MemoryStream.WriteTo replaces ToArray intermediate allocation in both appearance write paths. |
| `pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java` | `src/PdfBox.Net/Rendering/PageDrawer.cs` | `aba4428` | `046747d` | semantic-divergence | in-sync | 0 | Implemented stencil image painting for inline and XObject images using the current color/pattern and Decode direction. Skia layers preserve transparent paint gaps with DstIn and apply soft masks in page-device coordinates, avoiding the Java scratch-paint rescaling and alpha-dilation workaround. Fixed necessary baseline dependencies: a clean pattern graphics state/BBox clip, device-scale-independent tile height, full texture pattern matrix including signed reflections, and immutable PDColor retention across q/Q. The PageDrawer facade and Skia peer share the updated provenance marker; helper files retain their prior provenance because they were not comprehensively resynced. Existing transparency-group compositor/CMYK adaptations are preserved; broad pixel identity with Java is not claimed. Final runtime validation exposed antialiased fill residues at subpixel transparent-stencil edges. The DstIn mask now uses a Decal shader over the entire layer, clearing both fully transparent stencils and transparent outer borders without leaving bounded-image coverage fringes. |
| `pdfbox/src/main/java/org/apache/pdfbox/rendering/SoftMask.java` | `src/PdfBox.Net/Rendering/SoftMask.cs` | `aba4428` | `046747d` | semantic-divergence | in-sync | 0 | Ported Point2D origin and underlying paint/mask/origin/backdrop/transfer accessors. Existing alpha lookup uses the origin; native Skia applies full page-device masks through layers. |
| `pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripper.java` | `src/PdfBox.Net/Text/PDFTextStripper.cs` | `aba4428` | `046747d` | semantic-divergence | in-sync | 0 | Always visit pages through ProcessPage so page selection can run before content checks, including empty-page processing. |
| `pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripperByArea.java` | `src/PdfBox.Net/Text/PDFTextStripperByArea.cs` | `aba4428` | `046747d` | semantic-divergence | in-sync | 0 | Removed eager HasContents condition and always dispatch ProcessPage, matching upstream page-selection behavior. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/fdf/FDFUtilsTest.java` | `tests/PdfBox.Net.Tests/FDFUtilsTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | All 13 upstream cases translated, additional field/dictionary integration and invalid-codepoint coverage. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/CIDCharSetMatchTest.java` | `tests/PdfBox.Net.Tests/CIDCharSetMatchTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | All new upstream ROS/codepage cases ported, with independent regression checks for codepage masks and font ranking. Test-only metadata provider proves filtering does not need outline loading; production inventory separately tested with binary fonts. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0SubstituteTest.java` | `tests/PdfBox.Net.Tests/PDCIDFontType0SubstituteTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | Converted with deterministic synthetic CID-keyed Identity OTF and explicit mapper, so CID!=GID and nonempty outline/width assertions run on every machine without installed-font assumptions. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactoryTest.java` | `tests/PdfBox.Net.Tests/ImageFactoryTest.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | The upstream edit changes Color object identity to value equality in a comparison helper; equal colors already yielded zero RGB error before the change. Existing .NET image assertions compare byte/channel values; no corresponding identity-equality defect exists. Native JPEGFactoryStreamingTest covers the changed factory input route. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/interactive/annotation/AppearanceGenerationTest.java` | `tests/PdfBox.Net.Tests/AppearanceGenerationTest.cs` | — | — | semantic-divergence | needs-manual-sync | 0 | Faithful upstream fixture/rectangle-reset/pixel comparison translated and executed: 111001 differing pixels. Existing FreeText/callout, Caret and Polygon handlers are stubs; Line endings/caption, Highlight shape and Square rectangle difference also deviate. Test now has an explicit Skip documenting this pre-existing behavior gap; original assertion is unchanged. |
| `pdfbox/src/test/java/org/apache/pdfbox/rendering/TestQuality.java` | `tests/PdfBox.Net.Tests/Rendering/TestQuality.cs` | — | `046747d` | semantic-divergence | in-sync | 0 | Ported all three changed upstream fixtures/assertions at 100 DPI with verified SHA-512 provenance. Strengthened 6077 with the green image payload and 5842 with the red map marker, because non-white assertions would accept the old opaque black stencil. |
| `pdfbox/src/test/java/org/apache/pdfbox/util/TestDateUtil.java` | `tests/PdfBox.Net.Tests/DateConverterTest.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream fixes typo in Antarctica/McMurdo timezone identifier and tests JDK DST history. .NET tests intentionally use fixed SimpleTimeZone offsets; no misspelled McMurdo identifier exists. Historical JDK TZDB assertions deferred to native timezone adapter coverage. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/font/PDFBOX-6249-cns1-nonembedded-latin.pdf` | — | `1187c45` | `046747d` | none | in-sync | 0 | Original upstream PDFBOX-6249 repro validated externally with full official NotoSansCJKtc-Regular (git blob f9376bae1d421520f73a3c6c9d50b22f89548301,16435884 bytes). H outline/width and 中 cmap mapping pass; rendered text includes both ideographs. Deterministic committed synthetic CID-keyed font and no-ToUnicode CNS test avoid installed-font requirements in CI. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/interactive/annotation/Annotations.pdf` | `tests/PdfBox.Net.Tests/Fixtures/Annotations.pdf` | — | `046747d` | semantic-divergence | in-sync | 0 | Copied upstream fixture exactly for AppearanceGenerationTest. Initial genuine pixel comparison failed by 111001 pixels; existing FreeText/Caret/Polygon handler stubs and Line/Highlight/Square geometry gaps recorded in assessment. Test retained with explicit Skip; assertion is unchanged. |
| `tools/src/main/java/org/apache/pdfbox/tools/Decrypt.java` | `src/PdfBox.Net/Tools/Decrypt.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream documentation typo and-and is absent in the .NET CLI implementation. |
| `tools/src/main/java/org/apache/pdfbox/tools/Encrypt.java` | `src/PdfBox.Net/Tools/Encrypt.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | Upstream documentation typo and-and is absent in the .NET CLI implementation. |
| `tools/src/main/java/org/apache/pdfbox/tools/PDFText2Markdown.java` | `src/PdfBox.Net/Tools/PDFText2Markdown.cs` | `ccd281c` | `046747d` | semantic-divergence | in-sync | 0 | The intentional text-only .NET converter uses fenced literal text, not upstream inline HTML/font styling. Hardened fence length and closing-line placement so all upstream metacharacters including backticks/HTML remain literal; styled Markdown remains the pre-existing adaptation. |

## Upstream-test parity by production class

| Production class | Test parity and evidence |
|---|---|
| `PDFDebugger` | converted: Issue954DebuggerDataTest certificate string/stream and COS inspection; Swing-only interaction deferred to any future GUI |
| `CertificatePane` | converted: Issue954DebuggerDataTest real self-signed DER/string/stream and malformed input |
| `PanoseFlag` | converted: Issue954DebuggerDataTest missing/short/exact/long strings and invalid classification values |
| `PagePane` | deferred: Swing event-loop and OS dispatcher tests outside headless product; no document link is dispatched by .NET debugger |
| `FontToolTip` | converted: Issue954AnnotationAndTooltipTest uses actual resource font and verifies inert HTML, passed |
| `AddAnnotations` | existing example suite; visual fixture parity reported separately |
| `BengaliPdfGenerationHelloWorld` | upstream no new test; existing example suite |
| `ExtractEmbeddedFiles` | converted native integration: TestExtractEmbeddedFiles normal extraction, parent traversal, directory symlink and final-file symlink tests all passed on macOS |
| `CreateEmbeddedTimeStamp` | converted native regression saves/loads malformed two-element ByteRange and asserts correct IOException before TSA/output; passed. Existing detached CMS signing/checking passed; online timestamp fixtures remain explicitly skipped |
| `SigUtils` | converted native TestSigUtils exact allowlist rejections and fake-handler exact HTTPS upgrade, denied redirect and redirect-chain checks all passed; no live HTTP calls |
| `CRLVerifier` | converted native denied-URL and FTP tests passed; upstream LDAP network test deferred due missing LDAP transport |
| `CertificateVerificationResult` | not-applicable: compatibility retention; existing certificate verification tests |
| `OcspHelper` | existing OCSP tests; CheckAccess unit regression shared with SigUtils |
| `CMapParser` | converted: CMapParserTest invalid predefined resource names plus existing mapping fixture; CMapParserTest verifies all 92 maps parse and all four CJK encoding/Unicode paths |
| `PfbParser` | converted: PfbAndType1FontTest short reads/EOF marker/huge truncated size/malformed segments and existing Type1 semantic fixture |
| `HeaderTable` | converted: existing TTFParserTest header fields |
| `TTFParser` | converted: TTFParserTest successful/malformed input closure and header-vs-complete parsing |
| `GlyphLayoutFontLoaderAwt` | Allocation-only route reuses the existing tested font parser. The 145-test focused gate (including CID substitute/metadata tests) passed; root full-suite validation is separate. No upstream AWT loader test changed in this window; actual AWT shaping/subsetting parity remains a baseline gap. |
| `GlyphLayoutFontLoaderFop` | not-applicable |
| `PDFStreamEngine` | converted: existing ContentStreamEngineTest character code processing |
| `COSName` | converted: COSPrimitivesTest and annotation OPEN tests |
| `CCITTFaxFilter` | converted: FilterTest.CCITTFaxDecodeBoundsChangingElementBuffersBeforeReadingInput plus CCITT round trips |
| `FlateFilterDecoderStream` | converted: existing FlateFilterDecoderStream/FilterTest behavior |
| `PDFMergerUtility` | not-applicable: documentation-only delta |
| `COSParser` | converted: existing parser/indirect-reference tests; no new null path introduced |
| `COSWriter` | converted: existing SaveIncrementalTests; unchanged allocation-equivalent path |
| `PDUserAttributeObject` | converted: existing structure-tree/user-property tests |
| `RC4Cipher` | converted: existing encryption known-vector/roundtrip tests |
| `StandardSecurityHandler` | converted: existing standard-security round trips/password vectors |
| `FDFDictionary` | converted: FDFUtilsTest dictionary-writer XML parse regression passed in initial core focused run |
| `FDFField` | converted: all changed write sites covered by FDFUtilsTest; initial focused run passed |
| `FDFUtils` | converted: complete upstream parameter cases plus invalid-surrogate/codepoint boundaries and XML parse regressions, passed |
| `AcroFormOrphanWidgetsProcessor` | existing regression suite; upstream adds no changed test |
| `FontMapperImpl` | converted: all six upstream CIDCharSetMatchTest cases plus masks, weight, DroidSans/barcode regression; FileSystemFontMetadataTest validates real TTF/CFF parser metadata/cmap/outlines/cache, installed substitution and bundled fallback; all passed in 906 focused run |
| `PDCIDFontType0` | converted: PDCIDFontType0SubstituteTest synthetic Identity CID-keyed OTF and UniCNS without ToUnicode; independent real PDFBOX-6249 plus full official NotoSansCJKtc-Regular passed (H present/13 segments/width728; 中 GID9544 equals cmap; rendered Hello Tika 123 中文); added UniCNS-UTF16-H without ToUnicode, CNS CID34 to A to substitute GID1 regression after real-fixture gate exposed absent CMaps |
| `PDType1FontEmbedder` | converted: FontStubsReplacementTest.PDType1Font_StreamConstructor_EmbedsFontProgramAndMetrics and streaming PFB regressions |
| `CCITTFactory` | Existing CCITTFactory image/decode tests and new caller-input lifetime test passed. Raw-TIFF passthrough/page-number overloads and exact Java unsupported-mode exception parity remain pre-existing adaptations. |
| `JPEGFactory` | JPEGFactoryStreamingTest and existing ImageFactoryTest passed: forward-only short reads, caller stream lifetime, raw-byte and pixel preservation, malformed/truncated JPEG segments. Full Java JPEGFactoryTest encoding-suite parity remains deferred because the baseline factory lacks createFromImage. |
| `PDAnnotation` | converted: Issue954AnnotationAndTooltipTest RGB/gray/interior invariants passed; wider new AppearanceGenerationTest separately reports baseline gap |
| `PDAnnotationText` | existing annotation API coverage; no new upstream tests |
| `AppearanceGeneratorHelper` | existing PDAppearanceGenerationTest validates both generated/updated parseable appearance streams |
| `PageDrawer` | 145/145 focused tests passed (0 skipped), including 13 synthetic stencil/pattern/softmask/graphics-state tests, all three new real TestQuality fixtures, AdvancedRenderingIssue419Test, JPEGFactoryStreamingTest, ImageFactoryTest, CIDCharSetMatchTest, FileSystemFontMetadataTest, Issue954DebuggerDataTest and PDCIDFontType0SubstituteTest. The synthetic tests cover inline/XObject, inverse Decode, DPI 72/144, reflected patterns, q/Q state, transparent gaps, and alpha masks. Real tests run at 100 DPI; TestQuality also checks positive image/color payload to reject empty or black output. Final AA-fix gate: 108/108 tests passed, 0 skipped (StencilRenderingTest, Rendering.TestQuality, AdvancedRenderingIssue419Test, ImageFactoryTest). All four added 36-DPI transparent-stencil/transparent-border cases failed before the fix and passed after it, with nearest and interpolated sampling; painted-center assertions reject empty output. Synthetic stencil/state coverage is now 17 tests. |
| `SoftMask` | Native SoftMask_CreateContext_AppliesMaskAlpha now checks a nonzero origin. Upstream has no direct SoftMask test; new TestQuality fixture coverage exercises the Skia path. Initial focused run passed. |
| `PDFTextStripper` | converted: Issue954TextPageTraversalTest plus runtime text parity |
| `PDFTextStripperByArea` | converted: Issue954TextPageTraversalTest.ExtractRegions_ProcessesEmptyPage |
| `Decrypt` | not-applicable: documentation-only delta |
| `Encrypt` | not-applicable: documentation-only delta |
| `PDFText2Markdown` | converted: Issue954MarkdownEscapingTest literal metacharacters and embedded closing fence |

## Explicit deferred tests and baseline behavior

| Upstream test/source | Disposition |
|---|---|
| `debugger/src/test/java/org/apache/pdfbox/debugger/pagepane/PagePaneTest.java` | deferred: no native OS URI-dispatch path; New isBrowsableScheme test exercises Java Desktop link launching, absent in the .NET headless inspector. No dormant allowlist is added to a model that never opens URIs. |
| `examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateSignature.java` | deferred online TSA/LTV/visible paths remain explicit skips; both existing deterministic detached CMS and empty form tests passed; Changed online LTV invocation and new TSA-certificate test require live TSA/CRL/OCSP; existing .NET deterministic signing test retained without empty success placeholder. |
| `examples/src/test/java/org/apache/pdfbox/examples/signature/cert/CRLVerifierTest.java` | deferred: LDAP transport and real CRL verification absent in baseline .NET adapter; New LDAP CRL download/signature-validation test cannot run against .NET adapter because LDAP retrieval is explicitly unsupported. No success-shaped placeholder added. |
| `fontbox/src/test/java/org/apache/fontbox/pfb/PfbParserTest.java` | partial: deterministic parser regressions converted; external font corpus deferred; Ported negative/huge/short input validation and getters via deterministic synthetic Type1 fixture. External OpenSans/DejaVu/KIX full-glyph assertions are deferred because those fixture downloads are not part of .NET test assets. |
| `fontbox/src/test/java/org/apache/fontbox/ttf/TestTTFParser.java` | partial: synthetic header contract converted; external IPA/font-specific values deferred; Ported header-vs-full-font comparison and source closure checks with repository-owned synthetic font. Exact LiberationSans kerning/IPA vertical-property corpus assertions are deferred; no corresponding production metric change exists in this batch. |
| `fontbox/src/test/java/org/apache/fontbox/ttf/TrueTypeFontCollectionTest.java` | partial: malformed collection coverage converted; installed font fixture assertions deferred; Added exact missing-header/invalid-font-count exceptions. OS-installed-font assertions are deferred to provider tests using deterministic temporary collections rather than requiring Windows/macOS bundled fonts in CI. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/fdf/FDFUtilsTest.java` | converted; passed; All 13 upstream cases translated, additional field/dictionary integration and invalid-codepoint coverage. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/CIDCharSetMatchTest.java` | converted: six upstream cases plus eleven misleading-codepage cases and four ranking/filter tests; passed; All new upstream ROS/codepage cases ported, with independent regression checks for codepage masks and font ranking. Test-only metadata provider proves filtering does not need outline loading; production inventory separately tested with binary fonts. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0SubstituteTest.java` | converted; Converted with deterministic synthetic CID-keyed Identity OTF and explicit mapper, so CID!=GID and nonempty outline/width assertions run on every machine without installed-font assumptions. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactoryTest.java` | Adapted equivalents passed; full upstream createFromImage quality/encoding suite is deferred because the baseline .NET factory lacks those APIs.; The upstream edit changes Color object identity to value equality in a comparison helper; equal colors already yielded zero RGB error before the change. Existing .NET image assertions compare byte/channel values; no corresponding identity-equality defect exists. Native JPEGFactoryStreamingTest covers the changed factory input route. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdmodel/interactive/annotation/AppearanceGenerationTest.java` | deferred: production handler prerequisites predate this upstream window and are outside the changed production slice; no all-appearance parity claim; Faithful upstream fixture/rectangle-reset/pixel comparison translated and executed: 111001 differing pixels. Existing FreeText/callout, Caret and Polygon handlers are stubs; Line endings/caption, Highlight shape and Square rectangle difference also deviate. Test now has an explicit Skip documenting this pre-existing behavior gap; original assertion is unchanged. |
| `pdfbox/src/test/java/org/apache/pdfbox/rendering/TestQuality.java` | All three changed upstream test methods pass at 100 DPI, including stronger green-image payload and red-marker assertions. PDFBOX-6077 and PDFBOX-5842 prove before/after improvements; PDFBOX-5403 already passed the upstream dark-pixel check at baseline and is retained as a regression guard. Initial unchanged PDFBOX-4831 bitonal-scan identity test remains a baseline test-parity gap.; Ported all three changed upstream fixtures/assertions at 100 DPI with verified SHA-512 provenance. Strengthened 6077 with the green image payload and 5842 with the red map marker, because non-white assertions would accept the old opaque black stencil. |
| `pdfbox/src/test/java/org/apache/pdfbox/util/TestDateUtil.java` | deferred: JDK timezone database specific change; fixed-offset conversion suite unchanged; Upstream fixes typo in Antarctica/McMurdo timezone identifier and tests JDK DST history. .NET tests intentionally use fixed SimpleTimeZone offsets; no misspelled McMurdo identifier exists. Historical JDK TZDB assertions deferred to native timezone adapter coverage. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/font/PDFBOX-6249-cns1-nonembedded-latin.pdf` | external-validation; Original upstream PDFBOX-6249 repro validated externally with full official NotoSansCJKtc-Regular (git blob f9376bae1d421520f73a3c6c9d50b22f89548301,16435884 bytes). H outline/width and 中 cmap mapping pass; rendered text includes both ideographs. Deterministic committed synthetic CID-keyed font and no-ToUnicode CNS test avoid installed-font requirements in CI. |
| `pdfbox/src/test/resources/org/apache/pdfbox/pdmodel/interactive/annotation/Annotations.pdf` | deferred: missing pre-existing appearance handler behavior outside the changed production slice; Copied upstream fixture exactly for AppearanceGenerationTest. Initial genuine pixel comparison failed by 111001 pixels; existing FreeText/Caret/Polygon handler stubs and Line/Highlight/Square geometry gaps recorded in assessment. Test retained with explicit Skip; assertion is unchanged. |

The annotation regression was run before adding its explicit skip. At 72 DPI the upstream original appearances and regenerated .NET appearances differed in 111,001 pixels. Independent visual review localized absent FreeText/callout, Caret and Polygon graphics to existing default-appearance stubs, plus Line arrow/caption, Highlight and Square geometry differences. The fixture and exact zero-difference assertion remain committed, so restoring those pre-existing handlers can activate the test without weakening it.

## Similarity, dependencies and normalization

- Parser changes retain upstream method boundaries and error semantics; .NET Stream lifetime and bounded buffer operations replace Java InputStream APIs.
- CID mapping uses existing FontBox CFF/TrueType parsers and FontProvider abstractions. Extra touched files are direct prerequisites: DefaultFontProvider, FileSystemFontProvider, FontMapper, FontMappers, PDType0Font, the embedded fallback font and CMap data. No new font/rendering package is introduced.
- Type 1 embedding is implemented in PDType1Font.CreateEmbeddedType1FontData. PDType1FontEmbedder remains an internal compatibility encoding holder; stale metadata claiming no PFB embedding support is corrected.
- Native Skia stencil compositing is an adapted backend implementation of the upstream paint/mask behavior; the core SoftMask remains Java-shaped.
- Annotation markup and square/circle readers share the new color conversion helper so interior colors follow the same gray normalization as /C.
- External fonts and borrowed streams preserve existing ownership boundaries. AWT font bytes are passed to the existing internal loader directly.
- The existing COS dynamic-name cache test held only a WeakReference before checking initial cache membership. Its initial assertion now runs in the NoInlining helper while the strong reference is alive, followed by GC.KeepAlive; subsequent collection and cache-removal assertions are unchanged.

## Validation

- Final Release solution build succeeded; final solution suite: **1,657 passed, 0 failed, 15 skipped**. The additional skip is the explicitly documented upstream appearance-handler gap.
- Focused rendering/font/debugger/image gate: 145 passed, 0 failed, 0 skipped. The later antialiasing correction passed a 108-test renderer gate, including four new cases observed failing before the fix; synthetic stencil/state coverage is now 17 tests.
- FontBox full Release suite: 164 passed, including all 92 predefined CMaps, four CJK Unicode mapping paths and the streaming/parser regressions. Focused examples gate: 17 passed, 0 failed, 6 existing skips.
- Independent API-surface generator and unchanged ratchet against target Apache: passed, with 0 unreviewed deltas and 0 invalid dispositions. The validated artifacts are `reports/api-surface-comparison.json` and `reports/pdfbox-api-surface-analysis.md`.
- Independent runtime gate and unchanged ratchet: **691 operation rows across 161 PDFs and 80 merge pairs**, all classified as matches, 0 known and 0 unexpected divergences. Visual-equivalence count is 72, within the existing allowance of 72. The 24 prior trailing-whitespace semantic gaps are eliminated. The aggregate summary's `match` counter also counts categories; the 691-row count is used here to avoid double-counting.
- Independent real PDFBOX-6249 plus full official NotoSansCJKtc-Regular gate passed: H is present with 13 path segments and width 728; 中 maps to GID 9544, equal to the font Unicode cmap; the rendered page contains “Hello Tika 123 中文”. Font provenance: official Noto git blob `f9376bae1d421520f73a3c6c9d50b22f89548301`, 16,435,884 bytes. The committed no-ToUnicode CNS test exercises this prerequisite deterministically.
- Core and FontBox NuGet packs succeeded. The packages contain `licenses/LiberationSans/LICENSE.txt` (SIL OFL) and `licenses/AdobeCMaps/LICENSE` (Adobe CMap terms). All 92 CMaps and the Liberation Sans font were independently checked byte-for-byte against the exact upstream revision.
- Canonical inventory: 1,075 / 1,075 in-scope production Java sources mapped, 0 missing. All 67 changed upstream paths have a disposition, with no missing or extra paths. Every changed/new C# file has conversion, normalization and traceability coverage; no report-row gaps.

## Artifact integrity

- Applicable source headers are stamped only after the corresponding implementation passes focused validation. Unchanged dependency headers keep their previous markers.
- Deferred tests keep their old or absent sync marker; the test result and absent prerequisites are explicit in the log.
- CMap and rendering fixture source hashes are recorded alongside the resources; imported font/CMap license texts are preserved.
- The initial unstaged `git diff --check` omitted newly imported files. The final staged check includes them: `.gitattributes` exempts only upstream CMap trailing whitespace and the copied Liberation license’s space-before-tab indentation, preserving byte-for-byte integrity. All other whitespace checks remain enabled; `git diff --cached --check` passes with these narrowly scoped attributes.

## Commit audit

| Commit | Upstream subject |
|---|---|
| `46f998c1631e26be6968b440bbd942633ed8acce` | PDFBOX-6077: combine stencil mask alpha with the pattern's own alpha, fix soft-masked patterns used as a stencil mask fill, dilate paint alpha before combining with the stencil mask, by Valery Bokov and Claude Code; closes #491 |
| `d74ddf04c6a9d8e1fd58a42d90ab7ce2b62a9b8d` | PDFBOX-6233: refactor to return origin only |
| `3c69bdf5a0b7927d99e7d6fa2a25712db0303fa6` | PDFBOX-6077: go back to assigning alpha instead of combining, but only if not transparent; optimize |
| `97fe6ef975ae64bc84f021bcff2426ef0dcf9456` | PDFBOX-6235: fix equals, by Valery Bokov; closes #496 |
| `386b026a33980df0bccb5ac3b5fd97ceb7d9cef5` | PDFBOX-5660: optimize, by Valery Bokov; closes #497 |
| `dc27f2b9889c1c9ce21dabed8f0f5032329b0637` | PDFBOX-6077: assign alpha instead of combining, but only if not transparent; optimize; add comment |
| `f7f4c1cf2c8f4240903468658086a76f2c82c852` | PDFBOX-6233: remove my own almost 10 year old TODO comment - it's unlikely this will ever be done due to increased complexity of that part |
| `2b1aa32dc1f4b048e597bff505b2b6f902750085` | PDFBOX-5660: remove outdated |
| `8d71146f52f2a4cda9768d0c6df652981cf0e988` | PDFBOX-5660: add allowlist |
| `3ae9f116be50a83270cdd54b48e9f91ddb3e0c4b` | PDFBOX-5070: add test |
| `7df2bb43001810e9a4a283d4fb2325074639d684` | PDFBOX-6237: add LDAP download test; add some URIs |
| `d761a8cad6ed583a5dda774b135fdc2ba69c3bb4` | PDFBOX-6238: remove unused |
| `1ed76f05aee7d2432a2ef4c35dfb20bedd05d99d` | PDFBOX-5660: call main to improve code coverage |
| `c8966d5cabc368cc35ec334020559738dccb67de` | PDFBOX-5660: update byte-buddy |
| `93385b1583f4ed162a3c32e2d144f1795dc56bb6` | PDFBOX-2941: avoid / confirm URI |
| `8a235c7b1e8dc27e3aa4eaacb32bee4fa8ed0786` | PDFBOX-2941: remove entries that aren't needed, add junit for next commit |
| `cfd574a0cb29ffa095aa55515dee0beb357c5155` | PDFBOX-2941: add test |
| `e49212b0c5f31392a87ef434f34bd8569885b51c` | PDFBOX-2941: escape font name |
| `9211c7929fb87d28e456bdf569f560b18ee5b3d4` | PDFBOX-2941: restore jbig2 |
| `276388d0c2ce3f26fafcb2f6e7da2acdd8a73c9e` | PDFBOX-6077: Add regression tests for stencil-mask pattern fills, by Valery Bokov; closes #492 |
| `656aab3c0099fa39739b2c795edb4084aae0cee8` | PDFBOX-6239: show certificate details |
| `a26c27af7965325b554ebc3d378776b5420fbac6` | PDFBOX-2941: simplify signature check |
| `1ba96c7131d449d9c6d63fdc631aac4205ffbaed` | PDFBOX-6239: refactor |
| `e36e5d41dc80d1f6446dc69e29cabf127e4146b0` | PDFBOX-6239: refactor |
| `6ef022401873bc6f9a13de1a9e5485f931134d54` | PDFBOX-2941: refactor |
| `a07f766363a31bfcf8ee15fb5bfdf05c45b5ffb4` | PDFBOX-2941: refactor |
| `4825fbfd1d6d073eed659dc087d5d16e06845afd` | PDFBOX-3353: improve example |
| `1ce74adc463b89b73caacee3732be74937238608` | PDFBOX-3353: add rendering comparison test for the appearances of the annotations in our example |
| `8aed5b9f49b4e4ae525f051ed4f9a3925f476386` | PDFBOX-3353: expand existing test; return DeviceGray annotation color if R=G=B |
| `231aa964e6f2f9c36c2928375e90a037ccb9dc29` | PDFBOX-6240: change rectangle collecting to be done after rendering; improve map; display popup window for most annotations (swing parts by github copilot) |
| `3713f4d951e70d7a6b769619efe64ff39497a9ed` | PDFBOX-2941: add COSName.OPEN |
| `b55c2f93639abff258f7d63d53089c9227fe9ee5` | PDFBOX-6240: avoid NPE, as suggested by copilot |
| `f285cd11d364cce987fc59d342cea7997e156c2b` | PDFBOX-6240: reset caret, as suggested by copilot |
| `b4566cc393a05d538ab2675143356548196dc7a9` | PDFBOX-6240: fix logic issue, as suggested by copilot |
| `3fd4da30e8ddb5e0575de27bfd33cebe983678cf` | PDFBOX-6240: clear the text if you want stale content to never briefly flash, as suggested by copilot |
| `f43102cf71dd7694de51dd9684b5ef8fcaebfac8` | PDFBOX-6240: simplify copilot code, as suggested by copilot |
| `ff3603ec64fb6c73810853dadd73b9652fc27fb1` | PDFBOX-6240: simplify copilot code, as suggested by copilot |
| `696c75905a1d8ec10ecbaad62d6f561a384503ad` | PDFBOX-6240: add screen-bound popup positioning, as suggested by copilot |
| `340d52673586abe212cd34df8cab3cb01aad2562` | PDFBOX-6240: remove unused, as suggested by copilot |
| `d452f25a5fb2ce78612e37923cc16841e3cabad5` | PDFBOX-6240: add a title bar to the popup; swing changes by copilot |
| `44e14a72f63a49f95a1d002af2dc506e8690dd13` | PDFBOX-5660: refactor XML escaping |
| `92400e178b9992d66483a755a535d9e2121aa317` | PDFBOX-5660: enhance markdown output |
| `ff6c492c3ad954cb4bd173d198c8bc53fb4f69a7` | PDFBOX-5660: validate CMap name |
| `7df40df4c55a530f5022cc269363a7cd85bd810e` | PDFBOX-6241: align the security definition on the web page and for agents |
| `4d50786435f6a7196a3f28f42b6e2e8ce1a5f3bd` | PDFBOX-6242: properly escape characters for XML 1.0; parts by Claude Sonnet |
| `d32565b137f50dd2581280ae0e5c431413ce554b` | PDFBOX-6242: enhance javadoc |
| `4ecceb2ded047c1f98eed4b054cf90967f4b40b9` | PDFBOX-6243: improve validate dimension to avoid OOM |
| `d8215aff757c75e5e4aa1eec24173e4e2dc7816b` | PDFBOX-6244: Clarify that parsing doesn't check signatures |
| `aed8a21820a5ff0ba1ecd60e547e53fae4a1c74e` | PDFBOX-6244: Clarify that parsing doesn't check signatures (text by github copilot) |
| `30237d7e6962f81b93bb15ec554f3aa4109d24e1` | PDFBOX-6244: Clarify that parsing doesn't check signatures, by Maruan Sahyoun |
| `9686dc2061d861aed92904e22d2f63ea83f53e6a` | PDFBOX-6244: revert |
| `7b6e9db01cb31a6914b84178b1cde2a8727e4101` | PDFBOX-6244: clarify handling of PDF semantics and validation |
| `a905346e4676e1547f57de081e647a2d4f696bdc` | PDFBOX-5660: fix typo, as suggested by Valery Bokov; closes #499; closes #500 |
| `37d277f02b11ba73ba3831258334c04fb632f456` | PDFBOX-5660: improve test coverage |
| `871c2149c6d7c2b69b2cd57cb923d47107221374` | PDFBOX-5660: improve test coverage |
| `c28e49c0574b96eb923b26190c8b15c35824706f` | PDFBOX-5660: refactor, as suggested by Valery Bokov; closes #498; improve test coverage |
| `e5648855b6de3baf5e9c01d9ae1c5f32ad230176` | PDFBOX-6245: load test file |
| `5c302d1df021315b556eca211c4e763b6ffae569` | PDFBOX-6245: add test for 3654 |
| `fbf2cf586d9b7844e3bb51c7cc2550529bb80995` | PDFBOX-6246: exclude symlinks, thanks [LE VAN LUONG/lu0ng] (https://github.com/lu0ng) |
| `702caa7ce18f44bd7005be06dc61ab6d8ff3f542` | PDFBOX-5660: remove unneeded close, as suggested by Valery Bokov; closes #501 |
| `1f0ea10cd6276568e1cc520071c53df75de95f7a` | PDFBOX-5660: improve test coverage |
| `a8a92c27b1e3500ec0b920aa07aedf1da7c1edec` | PDFBOX-6145: remove content check so that not all pages get checked |
| `e73bf0f10703ae1547903f953740a1c1fb66e39a` | PDFBOX-5660: improve test coverage |
| `43231bace85746330ac84487f2549e21fb8f0dbd` | PDFBOX-5660: improve test coverage |
| `57f46cd1d20fbf9a3bd16bcd065c2c9561095423` | PDFBOX-5660: remove super() |
| `5d0571abba95773e78681f2a8f2ac7803875d3ae` | PDFBOX-5660: improve test coverage |
| `88e0a829ee49704f694aecd8f9a5bb2705b20ccf` | PDFBOX-5660: Sonar fix |
| `6935e0a698eeda2e0822477bbbaaf7cf88a45a51` | PDFBOX-5660: improve test coverage |
| `872e62207d373f26ba51f5f7ac6e56100deb0209` | PDFBOX-5660: DRY refactoring |
| `3b76d2f52b6e2d551eaa7c71b97e93c224227bbf` | PDFBOX-5660: delete always the same offset argument, as suggested by Valery Bokov; closes #504 |
| `f0bb91daa907e790fe899bec7e058c22bb6ffaad` | PDFBOX-5660: delete unneeded code, as suggested by Valery Bokov; closes #502 |
| `cc1249e64c0ded8b54bc5dbbb3e182fa867ff0dc` | PDFBOX-6247: use byte array method, as suggested by Valery Bokov; closes #505 |
| `0b64f6c2a342586e4eacb8b5b7eaf83cc71214f3` | PDFBOX-6248: fix error messages |
| `2254a761218d312e5b25f0992060e44569a0333e` | PDFBOX-3243: improve javadoc |
| `582567c5f087d10656ae3653558508f819142766` | PDFBOX-5660: simplify / optimize, as suggested by Valery Bokov; closes #503 |
| `c05f833a6920cc9ace3357a966a9c65f22278190` | PDFBOX-5660: simplify / optimize, as suggested by Valery Bokov; closes #506 |
| `6ce58b47067a094ed23980567b4289f14370c4be` | PDFBOX-5660: refactor + avoid mark with max int, as suggested by Valery Bokov; closes #511 |
| `b9de8299ef80efc8bd7f3fddb5c8a3c63ee07a5c` | PDFBOX-5660: simplify / optimize, as suggested by Valery Bokov; closes #510 |
| `53676c13de7937f92d69f7e4043a2f3ee3076b55` | PDFBOX-5660: improve test coverage |
| `3aa283c892cf77b606bfc98d12db2f700d53537d` | PDFBOX-6250: substitute Adobe-Identity-0 CID-keyed fonts for non-embedded legacy-ROS fonts + prefer a regular weight when the descriptor has no weight information, by Tim Allison and ðŸ¤– |
| `47ac6d8b1aec27f890335c507632c34a04fdebca` | PDFBOX-6250: add test file by Claude |
| `44ae1f5a0371c37128b20fac2beecdfd0c93b503` | PDFBOX-6249: Sonar fix |
| `3b63803c3a1628d4ac802d8cd1880e30e9e1ceee` | PDFBOX-6249: correct wrongly set bits |
| `ba5bb773c13ffbaf51396226ce72fee0d144a829` | PDFBOX-6249: avoid DroidSansFallback |
| `7ada62807acf28e37716f1d28f8336f14998d293` | PDFBOX-5660: refactor tests |
| `6888784f0d8076962a341540f8c1216323d56f3f` | PDFBOX-4951: refactor to avoid double allocation, as suggested by Valery Bokov; closes #514; closes #518 |
| `9e9ce8177c8a71569a76d06dd035bb80689e36f4` | PDFBOX-5660: refactor to reduce available() calls, as suggested by Valery Bokov; closes #517; rename variable for clarity |
| `bcf045388c2e17aa0d7792d37eaa71de053ef6f0` | PDFBOX-5660: avoid NPE |
| `32eaf2317fb0ad4c2f9b987902febbfee3d419d0` | PDFBOX-5660: Sonar fix |
| `a401a89c8d810074d93303e08518e163a383d687` | PDFBOX-2941: avoid exceptions |
| `4aa80d810fd24e5f8a09ce133cd03a016f99cc28` | PDFBOX-5660: Sonar fix |
| `19a6f0ccc053f0d3d5eb0f570d335a888047216a` | PDFBOX-2941: Sonar fix |
| `ced684bbab4340a5c80e0cf3e1c270ebb98120d3` | PDFBOX-5660: avoid NPE |
| `f7654f00138705d23780760167be9a9a13a010a6` | PDFBOX-5660: avoid NPE |
| `d404dd774d515995805ab7b79a23f6b530e28245` | PDFBOX-5660: avoid NPE |
| `c766e829831f02900fd9d5875738aaa9f18bbf98` | PDFBOX-5660: avoid NPE |
| `de68eb3e394b59244a5867acd2c7dfb85dee006d` | PDFBOX-5660: avoid NPE |
| `2ff5c9ab3bc2f31095c83427848762e279aabcd8` | PDFBOX-5660: avoid NPE |
| `e83fb5ddfcbf225ea625671984be47411090b303` | PDFBOX-5660: avoid ArrayIndexOutOfBoundsException, improve logging |
| `1e50c58efae5f02a10bb290514896850c0b0d4e8` | PDFBOX-5660: avoid NPE |
| `534bd487aa31b5d2fe965015c25e7bf4b2d2329d` | PDFBOX-5660: Sonar fix |
| `0079a9cc7845043cf657ac9e4277e5755eeda229` | PDFBOX-6254: fix typo + fix test |
| `9713012d282c350e81b136fdfa3927d9fac4e1a7` | PDFBOX-5660: refactor, as suggested by Valery Bokov; closes #519 |
| `046747da99a870902217efabf1c41297de157059` | PDFBOX-5660: update byte-buddy |
