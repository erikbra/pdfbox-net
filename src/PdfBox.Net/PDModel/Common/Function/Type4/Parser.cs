/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/Parser.java
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

public static class Parser
{
    public static void Parse(CharSequence input, ISyntaxHandler handler)
    {
        new Tokenizer(input, handler).Tokenize();
    }

    public interface ISyntaxHandler
    {
        void NewLine(CharSequence text);
        void Whitespace(CharSequence text);
        void Token(CharSequence text);
        void Comment(CharSequence text);
    }

    public abstract class AbstractSyntaxHandler : ISyntaxHandler
    {
        public virtual void Comment(CharSequence text) { }
        public virtual void NewLine(CharSequence text) { }
        public virtual void Whitespace(CharSequence text) { }
        public abstract void Token(CharSequence text);
    }

    public readonly record struct CharSequence(string Value)
    {
        public override string ToString() => Value;
    }

    private enum State
    {
        NewLine,
        Whitespace,
        Comment,
        Token
    }

    private sealed class Tokenizer(Parser.CharSequence input, ISyntaxHandler handler)
    {
        private const char Nul = '\u0000';
        private const char Eot = '\u0004';
        private const char Tab = '\u0009';
        private const char Ff = '\u000C';
        private const char Cr = '\r';
        private const char Lf = '\n';
        private const char Space = '\u0020';

        private readonly string _input = input.Value;
        private readonly ISyntaxHandler _handler = handler;
        private readonly System.Text.StringBuilder _buffer = new();
        private int _index;
        private State _state = State.Whitespace;

        public void Tokenize()
        {
            while (HasMore())
            {
                _buffer.Clear();
                NextState();
                switch (_state)
                {
                    case State.NewLine:
                        ScanNewLine();
                        break;
                    case State.Whitespace:
                        ScanWhitespace();
                        break;
                    case State.Comment:
                        ScanComment();
                        break;
                    default:
                        ScanToken();
                        break;
                }
            }
        }

        private bool HasMore() => _index < _input.Length;

        private char CurrentChar() => _input[_index];

        private char NextChar()
        {
            _index++;
            return HasMore() ? CurrentChar() : Eot;
        }

        private char Peek() => _index < _input.Length - 1 ? _input[_index + 1] : Eot;

        private void NextState()
        {
            char ch = CurrentChar();
            _state = ch switch
            {
                Cr or Lf or Ff => State.NewLine,
                Nul or Tab or Space => State.Whitespace,
                '%' => State.Comment,
                _ => State.Token
            };
        }

        private void ScanNewLine()
        {
            char ch = CurrentChar();
            _buffer.Append(ch);
            if (ch == Cr && Peek() == Lf)
            {
                _buffer.Append(NextChar());
            }
            _handler.NewLine(new CharSequence(_buffer.ToString()));
            NextChar();
        }

        private void ScanWhitespace()
        {
            _buffer.Append(CurrentChar());
            while (HasMore())
            {
                char ch = NextChar();
                if (ch is Nul or Tab or Space)
                {
                    _buffer.Append(ch);
                }
                else
                {
                    break;
                }
            }
            _handler.Whitespace(new CharSequence(_buffer.ToString()));
        }

        private void ScanComment()
        {
            _buffer.Append(CurrentChar());
            while (HasMore())
            {
                char ch = NextChar();
                if (ch is Cr or Lf or Ff)
                {
                    break;
                }
                _buffer.Append(ch);
            }
            _handler.Comment(new CharSequence(_buffer.ToString()));
        }

        private void ScanToken()
        {
            char ch = CurrentChar();
            _buffer.Append(ch);
            if (ch is '{' or '}')
            {
                _handler.Token(new CharSequence(_buffer.ToString()));
                NextChar();
                return;
            }

            while (HasMore())
            {
                ch = NextChar();
                if (ch is Nul or Tab or Space or Cr or Lf or Ff or Eot or '{' or '}')
                {
                    break;
                }
                _buffer.Append(ch);
            }
            _handler.Token(new CharSequence(_buffer.ToString()));
        }
    }
}
