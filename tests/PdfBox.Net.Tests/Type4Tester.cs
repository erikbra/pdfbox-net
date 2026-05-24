/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/function/type4/Type4Tester.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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
using PdfBox.Net.PDModel.Common.Function.Type4;

namespace PdfBox.Net.Tests;

internal sealed class Type4Tester
{
    private readonly PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext _context;

    private Type4Tester(PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext context)
    {
        _context = context;
    }

    public static Type4Tester Create(string text)
    {
        InstructionSequence instructions = InstructionSequenceBuilder.Parse(text);
        PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext context = new(new Operators());
        instructions.Execute(context);
        return new Type4Tester(context);
    }

    public Type4Tester Pop(bool expected)
    {
        Assert.Equal(expected, (bool)_context.GetStack().Pop());
        return this;
    }

    public Type4Tester PopReal(float expected, double delta = 0.0000001)
    {
        Assert.Equal(expected, Convert.ToSingle(_context.GetStack().Pop()), delta);
        return this;
    }

    public Type4Tester Pop(int expected)
    {
        Assert.Equal(expected, Convert.ToInt32(_context.GetStack().Pop()));
        return this;
    }

    public Type4Tester Pop(float expected, double delta = 0.0000001)
    {
        Assert.Equal(expected, Convert.ToDouble(_context.GetStack().Pop()), delta);
        return this;
    }

    public Type4Tester IsEmpty()
    {
        Assert.Empty(_context.GetStack());
        return this;
    }

    public PdfBox.Net.PDModel.Common.Function.Type4.ExecutionContext ToExecutionContext() => _context;
}
