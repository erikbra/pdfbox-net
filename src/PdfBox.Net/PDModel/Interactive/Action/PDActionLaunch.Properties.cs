/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionLaunch.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionLaunch
{
    public string? D
    {
        get => GetD();
        set => SetD(value!);
    }

    public string? F
    {
        get => GetF();
        set => SetF(value!);
    }

    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }

    public string? O
    {
        get => GetO();
        set => SetO(value!);
    }

    public OpenMode OpenInNewWindow
    {
        get => GetOpenInNewWindow();
        set => SetOpenInNewWindow(value);
    }

    public string? P
    {
        get => GetP();
        set => SetP(value!);
    }

    public PDWindowsLaunchParams? WinLaunchParams
    {
        get => GetWinLaunchParams();
        set => SetWinLaunchParams(value!);
    }
}
