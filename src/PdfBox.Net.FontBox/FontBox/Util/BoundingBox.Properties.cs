/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/util/BoundingBox.java
 */

namespace PdfBox.Net.FontBox.Util;

public partial class BoundingBox
{
    public float LowerLeftX
    {
        get => GetLowerLeftX();
        set => SetLowerLeftX(value);
    }

    public float LowerLeftY
    {
        get => GetLowerLeftY();
        set => SetLowerLeftY(value);
    }

    public float UpperRightX
    {
        get => GetUpperRightX();
        set => SetUpperRightX(value);
    }

    public float UpperRightY
    {
        get => GetUpperRightY();
        set => SetUpperRightY(value);
    }
}
