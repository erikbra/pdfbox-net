/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/InstructionSequence.java
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

public class InstructionSequence
{
    private readonly List<object> _instructions = [];

    public void AddName(string name) => _instructions.Add(name);

    public void AddInteger(int value) => _instructions.Add(value);

    public void AddReal(float value) => _instructions.Add(value);

    public void AddBoolean(bool value) => _instructions.Add(value);

    public void AddProc(InstructionSequence child) => _instructions.Add(child);

    public void Execute(ExecutionContext context)
    {
        ExecutionStack stack = context.GetStack();
        foreach (object instruction in _instructions)
        {
            if (instruction is string name)
            {
                Operator? cmd = context.GetOperators().GetOperator(name);
                if (cmd is null)
                {
                    throw new NotSupportedException($"Unknown operator or name: {name}");
                }

                cmd.Execute(context);
            }
            else
            {
                stack.Push(instruction);
            }
        }

        while (stack.Count > 0 && stack.Peek() is InstructionSequence nested)
        {
            stack.Pop();
            nested.Execute(context);
        }
    }
}
