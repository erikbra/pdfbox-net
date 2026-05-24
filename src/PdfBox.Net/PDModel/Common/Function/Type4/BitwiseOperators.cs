/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/BitwiseOperators.java
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

internal static class BitwiseOperators
{
    internal abstract class AbstractLogicalOperator : Operator
    {
        public void Execute(ExecutionContext context)
        {
            object op2 = context.GetStack().Pop();
            object op1 = context.GetStack().Pop();
            if (op1 is bool bool1 && op2 is bool bool2)
            {
                context.GetStack().Push(ApplyForBoolean(bool1, bool2));
            }
            else if (op1 is int int1 && op2 is int int2)
            {
                context.GetStack().Push(ApplyForInteger(int1, int2));
            }
            else
            {
                throw new InvalidCastException("Operands must be bool/bool or int/int");
            }
        }

        protected abstract bool ApplyForBoolean(bool bool1, bool bool2);
        protected abstract int ApplyForInteger(int int1, int int2);
    }

    internal sealed class And : AbstractLogicalOperator { protected override bool ApplyForBoolean(bool a, bool b) => a && b; protected override int ApplyForInteger(int a, int b) => a & b; }
    internal sealed class Bitshift : Operator { public void Execute(ExecutionContext c) { int shift = Convert.ToInt32(c.GetStack().Pop()); int value = Convert.ToInt32(c.GetStack().Pop()); c.GetStack().Push(shift < 0 ? value >> Math.Abs(shift) : value << shift); } }
    internal sealed class False : Operator { public void Execute(ExecutionContext c) => c.GetStack().Push(false); }
    internal sealed class Not : Operator { public void Execute(ExecutionContext c) { object op1 = c.GetStack().Pop(); if (op1 is bool b) c.GetStack().Push(!b); else if (op1 is int i) c.GetStack().Push(-i); else throw new InvalidCastException("Operand must be bool or int"); } }
    internal sealed class Or : AbstractLogicalOperator { protected override bool ApplyForBoolean(bool a, bool b) => a || b; protected override int ApplyForInteger(int a, int b) => a | b; }
    internal sealed class True : Operator { public void Execute(ExecutionContext c) => c.GetStack().Push(true); }
    internal sealed class Xor : AbstractLogicalOperator { protected override bool ApplyForBoolean(bool a, bool b) => a ^ b; protected override int ApplyForInteger(int a, int b) => a ^ b; }
}
