# QA assessment — issue 46 documentinterchange parent-tree and integration

- Upstream-test parity
  - Converted: Added `PDParentTreeIntegrationTest` (29 tests) covering:
    - Parent-tree set/get round-trip on `PDStructureTreeRoot`
    - `PDParentTreeNumberTreeNode.GetStructureElements` for single-dictionary values
    - `PDParentTreeNumberTreeNode.GetStructureElements` for array-of-dictionary values (page-level)
    - Missing-key guard returns empty list
    - `PDStructureTreeRoot.GetParentTreeEntries` returns empty when no parent tree is present
    - End-to-end catalog → `StructTreeRoot` → `ParentTree` → structure elements traversal
    - Multi-page stability: different keys resolve to different element lists
    - `PDLayoutAttributeObject`, `PDListAttributeObject`, `PDTableAttributeObject` owner, typed
      accessors, and null/default values
    - `PDAttributeObject.Create` factory dispatch to Layout, List, Table, and fallback to Default
  - Deferred: Fixture-based test with a real on-disk tagged PDF (no such fixture currently in the
    repository). The in-memory construction tests cover the identical code paths.
  - Updated: `PDAttributeObjectTest.StructureTreeRoot_ClassMap_SingleAttributeRoundTrip` updated
    from `Assert.IsType<PDDefaultAttributeObject>` to `Assert.IsType<PDLayoutAttributeObject>` now
    that the factory dispatches correctly for the `Layout` owner.
- Source-to-port similarity confidence
  - Adapted: `PDParentTreeNumberTreeNode` is a C#-idiomatic subclass of `PDNumberTreeNode` that
    overrides `ConvertCOSToPD` and `CreateChildNode` and adds `GetStructureElements(int)`.
  - Adapted: `PDStructureTreeRoot` parent-tree accessors (`GetParentTree`, `SetParentTree`,
    `GetParentTreeEntries`) are direct mechanical translations of upstream Java.
  - Adapted: `PDLayoutAttributeObject`, `PDListAttributeObject`, `PDTableAttributeObject` are
    direct mechanical translations of upstream Java with standard C# property patterns.
  - Adapted: `PDAttributeObject.Create` factory extended with owner-dispatch for Layout/List/Table.
- Report-row gaps
  - Added conversion-records rows for `PDParentTreeNumberTreeNode.cs`,
    `PDStructureTreeRoot.cs` (update), `PDLayoutAttributeObject.cs`, `PDListAttributeObject.cs`,
    `PDTableAttributeObject.cs`, `PDAttributeObject.cs` (update), and
    `PDParentTreeIntegrationTest.cs`.
  - Added traceability rows for the same files with `in-sync` status.
  - Updated `PDStructureTreeRoot` traceability note to reflect parent-tree integration complete.
  - Updated `pdfbox-main-gap-analysis.md`: documentinterchange coverage raised from ~55% to ~75%,
    9→13+ ported files, completed-in-#46 section added.
