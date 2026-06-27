/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/form/PDFormXObject.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Form;

public partial class PDFormXObject
{
    public PDRectangle? BBox
    {
        get => GetBBox();
        set => SetBBox(value!);
    }

    public int FormType
    {
        get => GetFormType();
        set => SetFormType(value);
    }

    public PDTransparencyGroupAttributes? Group
    {
        get => GetGroup();
        set => SetGroup(value!);
    }

    public PDPropertyList? OptionalContent
    {
        get => GetOptionalContent();
        set => SetOptionalContent(value!);
    }

    public PDResources? Resources
    {
        get => GetResources();
        set => SetResources(value!);
    }

    public int StructParents
    {
        get => GetStructParents();
        set => SetStructParents(value);
    }
}
