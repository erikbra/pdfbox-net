/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/ArithmeticOperators.java
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

internal static class ArithmeticOperators
{
    internal sealed class Abs : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); c.GetStack().Push(n is int i ? Math.Abs(i) : Math.Abs(Convert.ToSingle(n))); } }
    internal sealed class Add : Operator { public void Execute(ExecutionContext c) { var n2 = c.PopNumber(); var n1 = c.PopNumber(); if (n1 is int && n2 is int) { long sum = n1.ToInt64(null) + n2.ToInt64(null); c.GetStack().Push(sum < int.MinValue || sum > int.MaxValue ? (float)sum : (int)sum); } else { c.GetStack().Push(n1.ToSingle(null) + n2.ToSingle(null)); } } }
    internal sealed class Atan : Operator { public void Execute(ExecutionContext c) { float den = c.PopReal(); float num = c.PopReal(); float atan = (float)Math.Atan2(num, den); atan = (float)(atan * 180 / Math.PI) % 360; if (atan < 0) atan += 360; c.GetStack().Push(atan); } }
    internal sealed class Ceiling : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); c.GetStack().Push(n is int ? n : (float)Math.Ceiling(Convert.ToDouble(n))); } }
    internal sealed class Cos : Operator { public void Execute(ExecutionContext c) { float angle = c.PopReal(); c.GetStack().Push((float)Math.Cos(angle * Math.PI / 180)); } }
    internal sealed class Cvi : Operator { public void Execute(ExecutionContext c) { c.GetStack().Push(c.PopNumber().ToInt32(null)); } }
    internal sealed class Cvr : Operator { public void Execute(ExecutionContext c) { c.GetStack().Push(c.PopNumber().ToSingle(null)); } }
    internal sealed class Div : Operator { public void Execute(ExecutionContext c) { float n2 = c.PopReal(); float n1 = c.PopReal(); c.GetStack().Push(n1 / n2); } }
    internal sealed class Exp : Operator { public void Execute(ExecutionContext c) { var exp = c.PopNumber(); var @base = c.PopNumber(); c.GetStack().Push((float)Math.Pow(@base.ToDouble(null), exp.ToDouble(null))); } }
    internal sealed class Floor : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); c.GetStack().Push(n is int ? n : (float)Math.Floor(Convert.ToDouble(n))); } }
    internal sealed class IDiv : Operator { public void Execute(ExecutionContext c) { int n2 = c.PopInt(); int n1 = c.PopInt(); c.GetStack().Push(n1 / n2); } }
    internal sealed class Ln : Operator { public void Execute(ExecutionContext c) { c.GetStack().Push((float)Math.Log(c.PopNumber().ToDouble(null))); } }
    internal sealed class Log : Operator { public void Execute(ExecutionContext c) { c.GetStack().Push((float)Math.Log10(c.PopNumber().ToDouble(null))); } }
    internal sealed class Mod : Operator { public void Execute(ExecutionContext c) { int n2 = c.PopInt(); int n1 = c.PopInt(); c.GetStack().Push(n1 % n2); } }
    internal sealed class Mul : Operator { public void Execute(ExecutionContext c) { var n2 = c.PopNumber(); var n1 = c.PopNumber(); if (n1 is int && n2 is int) { long result = n1.ToInt64(null) * n2.ToInt64(null); c.GetStack().Push(result >= int.MinValue && result <= int.MaxValue ? (int)result : (float)result); } else { c.GetStack().Push((float)(n1.ToDouble(null) * n2.ToDouble(null))); } } }
    internal sealed class Neg : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); if (n is int i) c.GetStack().Push(i == int.MinValue ? -(float)i : -i); else c.GetStack().Push(-Convert.ToSingle(n)); } }
    internal sealed class Round : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); c.GetStack().Push(n is int ? n : (float)Math.Round(Convert.ToDouble(n))); } }
    internal sealed class Sin : Operator { public void Execute(ExecutionContext c) { float angle = c.PopReal(); c.GetStack().Push((float)Math.Sin(angle * Math.PI / 180)); } }
    internal sealed class Sqrt : Operator { public void Execute(ExecutionContext c) { float n = c.PopReal(); if (n < 0) throw new ArgumentException("argument must be nonnegative"); c.GetStack().Push((float)Math.Sqrt(n)); } }
    internal sealed class Sub : Operator { public void Execute(ExecutionContext c) { var n2 = c.PopNumber(); var n1 = c.PopNumber(); if (n1 is int && n2 is int) { long result = n1.ToInt64(null) - n2.ToInt64(null); c.GetStack().Push(result < int.MinValue || result > int.MaxValue ? (float)result : (int)result); } else { c.GetStack().Push(n1.ToSingle(null) - n2.ToSingle(null)); } } }
    internal sealed class Truncate : Operator { public void Execute(ExecutionContext c) { object n = c.GetStack().Pop(); c.GetStack().Push(n is int ? n : (float)(int)Convert.ToSingle(n)); } }
}
