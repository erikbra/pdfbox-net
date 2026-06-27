/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDAnnotationAdditionalActions.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDAnnotationAdditionalActions
{
    public PDAction? Bl
    {
        get => GetBl();
        set => SetBl(value!);
    }

    public PDAction? D
    {
        get => GetD();
        set => SetD(value!);
    }

    public PDAction? E
    {
        get => GetE();
        set => SetE(value!);
    }

    public PDAction? Fo
    {
        get => GetFo();
        set => SetFo(value!);
    }

    public PDAction? PC
    {
        get => GetPC();
        set => SetPC(value!);
    }

    public PDAction? PI
    {
        get => GetPI();
        set => SetPI(value!);
    }

    public PDAction? PO
    {
        get => GetPO();
        set => SetPO(value!);
    }

    public PDAction? PV
    {
        get => GetPV();
        set => SetPV(value!);
    }

    public PDAction? U
    {
        get => GetU();
        set => SetU(value!);
    }

    public PDAction? X
    {
        get => GetX();
        set => SetX(value!);
    }
}
