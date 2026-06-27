/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDObjectStream.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public partial class PDObjectStream
{
    public PDObjectStream? Extends
    {
        get => GetExtends();
        set => SetExtends(value!);
    }

    public int FirstByteOffset
    {
        get => GetFirstByteOffset();
        set => SetFirstByteOffset(value);
    }

    public int NumberOfObjects
    {
        get => GetNumberOfObjects();
        set => SetNumberOfObjects(value);
    }
}
