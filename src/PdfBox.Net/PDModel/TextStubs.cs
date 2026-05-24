/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Stub implementations for PDModel types required by the text extraction package
 * that have not yet been fully ported.
 *
 * PORT_MODE: adapted
 *
 * NOTE: PDGraphicsState, PDTextState, PDMarkedContent, and PDXObject have been
 * promoted to their own files in the canonical namespace locations.
 * Only unported stubs remain here.
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

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline
{
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
