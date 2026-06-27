/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/optionalcontent/PDOptionalContentMembershipDictionary.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.PDModel.Graphics.OptionalContent;

public partial class PDOptionalContentMembershipDictionary
{
    public COSArray? VisibilityExpression
    {
        get => GetVisibilityExpression();
        set => SetVisibilityExpression(value!);
    }

    public COSName VisibilityPolicy
    {
        get => GetVisibilityPolicy();
        set => SetVisibilityPolicy(value);
    }
}
