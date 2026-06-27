/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDRectlinearMeasureDictionary.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public partial class PDRectlinearMeasureDictionary
{
    public PDNumberFormatDictionary[]? Angles
    {
        get => GetAngles();
        set => SetAngles(value!);
    }

    public PDNumberFormatDictionary[]? Areas
    {
        get => GetAreas();
        set => SetAreas(value!);
    }

    public float CYX
    {
        get => GetCYX();
        set => SetCYX(value);
    }

    public PDNumberFormatDictionary[]? ChangeXs
    {
        get => GetChangeXs();
        set => SetChangeXs(value!);
    }

    public PDNumberFormatDictionary[]? ChangeYs
    {
        get => GetChangeYs();
        set => SetChangeYs(value!);
    }

    public float[]? CoordSystemOrigin
    {
        get => GetCoordSystemOrigin();
        set => SetCoordSystemOrigin(value!);
    }

    public PDNumberFormatDictionary[]? Distances
    {
        get => GetDistances();
        set => SetDistances(value!);
    }

    public PDNumberFormatDictionary[]? LineSloaps
    {
        get => GetLineSloaps();
        set => SetLineSloaps(value!);
    }

    public PDNumberFormatDictionary[]? LineSlopes
    {
        get => GetLineSlopes();
        set => SetLineSlopes(value!);
    }

    public string ScaleRatio
    {
        get => GetScaleRatio();
        set => SetScaleRatio(value);
    }
}
