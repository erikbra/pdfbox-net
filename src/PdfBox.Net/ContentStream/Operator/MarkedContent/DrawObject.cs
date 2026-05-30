/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/markedcontent/DrawObject.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
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
using PdfBox.Net.PDModel.Graphics;

namespace PdfBox.Net.ContentStream.Operator.MarkedContent;

public sealed class DrawObject : OperatorProcessor
{
    public DrawObject(PDFStreamEngine context)
        : base(OperatorName.DRAW_OBJECT, context)
    {
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count == 0 || operands[0] is not COSName name)
        {
            throw new MissingOperandException(op, operands);
        }

        PDXObject? xobject = Context.GetResources()?.GetXObject(name);
        if (xobject is null)
        {
            return;
        }

        Context.XObject(xobject);
    }
}
