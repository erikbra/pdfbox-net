/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDSoftMask.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.State;

public sealed partial class PDSoftMask
{
    public Matrix? InitialTransformationMatrix
    {
        get => GetInitialTransformationMatrix();
        set => SetInitialTransformationMatrix(value!);
    }
}
