### Title
Add missing close-fill graphics operator processors (b and b*)

### Background
Two PDF path-painting operators are defined in `OperatorName.cs` but have no corresponding
processor class and are not registered in `PDFStreamEngine.RegisterOperators()`:

- `b` (`CLOSE_FILL_NON_ZERO_AND_STROKE`) — close the current subpath, fill using the
  non-zero winding number rule, then stroke the path. Equivalent to `h B`.
- `b*` (`CLOSE_FILL_EVEN_ODD_AND_STROKE`) — close the current subpath, fill using the
  even-odd rule, then stroke the path. Equivalent to `h B*`.

These are rare in modern PDFs but present in some older documents and Type 3 font glyph
streams. Their absence means any content stream using `b` or `b*` will silently skip the
operation.

### Depends on
- `ClosePath` processor (already ported — `h`)
- `FillNonZeroAndStrokePath` processor (already ported — `B`)
- `FillEvenOddAndStrokePath` processor (already ported — `B*`)

### Scope

1. Add `src/PdfBox.Net/ContentStream/Operator/Graphics/CloseAndFillNonZeroAndStrokePath.cs`
   — implements operator `b`: calls `ClosePath.Process()` then
   `FillNonZeroAndStrokePath.Process()`.

2. Add `src/PdfBox.Net/ContentStream/Operator/Graphics/CloseAndFillEvenOddAndStrokePath.cs`
   — implements operator `b*`: calls `ClosePath.Process()` then
   `FillEvenOddAndStrokePath.Process()`.

3. Register both in `PDFStreamEngine.RegisterOperators()`:
   ```csharp
   AddOperator(new CloseAndFillNonZeroAndStrokePath(this));
   AddOperator(new CloseAndFillEvenOddAndStrokePath(this));
   ```

4. Add provenance headers mirroring the other graphics operator files.

### Expected test scope
- Extend `OperatorProcessorsTest.cs` with two test cases verifying that `b` and `b*`
  dispatch correctly through the engine without throwing.
- Verify the path is closed (last point equals first point) before fill+stroke.

### Entry criteria
- `dotnet build` passes and 638 tests pass.

### Exit criteria
- Both operator processors are registered and functional.
- New tests for `b` and `b*` pass.
- `conversion-records.json` updated with the two new files.

### Risk register
- Low risk. The implementation simply composes existing ClosePath + Fill+Stroke processors.

### Definition of done
- `dotnet build` passes.
- `OperatorProcessorsTest.cs` extended with passing tests.
- Two new source files added with provenance headers.
- `reports/conversion-records.json` updated.
