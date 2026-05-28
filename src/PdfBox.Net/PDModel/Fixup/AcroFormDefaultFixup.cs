/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/AcroFormDefaultFixup.java
 * PDFBOX_SOURCE_COMMIT: aa2d26fc40a6ffc20c77cae44081c9ef5b67daa6
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: aa2d26fc40a6ffc20c77cae44081c9ef5b67daa6
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

using PdfBox.Net.PDModel.Fixup.Processor;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.PDModel.Fixup;

public sealed class AcroFormDefaultFixup : AbstractFixup
{
    public AcroFormDefaultFixup(PDDocument document)
        : base(document)
    {
    }

    public override void Apply()
    {
        new AcroFormDefaultsProcessor(document).Process();

        PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm(null);
        if (acroForm is null || !acroForm.GetNeedAppearances())
        {
            return;
        }

        if (acroForm.GetFields().Count == 0)
        {
            new AcroFormOrphanWidgetsProcessor(document).Process();
        }

        new AcroFormGenerateAppearancesProcessor(document).Process();
    }
}
