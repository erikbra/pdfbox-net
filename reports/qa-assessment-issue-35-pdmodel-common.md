# QA assessment — issue 35 PDModel/Common slice

- Upstream-test parity
  - Converted: `PDIntegerNameTreeNode`, `TestPDNameTreeNode`, `TestPDNumberTreeNode`, `Type4Tester`, and the core scenarios from `TestPDFunctionType4` / `TestParser` into xUnit v3 coverage.
  - Adapted: Added focused `PDPageLabelsTest` and `PDFunctionType2` assertions because the issue requires page-label and Type 2 behavior checks but upstream coverage is distributed differently.
  - Deferred: the full upstream `TestOperators` matrix was not ported in this slice; the Type 4 execution path is covered by parser/basic operator scenarios and the end-to-end function tests added here.
- Source-to-port similarity confidence
  - Mechanical: wrappers, file specifications, most function classes, parser/tokenizer helpers, and the upstream-derived tests remain close to the Java originals.
  - Adapted: `COSArrayList`, tree-node reflection handling, sampled-function bit reading, execution-stack emulation, page-label generation plumbing, and the PD catalog / color-space integrations were adjusted for .NET/runtime differences.
- Report-row gaps
  - Added conversion, normalization, and traceability rows for every new production/test file in this slice.
  - Existing integration edits in `PDDocumentCatalog`, `PDSoftMask`, `PDSeparation`, and `PDDeviceN` reuse already-tracked upstream mappings and therefore do not introduce new report rows.
