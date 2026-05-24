/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/InstructionSequenceBuilder.java
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

using System.Globalization;
using System.Text.RegularExpressions;

namespace PdfBox.Net.PDModel.Common.Function.Type4;

public sealed class InstructionSequenceBuilder : Parser.AbstractSyntaxHandler
{
    private static readonly Regex MatchesInteger = new(@"^[\+\-]?\d+$", RegexOptions.Compiled);
    private static readonly Regex MatchesReal = new(@"^\-?\d*\.\d*([Ee]\-?\d+)?$", RegexOptions.Compiled);

    private readonly InstructionSequence _mainSequence = new();
    private readonly Stack<InstructionSequence> _seqStack = new();

    private InstructionSequenceBuilder()
    {
        _seqStack.Push(_mainSequence);
    }

    public InstructionSequence GetInstructionSequence() => _mainSequence;

    public static InstructionSequence Parse(string text)
    {
        InstructionSequenceBuilder builder = new();
        Parser.Parse(new Parser.CharSequence(text), builder);
        return builder.GetInstructionSequence();
    }

    public override void Token(Parser.CharSequence text)
    {
        string token = text.ToString();
        if (token == "{")
        {
            InstructionSequence child = new();
            _seqStack.Peek().AddProc(child);
            _seqStack.Push(child);
        }
        else if (token == "}")
        {
            _seqStack.Pop();
        }
        else if (MatchesInteger.IsMatch(token))
        {
            _seqStack.Peek().AddInteger(ParseInt(token));
        }
        else if (MatchesReal.IsMatch(token))
        {
            _seqStack.Peek().AddReal(ParseReal(token));
        }
        else
        {
            _seqStack.Peek().AddName(token);
        }
    }

    public static int ParseInt(string token) => int.Parse(token, CultureInfo.InvariantCulture);

    public static float ParseReal(string token) => float.Parse(token, CultureInfo.InvariantCulture);
}
