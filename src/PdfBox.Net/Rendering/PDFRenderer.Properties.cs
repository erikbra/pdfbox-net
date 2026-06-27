/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/rendering/PDFRenderer.java
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Rendering;

public partial class PDFRenderer
{
    public AnnotationFilter AnnotationsFilter
    {
        get => GetAnnotationsFilter();
        set => SetAnnotationsFilter(value);
    }

    public float ImageDownscalingOptimizationThreshold
    {
        get => GetImageDownscalingOptimizationThreshold();
        set => SetImageDownscalingOptimizationThreshold(value);
    }

    public RenderingHints? RenderingHints
    {
        get => GetRenderingHints();
        set => SetRenderingHints(value!);
    }
}
