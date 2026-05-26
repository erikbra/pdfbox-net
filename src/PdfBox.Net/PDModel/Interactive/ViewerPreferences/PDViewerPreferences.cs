/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/viewerpreferences/PDViewerPreferences.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.ViewerPreferences;

public class PDViewerPreferences : COSObjectable
{
    public enum NonFullScreenPageMode
    {
        UseNone,
        UseOutlines,
        UseThumbs,
        UseOC
    }

    public enum ReadingDirection
    {
        L2R,
        R2L
    }

    public enum Boundary
    {
        MediaBox,
        CropBox,
        BleedBox,
        TrimBox,
        ArtBox
    }

    public enum Duplex
    {
        Simplex,
        DuplexFlipShortEdge,
        DuplexFlipLongEdge
    }

    public enum PrintScaling
    {
        None,
        AppDefault
    }

    private static readonly COSName HideToolbarName = COSName.GetPDFName("HideToolbar");
    private static readonly COSName HideMenubarName = COSName.GetPDFName("HideMenubar");
    private static readonly COSName HideWindowUIName = COSName.GetPDFName("HideWindowUI");
    private static readonly COSName FitWindowName = COSName.GetPDFName("FitWindow");
    private static readonly COSName CenterWindowName = COSName.GetPDFName("CenterWindow");
    private static readonly COSName DisplayDocTitleName = COSName.GetPDFName("DisplayDocTitle");
    private static readonly COSName NonFullScreenPageModeName = COSName.GetPDFName("NonFullScreenPageMode");
    private static readonly COSName DirectionName = COSName.GetPDFName("Direction");
    private static readonly COSName ViewAreaName = COSName.GetPDFName("ViewArea");
    private static readonly COSName ViewClipName = COSName.GetPDFName("ViewClip");
    private static readonly COSName PrintAreaName = COSName.GetPDFName("PrintArea");
    private static readonly COSName PrintClipName = COSName.GetPDFName("PrintClip");
    private static readonly COSName DuplexName = COSName.GetPDFName("Duplex");
    private static readonly COSName PrintScalingName = COSName.GetPDFName("PrintScaling");

    private readonly COSDictionary _preferences;

    public PDViewerPreferences()
    {
        _preferences = new COSDictionary();
    }

    public PDViewerPreferences(COSDictionary dictionary)
    {
        _preferences = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSDictionary GetCOSObject() => _preferences;

    COSBase COSObjectable.GetCOSObject() => _preferences;

    public bool HideToolbar() => _preferences.GetBoolean(HideToolbarName, false);

    public void SetHideToolbar(bool value) => _preferences.SetBoolean(HideToolbarName, value);

    public bool HideMenubar() => _preferences.GetBoolean(HideMenubarName, false);

    public void SetHideMenubar(bool value) => _preferences.SetBoolean(HideMenubarName, value);

    public bool HideWindowUI() => _preferences.GetBoolean(HideWindowUIName, false);

    public void SetHideWindowUI(bool value) => _preferences.SetBoolean(HideWindowUIName, value);

    public bool FitWindow() => _preferences.GetBoolean(FitWindowName, false);

    public void SetFitWindow(bool value) => _preferences.SetBoolean(FitWindowName, value);

    public bool CenterWindow() => _preferences.GetBoolean(CenterWindowName, false);

    public void SetCenterWindow(bool value) => _preferences.SetBoolean(CenterWindowName, value);

    public bool DisplayDocTitle() => _preferences.GetBoolean(DisplayDocTitleName, false);

    public void SetDisplayDocTitle(bool value) => _preferences.SetBoolean(DisplayDocTitleName, value);

    public NonFullScreenPageMode GetNonFullScreenPageMode() => ParseOrDefault(
        _preferences.GetNameAsString(NonFullScreenPageModeName),
        NonFullScreenPageMode.UseNone);

    public void SetNonFullScreenPageMode(NonFullScreenPageMode value) => _preferences.SetName(NonFullScreenPageModeName, value.ToString());

    public ReadingDirection GetReadingDirection() => ParseOrDefault(
        _preferences.GetNameAsString(DirectionName),
        ReadingDirection.L2R);

    public void SetReadingDirection(ReadingDirection value) => _preferences.SetName(DirectionName, value.ToString());

    public Boundary GetViewArea() => ParseOrDefault(
        _preferences.GetNameAsString(ViewAreaName),
        Boundary.CropBox);

    public void SetViewArea(Boundary value) => _preferences.SetName(ViewAreaName, value.ToString());

    public Boundary GetViewClip() => ParseOrDefault(
        _preferences.GetNameAsString(ViewClipName),
        Boundary.CropBox);

    public void SetViewClip(Boundary value) => _preferences.SetName(ViewClipName, value.ToString());

    public Boundary GetPrintArea() => ParseOrDefault(
        _preferences.GetNameAsString(PrintAreaName),
        Boundary.CropBox);

    public void SetPrintArea(Boundary value) => _preferences.SetName(PrintAreaName, value.ToString());

    public Boundary GetPrintClip() => ParseOrDefault(
        _preferences.GetNameAsString(PrintClipName),
        Boundary.CropBox);

    public void SetPrintClip(Boundary value) => _preferences.SetName(PrintClipName, value.ToString());

    public Duplex? GetDuplex()
    {
        string? value = _preferences.GetNameAsString(DuplexName);
        return Enum.TryParse(value, false, out Duplex duplex) ? duplex : null;
    }

    public void SetDuplex(Duplex value) => _preferences.SetName(DuplexName, value.ToString());

    public PrintScaling GetPrintScaling() => ParseOrDefault(
        _preferences.GetNameAsString(PrintScalingName),
        PrintScaling.AppDefault);

    public void SetPrintScaling(PrintScaling value) => _preferences.SetName(PrintScalingName, value.ToString());

    private static T ParseOrDefault<T>(string? value, T defaultValue) where T : struct, Enum
    {
        return Enum.TryParse(value, false, out T parsed) ? parsed : defaultValue;
    }
}
