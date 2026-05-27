/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDDefaultAppearanceString.java
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDDefaultAppearanceString
{
    private readonly PDResources _defaultResources;

    public COSName? FontName { get; private set; }
    public PDFont? Font { get; private set; }
    public float FontSize { get; private set; } = 12f;
    public PDColor? FontColor { get; private set; }

    internal PDDefaultAppearanceString(COSString? defaultAppearance, PDResources? defaultResources)
    {
        if (defaultAppearance == null)
        {
            throw new ArgumentException("Default appearance string is required.", nameof(defaultAppearance));
        }

        _defaultResources = defaultResources ?? throw new ArgumentException("Default resources dictionary is required for font resolution.", nameof(defaultResources));
        ProcessAppearanceStringOperators(defaultAppearance.GetBytes());
    }

    private void ProcessAppearanceStringOperators(byte[] content)
    {
        using MemoryStream stream = new(content, writable: false);
        IList<object> tokens = PDFStreamParser.Parse(stream);
        List<COSBase> arguments = [];

        foreach (object token in tokens)
        {
            if (token is Operator op)
            {
                ProcessOperator(op, arguments);
                arguments.Clear();
            }
            else if (token is COSBase baseToken)
            {
                arguments.Add(baseToken);
            }
        }
    }

    private void ProcessOperator(Operator op, List<COSBase> operands)
    {
        switch (op.GetName())
        {
            case OperatorName.SET_FONT_AND_SIZE:
                ProcessSetFont(operands);
                break;
            case OperatorName.NON_STROKING_GRAY:
            case OperatorName.NON_STROKING_RGB:
            case OperatorName.NON_STROKING_CMYK:
                ProcessSetFontColor(operands);
                break;
        }
    }

    private void ProcessSetFont(List<COSBase> operands)
    {
        if (operands.Count < 2)
        {
            throw new IOException("Missing operands for set font operator.");
        }

        if (operands[0] is not COSName fontName || operands[1] is not COSNumber fontSize)
        {
            return;
        }

        PDFont? font = _defaultResources.GetFont(fontName);
        if (font == null)
        {
            throw new IOException($"Could not find font: /{fontName.GetName()}");
        }

        FontName = fontName;
        Font = font;
        FontSize = fontSize.FloatValue();
    }

    private void ProcessSetFontColor(List<COSBase> operands)
    {
        PDColorSpace colorSpace = operands.Count switch
        {
            1 => PDDeviceGray.Instance,
            3 => PDDeviceRGB.Instance,
            4 => PDDeviceCMYK.Instance,
            _ => throw new IOException("Invalid operands for non stroking color operator.")
        };

        COSArray array = new();
        array.AddAll(operands);
        FontColor = new PDColor(array, colorSpace);
    }
}
