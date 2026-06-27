/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDPropBuildDataDict.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public partial class PDPropBuildDataDict
{
    public string? Date
    {
        get => GetDate();
        set => SetDate(value!);
    }

    public long MinimumRevision
    {
        get => GetMinimumRevision();
        set => SetMinimumRevision(value);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public bool NonEFontNoWarn
    {
        get => GetNonEFontNoWarn();
        set => SetNonEFontNoWarn(value);
    }

    public string? OS
    {
        get => GetOS();
        set => SetOS(value!);
    }

    public bool PreRelease
    {
        get => GetPreRelease();
        set => SetPreRelease(value);
    }

    public long Revision
    {
        get => GetRevision();
        set => SetRevision(value);
    }

    public bool TrustedMode
    {
        get => GetTrustedMode();
        set => SetTrustedMode(value);
    }

    public string? Version
    {
        get => GetVersion();
        set => SetVersion(value!);
    }
}
