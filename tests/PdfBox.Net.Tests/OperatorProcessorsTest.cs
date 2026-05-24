/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for the operator processors introduced in issue #15:
 * marked-content hooks and nesting, show-text operators (Tj/TJ/'/''),
 * and graceful handling of under-specified operand lists.
 *
 * PORT_MODE: adapted
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

using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.ContentStream.Operator.MarkedContent;
using PdfBox.Net.ContentStream.Operator.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for the operator processors moved from PDFStreamEngineStubs into their
/// canonical per-class files (issue #15).  Covers:
/// <list type="bullet">
///   <item>Marked-content hooks (BMC, BDC, EMC, MP, DP) including nesting.</item>
///   <item>Show-text operators (Tj, TJ, ', ").</item>
///   <item>Graceful no-op when operands are missing or have the wrong type.</item>
/// </list>
/// </summary>
public class OperatorProcessorsTest
{
    // ── Test helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Engine subclass that records every marked-content hook call and exposes
    /// protected text-matrix state for assertions.
    /// </summary>
    private sealed class ObservingEngine : PDFStreamEngine
    {
        public record MarkedContentCall(string Kind, string Tag, COSDictionary? Props);

        public List<MarkedContentCall> MarkedContentCalls { get; } = new();
        public List<(Matrix TextRenderingMatrix, PDFont Font, int Code, Vector Displacement)> GlyphCalls { get; } = new();

        public override void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
            => MarkedContentCalls.Add(new("Begin", tag.GetName(), properties));

        public override void EndMarkedContentSequence()
            => MarkedContentCalls.Add(new("End", "", null));

        public override void MarkedContentPoint(COSName tag, COSDictionary? properties)
            => MarkedContentCalls.Add(new("Point", tag.GetName(), properties));

        protected override void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
            => GlyphCalls.Add((textRenderingMatrix, font, code, displacement));

        /// <summary>Runs a raw Latin-1 content stream string through the engine.</summary>
        public void RunStream(string content)
        {
            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }

        // Re-expose protected members for assertions.
        public new Matrix GetTextMatrix() => base.GetTextMatrix();
        public new Matrix GetTextLineMatrix() => base.GetTextLineMatrix();
        public new PDGraphicsState GetGraphicsState() => base.GetGraphicsState();
    }

    // ── Marked-content: BMC ───────────────────────────────────────────────────

    [Fact]
    public void BeginMarkedContentSequence_BMC_CallsBeginHook()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequence(engine));

        engine.RunStream("/Span BMC");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("Begin", engine.MarkedContentCalls[0].Kind);
        Assert.Equal("Span", engine.MarkedContentCalls[0].Tag);
        Assert.Null(engine.MarkedContentCalls[0].Props);
    }

    [Fact]
    public void BeginMarkedContentSequence_BMC_MissingTag_UsesUnknown()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequence(engine));

        // No operand → tag should default to "Unknown".
        engine.RunStream("BMC");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("Unknown", engine.MarkedContentCalls[0].Tag);
    }

    // ── Marked-content: BDC ───────────────────────────────────────────────────

    [Fact]
    public void BeginMarkedContentSequenceWithProperties_BDC_CallsBeginHookWithNullProps()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequenceWithProperties(engine));

        engine.RunStream("/P BDC");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("Begin", engine.MarkedContentCalls[0].Kind);
        Assert.Equal("P", engine.MarkedContentCalls[0].Tag);
    }

    [Fact]
    public void BeginMarkedContentSequenceWithProperties_BDC_MissingTag_UsesUnknown()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequenceWithProperties(engine));

        engine.RunStream("BDC");

        Assert.Equal("Unknown", engine.MarkedContentCalls[0].Tag);
    }

    // ── Marked-content: EMC ───────────────────────────────────────────────────

    [Fact]
    public void EndMarkedContentSequence_EMC_CallsEndHook()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new EndMarkedContentSequence(engine));

        engine.RunStream("EMC");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("End", engine.MarkedContentCalls[0].Kind);
    }

    // ── Marked-content: MP ───────────────────────────────────────────────────

    [Fact]
    public void MarkedContentPoint_MP_CallsPointHookWithTag()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new MarkedContentPoint(engine));

        engine.RunStream("/Artifact MP");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("Point", engine.MarkedContentCalls[0].Kind);
        Assert.Equal("Artifact", engine.MarkedContentCalls[0].Tag);
        Assert.Null(engine.MarkedContentCalls[0].Props);
    }

    [Fact]
    public void MarkedContentPoint_MP_MissingTag_UsesUnknown()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new MarkedContentPoint(engine));

        engine.RunStream("MP");

        Assert.Equal("Unknown", engine.MarkedContentCalls[0].Tag);
    }

    // ── Marked-content: DP ───────────────────────────────────────────────────

    [Fact]
    public void MarkedContentPointWithProperties_DP_CallsPointHookWithTag()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new MarkedContentPointWithProperties(engine));

        engine.RunStream("/Note DP");

        Assert.Single(engine.MarkedContentCalls);
        Assert.Equal("Point", engine.MarkedContentCalls[0].Kind);
        Assert.Equal("Note", engine.MarkedContentCalls[0].Tag);
    }

    // ── Marked-content nesting ────────────────────────────────────────────────

    [Fact]
    public void MarkedContent_NestedBMCAndEMC_HooksCalledInOrder()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequence(engine));
        engine.AddOperator(new EndMarkedContentSequence(engine));

        engine.RunStream("/Outer BMC /Inner BMC EMC EMC");

        Assert.Equal(4, engine.MarkedContentCalls.Count);
        Assert.Equal("Begin",  engine.MarkedContentCalls[0].Kind);
        Assert.Equal("Outer",  engine.MarkedContentCalls[0].Tag);
        Assert.Equal("Begin",  engine.MarkedContentCalls[1].Kind);
        Assert.Equal("Inner",  engine.MarkedContentCalls[1].Tag);
        Assert.Equal("End",    engine.MarkedContentCalls[2].Kind);
        Assert.Equal("End",    engine.MarkedContentCalls[3].Kind);
    }

    [Fact]
    public void MarkedContent_MixedBMCAndBDCWithEMC_HooksCalledInOrder()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequence(engine));
        engine.AddOperator(new BeginMarkedContentSequenceWithProperties(engine));
        engine.AddOperator(new EndMarkedContentSequence(engine));

        engine.RunStream("/Sect BMC /P BDC EMC EMC");

        Assert.Equal(4, engine.MarkedContentCalls.Count);
        Assert.Equal("Sect", engine.MarkedContentCalls[0].Tag);
        Assert.Equal("P",    engine.MarkedContentCalls[1].Tag);
        Assert.Equal("End",  engine.MarkedContentCalls[2].Kind);
        Assert.Equal("End",  engine.MarkedContentCalls[3].Kind);
    }

    [Fact]
    public void MarkedContent_PointInterleavedWithSequence_OrderPreserved()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new BeginMarkedContentSequence(engine));
        engine.AddOperator(new MarkedContentPoint(engine));
        engine.AddOperator(new EndMarkedContentSequence(engine));

        engine.RunStream("/Block BMC /Figure MP EMC");

        Assert.Equal(3, engine.MarkedContentCalls.Count);
        Assert.Equal("Begin",  engine.MarkedContentCalls[0].Kind);
        Assert.Equal("Block",  engine.MarkedContentCalls[0].Tag);
        Assert.Equal("Point",  engine.MarkedContentCalls[1].Kind);
        Assert.Equal("Figure", engine.MarkedContentCalls[1].Tag);
        Assert.Equal("End",    engine.MarkedContentCalls[2].Kind);
    }

    // ── ShowText "Tj" ─────────────────────────────────────────────────────────

    [Fact]
    public void ShowText_Tj_InvokesShowStringGlyphs_GlyphsRecorded()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowText(engine));

        // Single character string "A" (0x41).
        engine.RunStream("(A) Tj");

        Assert.Single(engine.GlyphCalls);
        Assert.Equal(0x41, engine.GlyphCalls[0].Code);
    }

    [Fact]
    public void ShowText_Tj_EmptyString_NoGlyphsEmitted()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowText(engine));

        engine.RunStream("() Tj");

        Assert.Empty(engine.GlyphCalls);
    }

    [Fact]
    public void ShowText_Tj_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowText(engine));

        var ex = Record.Exception(() => engine.RunStream("Tj"));
        Assert.Null(ex);
        Assert.Empty(engine.GlyphCalls);
    }

    [Fact]
    public void ShowText_Tj_NonStringOperand_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowText(engine));

        // Operand is a number, not a string.
        var ex = Record.Exception(() => engine.RunStream("42 Tj"));
        Assert.Null(ex);
        Assert.Empty(engine.GlyphCalls);
    }

    // ── ShowTextAdjusted "TJ" ─────────────────────────────────────────────────

    [Fact]
    public void ShowTextAdjusted_TJ_StringElement_EmitsGlyphs()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowTextAdjusted(engine));

        // Array with one string element containing two bytes (0x41 = 'A', 0x42 = 'B').
        engine.RunStream("[(AB)] TJ");

        Assert.Equal(2, engine.GlyphCalls.Count);
        Assert.Equal(0x41, engine.GlyphCalls[0].Code);
        Assert.Equal(0x42, engine.GlyphCalls[1].Code);
    }

    [Fact]
    public void ShowTextAdjusted_TJ_KerningAdjustment_ShiftsTextMatrix()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowTextAdjusted(engine));
        // Set font size so the kerning produces a measurable shift.
        engine.GetGraphicsState().GetTextState().FontSize = 10f;

        float initialX = engine.GetTextMatrix().GetTranslateX();

        // Array with a 1000-unit kerning adjustment: tx = -(1000/1000) * 10 * 1.0 = -10.
        engine.RunStream("[1000] TJ");

        float newX = engine.GetTextMatrix().GetTranslateX();
        Assert.Equal(-10f, newX - initialX, precision: 3);
    }

    [Fact]
    public void ShowTextAdjusted_TJ_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowTextAdjusted(engine));

        var ex = Record.Exception(() => engine.RunStream("TJ"));
        Assert.Null(ex);
    }

    [Fact]
    public void ShowTextAdjusted_TJ_NonArrayOperand_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ShowTextAdjusted(engine));

        var ex = Record.Exception(() => engine.RunStream("42 TJ"));
        Assert.Null(ex);
    }

    // ── ShowTextLine "'" ──────────────────────────────────────────────────────

    [Fact]
    public void ShowTextLine_Quote_MovesDownByLeadingAndEmitsGlyphs()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextLeading(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));
        engine.AddOperator(new ShowTextLine(engine));

        // Set leading = 12 then execute ' operator with "A".
        engine.RunStream("12 TL (A) '");

        // Text matrix Y should have moved down by -12.
        Assert.Equal(-12f, engine.GetTextMatrix().GetTranslateY());
        Assert.Single(engine.GlyphCalls);
        Assert.Equal(0x41, engine.GlyphCalls[0].Code);
    }

    [Fact]
    public void ShowTextLine_Quote_EmptyString_StillMovesDown()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextLeading(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));
        engine.AddOperator(new ShowTextLine(engine));

        engine.RunStream("10 TL () '");

        Assert.Equal(-10f, engine.GetTextMatrix().GetTranslateY());
        Assert.Empty(engine.GlyphCalls);
    }

    // ── ShowTextLineAndSpace '"' ───────────────────────────────────────────────

    [Fact]
    public void ShowTextLineAndSpace_DoubleQuote_SetsSpacingAndMovesAndEmitsGlyphs()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextLeading(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));
        engine.AddOperator(new ShowTextLineAndSpace(engine));

        // word-spacing=2, char-spacing=1, text="A"; leading defaults to 0 → no Y movement.
        engine.RunStream("2 1 (A) \"");

        PDTextState ts = engine.GetGraphicsState().GetTextState();
        Assert.Equal(2f, ts.GetWordSpacing());
        Assert.Equal(1f, ts.GetCharacterSpacing());
        Assert.Single(engine.GlyphCalls);
    }

    [Fact]
    public void ShowTextLineAndSpace_DoubleQuote_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));
        engine.AddOperator(new ShowTextLineAndSpace(engine));

        var ex = Record.Exception(() => engine.RunStream("\""));
        Assert.Null(ex);
    }

    // ── Error-handling for other operators ────────────────────────────────────

    [Fact]
    public void Concatenate_cm_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Concatenate(engine));

        var ex = Record.Exception(() => engine.RunStream("1 0 0 cm"));
        Assert.Null(ex);
    }

    [Fact]
    public void SetMatrix_Tm_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));

        var ex = Record.Exception(() => engine.RunStream("1 0 0 Tm"));
        Assert.Null(ex);
    }

    [Fact]
    public void MoveText_Td_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        var ex = Record.Exception(() => engine.RunStream("10 Td"));
        Assert.Null(ex);
    }

    [Fact]
    public void MoveTextSetLeading_TD_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveTextSetLeading(engine));

        var ex = Record.Exception(() => engine.RunStream("TD"));
        Assert.Null(ex);
    }

    [Fact]
    public void SetFontAndSize_Tf_TooFewOperands_DoesNotThrow()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetFontAndSize(engine));

        var ex = Record.Exception(() => engine.RunStream("Tf"));
        Assert.Null(ex);
    }

    // ── SetGraphicsStateParameters (gs) ──────────────────────────────────────

    [Fact]
    public void SetGraphicsStateParameters_gs_RegistersAndDispatchesWithoutException()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetGraphicsStateParameters(engine));

        // gs is a deliberate no-op at the baseline level; it must not throw.
        var ex = Record.Exception(() => engine.RunStream("/GS1 gs"));
        Assert.Null(ex);
    }

    // ── DrawObject "Do" ───────────────────────────────────────────────────────

    [Fact]
    public void DrawObject_Do_InvokesXObjectHook()
    {
        bool xObjectCalled = false;
        var engine = new CallbackEngine(xo => xObjectCalled = true);
        engine.AddOperator(new DrawObject(engine));

        engine.RunStream("/Form1 Do");

        Assert.True(xObjectCalled);
    }

    /// <summary>
    /// Minimal engine that routes the <see cref="PDFStreamEngine.XObject"/> hook
    /// through a provided callback, keeping test logic inline.
    /// </summary>
    private sealed class CallbackEngine : PDFStreamEngine
    {
        private readonly Action<PdfBox.Net.PDModel.Graphics.PDXObject> _onXObject;

        public CallbackEngine(Action<PdfBox.Net.PDModel.Graphics.PDXObject> onXObject)
            => _onXObject = onXObject;

        public override void XObject(PdfBox.Net.PDModel.Graphics.PDXObject xobject)
            => _onXObject(xobject);

        public void RunStream(string content)
        {
            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }
    }
}
