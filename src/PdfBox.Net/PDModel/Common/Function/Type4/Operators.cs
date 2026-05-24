/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/Operators.java
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

public class Operators
{
    private readonly Dictionary<string, Operator> _operators = new(StringComparer.Ordinal)
    {
        ["add"] = new ArithmeticOperators.Add(),
        ["abs"] = new ArithmeticOperators.Abs(),
        ["atan"] = new ArithmeticOperators.Atan(),
        ["ceiling"] = new ArithmeticOperators.Ceiling(),
        ["cos"] = new ArithmeticOperators.Cos(),
        ["cvi"] = new ArithmeticOperators.Cvi(),
        ["cvr"] = new ArithmeticOperators.Cvr(),
        ["div"] = new ArithmeticOperators.Div(),
        ["exp"] = new ArithmeticOperators.Exp(),
        ["floor"] = new ArithmeticOperators.Floor(),
        ["idiv"] = new ArithmeticOperators.IDiv(),
        ["ln"] = new ArithmeticOperators.Ln(),
        ["log"] = new ArithmeticOperators.Log(),
        ["mod"] = new ArithmeticOperators.Mod(),
        ["mul"] = new ArithmeticOperators.Mul(),
        ["neg"] = new ArithmeticOperators.Neg(),
        ["round"] = new ArithmeticOperators.Round(),
        ["sin"] = new ArithmeticOperators.Sin(),
        ["sqrt"] = new ArithmeticOperators.Sqrt(),
        ["sub"] = new ArithmeticOperators.Sub(),
        ["truncate"] = new ArithmeticOperators.Truncate(),
        ["and"] = new BitwiseOperators.And(),
        ["bitshift"] = new BitwiseOperators.Bitshift(),
        ["eq"] = new RelationalOperators.Eq(),
        ["false"] = new BitwiseOperators.False(),
        ["ge"] = new RelationalOperators.Ge(),
        ["gt"] = new RelationalOperators.Gt(),
        ["le"] = new RelationalOperators.Le(),
        ["lt"] = new RelationalOperators.Lt(),
        ["ne"] = new RelationalOperators.Ne(),
        ["not"] = new BitwiseOperators.Not(),
        ["or"] = new BitwiseOperators.Or(),
        ["true"] = new BitwiseOperators.True(),
        ["xor"] = new BitwiseOperators.Xor(),
        ["if"] = new ConditionalOperators.If(),
        ["ifelse"] = new ConditionalOperators.IfElse(),
        ["copy"] = new StackOperators.Copy(),
        ["dup"] = new StackOperators.Dup(),
        ["exch"] = new StackOperators.Exch(),
        ["index"] = new StackOperators.Index(),
        ["pop"] = new StackOperators.Pop(),
        ["roll"] = new StackOperators.Roll()
    };

    public Operator? GetOperator(string operatorName)
    {
        _operators.TryGetValue(operatorName, out Operator? value);
        return value;
    }
}
