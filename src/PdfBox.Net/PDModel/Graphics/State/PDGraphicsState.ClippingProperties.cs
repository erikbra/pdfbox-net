/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade helpers for retained clipping state.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDGraphicsState.java
 */

using PdfBox.Net.ContentStream;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.State;

public partial class PDGraphicsState
{
    /// <summary>
    /// Identifies a retained clipping path construction command.
    /// </summary>
    public enum ClippingPathCommandType
    {
        MoveTo,
        LineTo,
        CurveTo,
        Close
    }

    /// <summary>
    /// One retained clipping path construction command in source coordinates.
    /// </summary>
    public readonly record struct ClippingPathCommand(
        ClippingPathCommandType Type,
        float X1,
        float Y1,
        float X2,
        float Y2,
        float X3,
        float Y3);

    public sealed partial class ClippingPath
    {
        /// <summary>
        /// Gets the retained path construction commands for layout and export consumers.
        /// </summary>
        public IReadOnlyList<ClippingPathCommand> GetCommands()
        {
            return Segments.Select(static segment => new ClippingPathCommand(
                segment.Type switch
                {
                    PDFStreamEngine.PathSegmentType.MoveTo => ClippingPathCommandType.MoveTo,
                    PDFStreamEngine.PathSegmentType.LineTo => ClippingPathCommandType.LineTo,
                    PDFStreamEngine.PathSegmentType.CurveTo => ClippingPathCommandType.CurveTo,
                    _ => ClippingPathCommandType.Close
                },
                segment.X1,
                segment.Y1,
                segment.X2,
                segment.Y2,
                segment.X3,
                segment.Y3)).ToArray();
        }
    }

    /// <summary>
    /// Gets transformed axis-aligned bounds for each active clipping path.
    /// </summary>
    /// <remarks>
    /// The bounds are intentionally conservative for non-rectangular paths. Consumers that
    /// need exact geometry can use <see cref="ClippingPath.GetCommands"/> together with the
    /// clipping path's transformation matrix and winding rule.
    /// </remarks>
    public IReadOnlyList<PDRectangle> GetCurrentClippingBounds()
    {
        List<PDRectangle> bounds = new(_clippingPaths.Count);
        foreach (ClippingPath clippingPath in _clippingPaths)
        {
            if (GetTransformedBounds(clippingPath) is PDRectangle rectangle)
            {
                bounds.Add(rectangle);
            }
        }

        return bounds;
    }

    private static PDRectangle? GetTransformedBounds(ClippingPath clippingPath)
    {
        bool hasPoint = false;
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;

        foreach (PDFStreamEngine.PathSegment segment in clippingPath.Segments)
        {
            switch (segment.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                case PDFStreamEngine.PathSegmentType.LineTo:
                    Include(segment.X1, segment.Y1);
                    break;
                case PDFStreamEngine.PathSegmentType.CurveTo:
                    Include(segment.X1, segment.Y1);
                    Include(segment.X2, segment.Y2);
                    Include(segment.X3, segment.Y3);
                    break;
            }
        }

        return hasPoint
            ? new PDRectangle(minX, minY, maxX - minX, maxY - minY)
            : null;

        void Include(float x, float y)
        {
            Vector point = clippingPath.CurrentTransformationMatrix.Transform(x, y);
            if (!hasPoint)
            {
                minX = maxX = point.GetX();
                minY = maxY = point.GetY();
                hasPoint = true;
                return;
            }

            minX = MathF.Min(minX, point.GetX());
            minY = MathF.Min(minY, point.GetY());
            maxX = MathF.Max(maxX, point.GetX());
            maxY = MathF.Max(maxY, point.GetY());
        }
    }
}
