/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/StackOperators.java
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
namespace PdfBox.Net.PDModel.Common.Function.Type4;

internal static class StackOperators
{
    internal sealed class Copy : Operator
    {
        public void Execute(ExecutionContext context)
        {
            int n = Convert.ToInt32(context.GetStack().Pop());
            if (n > 0)
            {
                int size = context.GetStack().Count;
                List<object> copy = [.. context.GetStack().GetRange(size - n, n)];
                context.GetStack().AddRange(copy);
            }
        }
    }

    internal sealed class Dup : Operator { public void Execute(ExecutionContext context) => context.GetStack().Push(context.GetStack().Peek()); }
    internal sealed class Exch : Operator { public void Execute(ExecutionContext context) { object any2 = context.GetStack().Pop(); object any1 = context.GetStack().Pop(); context.GetStack().Push(any2); context.GetStack().Push(any1); } }
    internal sealed class Index : Operator { public void Execute(ExecutionContext context) { int n = Convert.ToInt32(context.GetStack().Pop()); if (n < 0) throw new ArgumentException($"rangecheck: {n}"); int size = context.GetStack().Count; context.GetStack().Push(context.GetStack()[size - n - 1]); } }
    internal sealed class Pop : Operator { public void Execute(ExecutionContext context) => context.GetStack().Pop(); }
    internal sealed class Roll : Operator
    {
        public void Execute(ExecutionContext context)
        {
            int j = Convert.ToInt32(context.GetStack().Pop());
            int n = Convert.ToInt32(context.GetStack().Pop());
            if (j == 0) return;
            if (n < 0) throw new ArgumentException($"rangecheck: {n}");
            j %= n;
            if (j < 0) j += n;
            int start = context.GetStack().Count - n;
            List<object> segment = context.GetStack().GetRange(start, n);
            context.GetStack().RemoveRange(start, n);
            List<object> rolled = [.. segment.Skip(n - j), .. segment.Take(n - j)];
            context.GetStack().AddRange(rolled);
        }
    }
}
