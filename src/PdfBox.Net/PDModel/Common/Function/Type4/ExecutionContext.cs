/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/ExecutionContext.java
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

namespace PdfBox.Net.PDModel.Common.Function.Type4;

public class ExecutionContext
{
    private readonly Operators _operators;
    private readonly ExecutionStack _stack = new();

    public ExecutionContext(Operators operatorSet)
    {
        _operators = operatorSet;
    }

    public ExecutionStack GetStack() => _stack;

    public Operators GetOperators() => _operators;

    public IConvertible PopNumber() => (IConvertible)_stack.Pop();

    public int PopInt() => Convert.ToInt32(_stack.Pop(), System.Globalization.CultureInfo.InvariantCulture);

    public float PopReal() => Convert.ToSingle(_stack.Pop(), System.Globalization.CultureInfo.InvariantCulture);
}

public sealed class ExecutionStack : List<object>
{
    public void Push(object value) => Add(value);

    public object Pop()
    {
        object value = this[^1];
        RemoveAt(Count - 1);
        return value;
    }

    public object Peek() => this[^1];
}
