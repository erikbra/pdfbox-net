# Issue 010 — `PDFGraphicsStreamEngine` abstract callback API

## Summary

Implement the abstract callback API on `PDFGraphicsStreamEngine` so that examples which
subclass it to intercept graphics operations (paths, images, text) compile and work correctly.

## Required API surface

- `PDFGraphicsStreamEngine` abstract class with the following abstract methods:
  - `AppendRectangle(PointF p0, PointF p1, PointF p2, PointF p3)` — rectangle path
  - `DrawImage(PDImageXObject pdImage)` — image draw callback
  - `Clip(int windingRule)` — clip path callback
  - `MoveTo(float x, float y)` — move-to path
  - `LineTo(float x, float y)` — line-to path
  - `CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)` — curve-to path
  - `GetCurrentPoint()` — current path point
  - `EndPath()` — end current path
  - `StrokePath()` — stroke callback
  - `FillPath(int windingRule)` — fill callback
  - `FillAndStrokePath(int windingRule)` — fill+stroke callback
  - `ShadingFill(COSName shadingName)` — shading fill callback
- `Run(PDPage page)` — processes the page content stream, dispatching callbacks

## Affected example files

- `Rendering/CustomGraphicsStreamEngine.cs`
- `Rendering/CustomPageDrawer.cs`

## Acceptance criteria

- Both example files compile without stubs and run without exceptions on a sample PDF.
- Integration tests verify that `CustomGraphicsStreamEngine` and `CustomPageDrawer` execute
  without throwing.
- Traceability rows for both affected source paths are `in-sync`.
