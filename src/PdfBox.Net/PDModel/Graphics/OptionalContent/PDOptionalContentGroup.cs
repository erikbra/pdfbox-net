/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/optionalcontent/PDOptionalContentGroup.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.PDModel.Graphics.OptionalContent;

public class PDOptionalContentGroup : PDPropertyList
{
    public enum RenderState
    {
        ON,
        OFF
    }

    public PDOptionalContentGroup(string name)
        : base()
    {
        Dict.SetItem(COSName.TYPE, COSName.GetPDFName("OCG"));
        SetName(name);
    }

    public PDOptionalContentGroup(COSDictionary dict)
        : base(dict)
    {
        if (!Equals(dict.GetDictionaryObject(COSName.TYPE), COSName.GetPDFName("OCG")))
        {
            throw new ArgumentException("Provided dictionary is not of type 'OCG'", nameof(dict));
        }
    }

    public string? GetName() => Dict.GetString(COSName.NAME);

    public void SetName(string name) => Dict.SetString(COSName.NAME, name);

    public RenderState? GetRenderState(RenderDestination destination)
    {
        COSDictionary? usage = Dict.GetCOSDictionary(COSName.GetPDFName("Usage"));
        COSName? state = null;
        if (usage is not null)
        {
            if (destination == RenderDestination.PRINT)
            {
                state = usage.GetCOSDictionary(COSName.GetPDFName("Print"))?.GetCOSName(COSName.GetPDFName("PrintState"));
            }
            else if (destination == RenderDestination.VIEW)
            {
                state = usage.GetCOSDictionary(COSName.GetPDFName("View"))?.GetCOSName(COSName.GetPDFName("ViewState"));
            }

            state ??= usage.GetCOSDictionary(COSName.GetPDFName("Export"))?.GetCOSName(COSName.GetPDFName("ExportState"));
        }

        return state?.GetName() switch
        {
            "ON" => RenderState.ON,
            "OFF" => RenderState.OFF,
            _ => null
        };
    }

    public override string ToString() => $"{base.ToString()} ({GetName()})";
}
