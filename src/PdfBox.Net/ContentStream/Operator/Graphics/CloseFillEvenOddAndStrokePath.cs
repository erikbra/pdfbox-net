/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CloseFillEvenOddAndStrokePath.java
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

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CloseFillEvenOddAndStrokePath : OperatorProcessor
{
    private readonly ClosePath _closePath;
    private readonly FillEvenOddAndStrokePath _fillEvenOddAndStrokePath;

    public CloseFillEvenOddAndStrokePath(PDFStreamEngine context)
        : base(OperatorName.CLOSE_FILL_EVEN_ODD_AND_STROKE, context)
    {
        _closePath = new ClosePath(context);
        _fillEvenOddAndStrokePath = new FillEvenOddAndStrokePath(context);
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        _closePath.Process(op, operands);
        _fillEvenOddAndStrokePath.Process(op, operands);
    }
}
