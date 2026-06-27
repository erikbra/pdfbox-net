/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSignature.java
 */

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public partial class PDSignature
{
    public int[] ByteRange
    {
        get => GetByteRange();
        set => SetByteRange(value);
    }

    public string? ContactInfo
    {
        get => GetContactInfo();
        set => SetContactInfo(value!);
    }

    public byte[] Contents
    {
        get => GetContents();
        set => SetContents(value);
    }

    public string? Location
    {
        get => GetLocation();
        set => SetLocation(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public PDPropBuild? PropBuild
    {
        get => GetPropBuild();
        set => SetPropBuild(value!);
    }

    public string? Reason
    {
        get => GetReason();
        set => SetReason(value!);
    }

}
