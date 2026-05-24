/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/RelationalOperators.java
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

internal static class RelationalOperators
{
    internal class Eq : Operator
    {
        public virtual void Execute(ExecutionContext context)
        {
            object op2 = context.GetStack().Pop();
            object op1 = context.GetStack().Pop();
            context.GetStack().Push(IsEqual(op1, op2));
        }

        protected virtual bool IsEqual(object op1, object op2)
        {
            if (op1 is IConvertible num1 && op2 is IConvertible num2 && op1 is not bool && op2 is not bool)
            {
                return num1.ToSingle(null).Equals(num2.ToSingle(null));
            }

            return Equals(op1, op2);
        }
    }

    internal abstract class AbstractNumberComparisonOperator : Operator
    {
        public void Execute(ExecutionContext context)
        {
            float op2 = Convert.ToSingle(context.GetStack().Pop());
            float op1 = Convert.ToSingle(context.GetStack().Pop());
            context.GetStack().Push(Compare(op1, op2));
        }

        protected abstract bool Compare(float op1, float op2);
    }

    internal sealed class Ge : AbstractNumberComparisonOperator { protected override bool Compare(float a, float b) => a >= b; }
    internal sealed class Gt : AbstractNumberComparisonOperator { protected override bool Compare(float a, float b) => a > b; }
    internal sealed class Le : AbstractNumberComparisonOperator { protected override bool Compare(float a, float b) => a <= b; }
    internal sealed class Lt : AbstractNumberComparisonOperator { protected override bool Compare(float a, float b) => a < b; }
    internal sealed class Ne : Eq { protected override bool IsEqual(object op1, object op2) => !base.IsEqual(op1, op2); }
}
