/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Stub implementations for PDModel types required by the text extraction package.
 * These are adapted stubs pending full porting of the corresponding PDFBox model layers.
 *
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Text;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.State
{
    public class PDGraphicsState
    {
        private readonly Matrix _currentTransformationMatrix = new();
        private readonly PDTextState _textState = new();

        public Matrix GetCurrentTransformationMatrix() => _currentTransformationMatrix;
        public PDTextState GetTextState() => _textState;
    }

    public class PDTextState
    {
        public float GetFontSize() => 0f;
        public float GetHorizontalScaling() => 100f;
    }
}

namespace PdfBox.Net.PDModel.DocumentInterchange.MarkedContent
{
    using PdfBox.Net.PDModel.Graphics;

    public class PDMarkedContent
    {
        private readonly List<PDMarkedContent> _markedContents = new();
        private readonly List<TextPosition> _texts = new();
        private readonly List<PDXObject> _xobjects = new();

        private PDMarkedContent(COSName tag, COSDictionary? properties)
        {
            Tag = tag;
            Properties = properties;
        }

        public COSName Tag { get; }
        public COSDictionary? Properties { get; }

        public static PDMarkedContent Create(COSName tag, COSDictionary? properties) => new(tag, properties);
        public void AddMarkedContent(PDMarkedContent markedContent) => _markedContents.Add(markedContent);
        public void AddText(TextPosition text) => _texts.Add(text);
        public void AddXObject(PDXObject xobject) => _xobjects.Add(xobject);
        public string? GetActualText() => Properties?.GetString(COSName.GetPDFName("ActualText"));
    }
}

namespace PdfBox.Net.PDModel.Graphics
{
    public class PDXObject
    {
    }
}

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline
{
    using PdfBox.Net.COS;

    public class PDOutlineItem
    {
        public virtual PDPage? FindDestinationPage(PDDocument document) => null;
        public virtual COSBase? GetCOSObject() => null;
    }
}

namespace PdfBox.Net.PDModel.Interactive.PageNavigation
{
    public class PDThreadBead
    {
        public virtual PDRectangle? GetRectangle() => null;
    }
}
