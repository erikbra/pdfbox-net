# PdfBox.Net 3.0 Source Coverage

Datetime (UTC): 2026-06-30T15:35:33.593Z
PdfBox.Net commit: `48ce3d1e4c1c1f52477aeb2e0d39b73b61c24b1f`
Apache upstream ref: `origin/3.0` (`ea68b6feae80e671b3d26565b12eccc79e74d967`)
Comparison ref: `origin/trunk` (`ed358c48dc5ab3f20687cc4a6bf9529436641ecd`)

## Scope

This report compares the `release/3.0` branch against Apache PDFBox `origin/3.0` as a living upstream branch.

Excluded modules:
- `preflight`: 116 production Java files
- `preflight-app`: 0 production Java files

Preflight/PDF-A validation is intentionally excluded from the initial 3.0 core parity branch. It was removed from Apache PDFBox trunk/4.0 and should be treated as a separate package or milestone if needed later.

## Summary

| Measure | Count |
|---|---:|
| Scoped Apache 3.0 Java files | 1071 |
| Mapped Java files | 1071 |
| Missing non-Preflight Java files | 0 |
| Coverage percent | 100.0% |
| Mapped source paths not present in 3.0 | 16 |
| 3.0 source paths not present in trunk | 12 |
| Trunk source paths not present in 3.0 | 8 |
| Common source paths changed vs trunk | 367 |
| Mapped common source paths changed vs trunk | 367 |

## Module Coverage

| Module | Java files | Mapped | Missing | Coverage |
|---|---:|---:|---:|---:|
| `benchmark` | 4 | 4 | 0 | 100.0% |
| `debugger` | 91 | 91 | 0 | 100.0% |
| `examples` | 94 | 94 | 0 | 100.0% |
| `fontbox` | 143 | 143 | 0 | 100.0% |
| `io` | 18 | 18 | 0 | 100.0% |
| `pdfbox` | 621 | 621 | 0 | 100.0% |
| `tools` | 26 | 26 | 0 | 100.0% |
| `xmpbox` | 74 | 74 | 0 | 100.0% |

## Missing Non-Preflight Files

None. Issue #588 reconciled all twelve previously missing non-Preflight Apache PDFBox 3.0 production Java source files with converted or compatibility .NET source files and traceability rows.

## Mapped Source Paths Not In Apache 3.0

These are currently mapped .NET source provenance paths that do not exist in Apache `origin/3.0`. They are likely trunk/4.0-only files or files renamed/removed between 3.0 and trunk, and should be reconciled while shaping the 3.0 branch.

- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/DebugLogAppender.java` -> `src/PdfBox.Net.Debugger/Ui/DebugLogAppender.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateComboBox.java` -> `src/PdfBox.Net.Examples/Interactive/Form/CreateComboBox.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateListBox.java` -> `src/PdfBox.Net.Examples/Interactive/Form/CreateListBox.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/interactive/form/FlattenAllFormFields.java` -> `src/PdfBox.Net.Examples/Interactive/Form/FlattenAllFormFields.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/rendering/ConvertPDFPagesToImages.java` -> `src/PdfBox.Net.Examples/Rendering/ConvertPDFPagesToImages.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/MemoryTTFDataStream.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/MemoryTTFDataStream.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForTamil.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForTamil.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/type3/SetType3GlyphWidth.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/SetType3GlyphWidth.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/type3/SetType3GlyphWidthAndBoundingBox.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/SetType3GlyphWidthAndBoundingBox.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/pdfparser/XrefParser.java` -> `src/PdfBox.Net/PdfParser/XrefParser.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/AppearanceStyle.java` -> `src/PdfBox.Net/PDModel/Interactive/AppearanceStyle.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/PlainText.java` -> `src/PdfBox.Net/PDModel/Interactive/PlainText.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/PlainTextFormatter.java` -> `src/PdfBox.Net/PDModel/Interactive/PlainTextFormatter.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/TextAlign.java` -> `src/PdfBox.Net/PDModel/Interactive/TextAlign.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrinter.java` -> `src/PdfBox.Net/Printing/PDFPrinter.cs`
- `xmpbox/src/main/java/org/apache/xmpbox/schema/XMPPageTextSchema.java` -> `src/PdfBox.Net.XmpBox/XmpBox/Schema/XMPPageTextSchema.cs`

## Changed Common Source Files

These mapped Java source paths exist in both Apache `origin/3.0` and `origin/trunk`, but their Java blob differs. The current .NET port has been tracking trunk, so these files form the initial review queue for issue #589.

- `benchmark/src/main/java/org/apache/pdfbox/benchmark/LoadAndSave.java` -> `src/PdfBox.Net.Benchmark/LoadAndSave.cs`, `src/PdfBox.Net.Benchmark/LoadAndSaveBenchmarks.cs`
- `benchmark/src/main/java/org/apache/pdfbox/benchmark/Rendering.java` -> `src/PdfBox.Net.Benchmark/Rendering.cs`, `src/PdfBox.Net.Benchmark/RenderingBenchmarks.cs`
- `benchmark/src/main/java/org/apache/pdfbox/benchmark/TextExtraction.java` -> `src/PdfBox.Net.Benchmark/TextExtraction.cs`, `src/PdfBox.Net.Benchmark/TextExtractionBenchmarks.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/PDFDebugger.java` -> `src/PdfBox.Net.Debugger/PDFDebugger.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/DeviceNTableModel.java` -> `src/PdfBox.Net.Debugger/Colorpane/DeviceNTableModel.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/IndexedTableModel.java` -> `src/PdfBox.Net.Debugger/Colorpane/IndexedTableModel.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/FontEncodingPaneController.java` -> `src/PdfBox.Net.Debugger/Fontencodingpane/FontEncodingPaneController.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/FontPane.java` -> `src/PdfBox.Net.Debugger/Fontencodingpane/FontPane.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/SimpleFont.java` -> `src/PdfBox.Net.Debugger/Fontencodingpane/SimpleFont.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/Type0Font.java` -> `src/PdfBox.Net.Debugger/Fontencodingpane/Type0Font.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/pagepane/DebugTextOverlay.java` -> `src/PdfBox.Net.Debugger/Pagepane/DebugTextOverlay.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/pagepane/PagePane.java` -> `src/PdfBox.Net.Debugger/Pagepane/PagePane.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/Stream.java` -> `src/PdfBox.Net.Debugger/Streampane/Stream.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/StreamPane.java` -> `src/PdfBox.Net.Debugger/Streampane/StreamPane.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/StreamTextView.java` -> `src/PdfBox.Net.Debugger/Streampane/StreamTextView.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/FontToolTip.java` -> `src/PdfBox.Net.Debugger/Streampane/Tooltip/FontToolTip.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/KToolTip.java` -> `src/PdfBox.Net.Debugger/Streampane/Tooltip/KToolTip.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/SCNToolTip.java` -> `src/PdfBox.Net.Debugger/Streampane/Tooltip/SCNToolTip.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/ToolTipController.java` -> `src/PdfBox.Net.Debugger/Streampane/Tooltip/ToolTipController.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/LogDialog.java` -> `src/PdfBox.Net.Debugger/Ui/LogDialog.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/OSXAdapter.java` -> `src/PdfBox.Net.Debugger/Ui/OSXAdapter.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/PDFTreeCellRenderer.java` -> `src/PdfBox.Net.Debugger/Ui/PDFTreeCellRenderer.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/PrintDpiMenu.java` -> `src/PdfBox.Net.Debugger/Ui/PrintDpiMenu.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/ReaderBottomPanel.java` -> `src/PdfBox.Net.Debugger/Ui/ReaderBottomPanel.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/RecentFiles.java` -> `src/PdfBox.Net.Debugger/Ui/RecentFiles.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/TextDialog.java` -> `src/PdfBox.Net.Debugger/Ui/TextDialog.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/Tree.java` -> `src/PdfBox.Net.Debugger/Ui/Tree.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/ZoomMenu.java` -> `src/PdfBox.Net.Debugger/Ui/ZoomMenu.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/textsearcher/SearchEngine.java` -> `src/PdfBox.Net.Debugger/Ui/Textsearcher/SearchEngine.cs`
- `debugger/src/main/java/org/apache/pdfbox/debugger/ui/textsearcher/Searcher.java` -> `src/PdfBox.Net.Debugger/Ui/Textsearcher/Searcher.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/lucene/IndexPDFFiles.java` -> `src/PdfBox.Net.Examples/Lucene/IndexPDFFiles.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePDFA.java` -> `src/PdfBox.Net.Examples/PDModel/CreatePDFA.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePatternsPDF.java` -> `src/PdfBox.Net.Examples/PDModel/CreatePatternsPDF.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/EmbeddedMultipleFonts.java` -> `src/PdfBox.Net.Examples/PDModel/EmbeddedMultipleFonts.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ExtractTTFFonts.java` -> `src/PdfBox.Net.Examples/PDModel/ExtractTTFFonts.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/printing/OpaquePDFRenderer.java` -> `src/PdfBox.Net.Examples/Printing/OpaquePDFRenderer.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/rendering/CustomPageDrawer.java` -> `src/PdfBox.Net.Examples/Rendering/CustomPageDrawer.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/CMSProcessableInputStream.java` -> `src/PdfBox.Net.Examples/Signature/CMSProcessableInputStream.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/CreateEmbeddedTimeStamp.java` -> `src/PdfBox.Net.Examples/Signature/CreateEmbeddedTimeStamp.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/CreateSignatureBase.java` -> `src/PdfBox.Net.Examples/Signature/CreateSignatureBase.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/CreateSignedTimeStamp.java` -> `src/PdfBox.Net.Examples/Signature/CreateSignedTimeStamp.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/CreateVisibleSignature.java` -> `src/PdfBox.Net.Examples/Signature/CreateVisibleSignature.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/ShowSignature.java` -> `src/PdfBox.Net.Examples/Signature/ShowSignature.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java` -> `src/PdfBox.Net.Examples/Signature/SigUtils.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/TSAClient.java` -> `src/PdfBox.Net.Examples/Signature/TSAClient.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CRLVerifier.java` -> `src/PdfBox.Net.Examples/Signature/Cert/CRLVerifier.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CertificateVerifier.java` -> `src/PdfBox.Net.Examples/Signature/Cert/CertificateVerifier.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/cert/OcspHelper.java` -> `src/PdfBox.Net.Examples/Signature/Cert/OcspHelper.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/validation/AddValidationInformation.java` -> `src/PdfBox.Net.Examples/Signature/Validation/AddValidationInformation.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/validation/CertInformationCollector.java` -> `src/PdfBox.Net.Examples/Signature/Validation/CertInformationCollector.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/signature/validation/CertInformationHelper.java` -> `src/PdfBox.Net.Examples/Signature/Validation/CertInformationHelper.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/util/DrawPrintTextLocations.java` -> `src/PdfBox.Net.Examples/Util/DrawPrintTextLocations.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/util/PDFHighlighter.java` -> `src/PdfBox.Net.Examples/Util/PDFHighlighter.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/util/PDFMergerExample.java` -> `src/PdfBox.Net.Examples/Util/PDFMergerExample.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/util/PrintTextColors.java` -> `src/PdfBox.Net.Examples/Util/PrintTextColors.cs`
- `examples/src/main/java/org/apache/pdfbox/examples/util/PrintTextLocations.java` -> `src/PdfBox.Net.Examples/Util/PrintTextLocations.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/CFFCIDFont.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/CFFCIDFont.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/CFFParser.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/CFFParser.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/CFFType1Font.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/CFFType1Font.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/CharStringCommand.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/CharStringCommand.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/DataInputRandomAccessRead.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/DataInputRandomAccessRead.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/Type1CharString.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/Type1CharString.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/Type1CharStringParser.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/Type1CharStringParser.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/Type2CharString.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/Type2CharString.cs`
- `fontbox/src/main/java/org/apache/fontbox/cff/Type2CharStringParser.java` -> `src/PdfBox.Net.FontBox/FontBox/CFF/Type2CharStringParser.cs`
- `fontbox/src/main/java/org/apache/fontbox/cmap/CMap.java` -> `src/PdfBox.Net.FontBox/FontBox/CMap/CMap.cs`
- `fontbox/src/main/java/org/apache/fontbox/cmap/CMapStrings.java` -> `src/PdfBox.Net.FontBox/FontBox/CMap/CMapStrings.cs`
- `fontbox/src/main/java/org/apache/fontbox/pfb/PfbParser.java` -> `src/PdfBox.Net.FontBox/FontBox/Pfb/PfbParser.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/CmapSubtable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/CmapSubtable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeDescript.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GlyfCompositeDescript.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/GlyfSimpleDescript.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GlyfSimpleDescript.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/GlyphRenderer.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GlyphRenderer.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/GlyphSubstitutionTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GlyphSubstitutionTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/KerningSubtable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/KerningSubtable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/KerningTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/KerningTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/NamingTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/NamingTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/OS2WindowsMetricsTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/OS2WindowsMetricsTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/OpenTypeScript.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/OpenTypeScript.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/PostScriptTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/PostScriptTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/RandomAccessReadDataStream.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/MemoryTTFDataStream.cs`, `src/PdfBox.Net.FontBox/FontBox/TTF/RandomAccessReadDataStream.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/TTFDataStream.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/TTFDataStream.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/TTFParser.cs`, `src/PdfBox.Net.FontBox/FontBox/TTF/TTFSupportStubs.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/TTFSubsetter.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/TTFSubsetter.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/TrueTypeFont.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/TrueTypeFont.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/WGL4Names.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/WGL4Names.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/CompoundCharacterTokenizer.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/CompoundCharacterTokenizer.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/DefaultGsubWorker.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/DefaultGsubWorker.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GlyphSubstitutionDataExtractor.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GlyphSubstitutionDataExtractor.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerFactory.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerFactory.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForBengali.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForBengali.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForDevanagari.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForDevanagari.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForDflt.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForDflt.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForGujarati.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForGujarati.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForLatin.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/GSub/GsubWorkerForLatin.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/model/Language.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Model/Language.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/model/MapBackedGsubData.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Model/MapBackedGsubData.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/model/MapBackedScriptFeature.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Model/GlyphIdListComparer.cs`, `src/PdfBox.Net.FontBox/FontBox/TTF/Model/MapBackedScriptFeature.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/model/ScriptFeature.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Model/ScriptFeature.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/table/common/CoverageTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Table/Common/CoverageTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/ttf/table/common/LookupSubTable.java` -> `src/PdfBox.Net.FontBox/FontBox/TTF/Table/Common/LookupSubTable.cs`
- `fontbox/src/main/java/org/apache/fontbox/type1/Type1Lexer.java` -> `src/PdfBox.Net.FontBox/FontBox/Type1/Type1Lexer.cs`
- `fontbox/src/main/java/org/apache/fontbox/util/autodetect/FontFileFinder.java` -> `src/PdfBox.Net.FontBox/FontBox/Util/Autodetect/FontFileFinder.cs`
- `fontbox/src/main/java/org/apache/fontbox/util/autodetect/NativeFontDirFinder.java` -> `src/PdfBox.Net.FontBox/FontBox/Util/Autodetect/NativeFontDirFinder.cs`
- `fontbox/src/main/java/org/apache/fontbox/util/autodetect/WindowsFontDirFinder.java` -> `src/PdfBox.Net.FontBox/FontBox/Util/Autodetect/WindowsFontDirFinder.cs`
- `io/src/main/java/org/apache/pdfbox/io/IOUtils.java` -> `src/PdfBox.Net.IO/IO/IOUtils.cs`
- `io/src/main/java/org/apache/pdfbox/io/NonSeekableRandomAccessReadInputStream.java` -> `src/PdfBox.Net.IO/IO/NonSeekableRandomAccessReadInputStream.cs`
- `io/src/main/java/org/apache/pdfbox/io/RandomAccessInputStream.java` -> `src/PdfBox.Net.IO/IO/RandomAccessInputStream.cs`
- `io/src/main/java/org/apache/pdfbox/io/RandomAccessReadBuffer.java` -> `src/PdfBox.Net.IO/IO/RandomAccessReadBuffer.cs`, `src/PdfBox.Net/IO/RandomAccessReadBuffer.cs`
- `io/src/main/java/org/apache/pdfbox/io/RandomAccessReadBufferedFile.java` -> `src/PdfBox.Net.IO/IO/RandomAccessReadBufferedFile.cs`, `src/PdfBox.Net/IO/RandomAccessReadBufferedFile.cs`
- `io/src/main/java/org/apache/pdfbox/io/ScratchFile.java` -> `src/PdfBox.Net.IO/IO/ScratchFile.cs`, `src/PdfBox.Net/IO/ScratchFile.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/Loader.java` -> `src/PdfBox.Net/Loader.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDFStreamEngine.java` -> `src/PdfBox.Net/ContentStream/PDFStreamEngine.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/DrawObject.java` -> `src/PdfBox.Net/ContentStream/Operator/DrawObject.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetColor.java` -> `src/PdfBox.Net/ContentStream/Operator/Color/SetColor.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/ClosePath.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/ClosePath.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CurveTo.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/CurveTo.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CurveToReplicateFinalPoint.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/CurveToReplicateFinalPoint.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CurveToReplicateInitialPoint.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/CurveToReplicateInitialPoint.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/DrawObject.java` -> `src/PdfBox.Net/ContentStream/Operator/DrawObject.cs`
- `pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/LineTo.java` -> `src/PdfBox.Net/ContentStream/Operator/Graphics/LineTo.cs`
- ... 247 more entries in `reports/pdfbox-3.0-source-coverage.json`

## Machine-Readable Detail

Full lists, including all changed common source paths and target C# mappings, are in `reports/pdfbox-3.0-source-coverage.json`.

## Follow-Up Issues

- #588: port or reconcile the missing non-Preflight source files.
- #589: review mapped common source files that differ between Apache 3.0 and trunk.
- #590: retarget the runtime corpus and ratchet baseline to Apache 3.0.
- #591: align CI/package metadata once the branch-specific reports and baselines exist.
