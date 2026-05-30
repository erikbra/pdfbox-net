/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/FontHeaders.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// To improve performance of font scanning, this class is used both as a marker and as a storage for collected data.
/// </summary>
public sealed class FontHeaders
{
    internal const int BYTES_GCID = 142;

    public string? Error { get; private set; }
    public string? Name { get; private set; }
    public int? HeaderMacStyle { get; private set; }
    public OS2WindowsMetricsTable? OS2Windows { get; private set; }
    public string? FontFamily { get; private set; }
    public string? FontSubFamily { get; private set; }
    public byte[]? NonOtfTableGCID142 { get; private set; }
    public bool IsOpenTypePostScript { get; private set; }
    public string? OtfRegistry { get; private set; }
    public string? OtfOrdering { get; private set; }
    public int OtfSupplement { get; private set; }

    public string? GetError() => Error;
    public string? GetName() => Name;
    public int? GetHeaderMacStyle() => HeaderMacStyle;
    public OS2WindowsMetricsTable? GetOS2Windows() => OS2Windows;
    public string? GetFontFamily() => FontFamily;
    public string? GetFontSubFamily() => FontSubFamily;
    public bool IsOTFAndPostScript() => IsOpenTypePostScript;
    public byte[]? GetNonOtfTableGCID142() => NonOtfTableGCID142;
    public string? GetOtfRegistry() => OtfRegistry;
    public string? GetOtfOrdering() => OtfOrdering;
    public int GetOtfSupplement() => OtfSupplement;

    public void SetError(string exception) => Error = exception;
    internal void SetName(string? name) => Name = name;
    internal void SetHeaderMacStyle(int? headerMacStyle) => HeaderMacStyle = headerMacStyle;
    internal void SetOs2Windows(OS2WindowsMetricsTable? os2Windows) => OS2Windows = os2Windows;
    internal void SetFontFamily(string? fontFamily, string? fontSubFamily)
    {
        FontFamily = fontFamily;
        FontSubFamily = fontSubFamily;
    }

    internal void SetNonOtfGcid142(byte[] nonOtfGcid142) => NonOtfTableGCID142 = nonOtfGcid142;
    internal void SetIsOTFAndPostScript(bool isOTFAndPostScript) => IsOpenTypePostScript = isOTFAndPostScript;

    public void SetOtfROS(string? otfRegistry, string? otfOrdering, int otfSupplement)
    {
        OtfRegistry = otfRegistry;
        OtfOrdering = otfOrdering;
        OtfSupplement = otfSupplement;
    }
}
