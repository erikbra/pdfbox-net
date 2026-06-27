/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java
 */

using PdfBox.Net.PDModel.Annotations;

namespace PdfBox.Net.Rendering;

public partial class PageDrawer
{
    public AnnotationFilter AnnotationFilter
    {
        get => GetAnnotationFilter();
        set => SetAnnotationFilter(value);
    }
}
