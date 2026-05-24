/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for the content-stream execution core introduced in issue #14:
 * operator registration/dispatch, graphics-state stack transitions, text-matrix
 * management, and simple page/content-stream execution.
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for the content-stream execution core.
/// A minimal <see cref="TrackingEngine"/> subclass makes protected/internal state
/// observable without coupling the tests to implementation details.
/// </summary>
public class ContentStreamEngineTest
{
    // ── Test helper ────────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal PDFStreamEngine subclass that exposes protected state for assertions
    /// and records which operator names were dispatched.
    /// </summary>
    private sealed class TrackingEngine : PDFStreamEngine
    {
        public List<string> DispatchedOperators { get; } = new();
        public List<(PdfBox.Net.ContentStream.Operator.Operator Op, IList<COSBase> Operands)> DispatchedCalls { get; } = new();

        protected override void ProcessOperator(
            PdfBox.Net.ContentStream.Operator.Operator op, IList<COSBase> operands)
        {
            DispatchedOperators.Add(op.GetName());
            DispatchedCalls.Add((op, new List<COSBase>(operands)));
            base.ProcessOperator(op, operands);
        }

        /// <summary>Runs a raw UTF-8/Latin1 content stream string through the engine.</summary>
        public void RunStream(string content)
        {
            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }

        // Re-expose protected members so test methods can observe them.
        public new PDGraphicsState GetGraphicsState() => base.GetGraphicsState();
        public new Matrix GetTextMatrix() => base.GetTextMatrix();
        public new Matrix GetTextLineMatrix() => base.GetTextLineMatrix();
        public new int GraphicsStateStackDepth => base.GraphicsStateStackDepth;
    }

    // ── Operator registration / dispatch ──────────────────────────────────────

    [Fact]
    public void AddOperator_RegistersProcessor_DispatchedWhenNameMatches()
    {
        var engine = new TrackingEngine();
        int callCount = 0;
        engine.AddOperator(new LambdaProcessor("q", engine, (_, _) => callCount++));

        engine.RunStream("q");

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void ProcessStream_DispatchesAllOperators_InOrder()
    {
        var engine = new TrackingEngine();
        // Register save and restore so they dispatch without crashing.
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));

        engine.RunStream("q Q");

        Assert.Equal(new[] { "q", "Q" }, engine.DispatchedOperators);
    }

    [Fact]
    public void ProcessStream_UnknownOperator_IsIgnoredWithoutException()
    {
        var engine = new TrackingEngine();
        // No operators registered; unknown operator should not throw.
        engine.RunStream("unknownOp");
        Assert.Contains("unknownOp", engine.DispatchedOperators);
    }

    [Fact]
    public void AddOperator_LaterRegistration_ReplacesEarlier()
    {
        var engine = new TrackingEngine();
        int firstCount = 0;
        int secondCount = 0;
        engine.AddOperator(new LambdaProcessor("q", engine, (_, _) => firstCount++));
        engine.AddOperator(new LambdaProcessor("q", engine, (_, _) => secondCount++));

        engine.RunStream("q");

        Assert.Equal(0, firstCount);
        Assert.Equal(1, secondCount);
    }

    [Fact]
    public void ProcessStream_OperandsArePassedToProcessor()
    {
        var engine = new TrackingEngine();
        IList<COSBase>? capturedOperands = null;
        engine.AddOperator(new LambdaProcessor("Tf", engine, (_, ops) => capturedOperands = ops));

        engine.RunStream("/F1 12 Tf");

        Assert.NotNull(capturedOperands);
        Assert.Equal(2, capturedOperands!.Count);
        Assert.Equal("F1", Assert.IsType<COSName>(capturedOperands[0]).GetName());
        Assert.Equal(12, Assert.IsType<COSInteger>(capturedOperands[1]).IntValue());
    }

    // ── Graphics-state stack ──────────────────────────────────────────────────

    [Fact]
    public void Save_PushesGraphicsState_StackDepthIncreases()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));

        Assert.Equal(0, engine.GraphicsStateStackDepth);
        engine.RunStream("q");
        Assert.Equal(1, engine.GraphicsStateStackDepth);
    }

    [Fact]
    public void Restore_PopsGraphicsState_StackDepthDecreases()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));

        engine.RunStream("q Q");

        Assert.Equal(0, engine.GraphicsStateStackDepth);
    }

    [Fact]
    public void SaveRestore_NestedThreeLevels_StackReturnsToZero()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));

        engine.RunStream("q q q Q Q Q");

        Assert.Equal(0, engine.GraphicsStateStackDepth);
    }

    [Fact]
    public void Restore_WithoutSave_DoesNotThrow()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));

        var ex = Record.Exception(() => engine.RunStream("Q"));
        Assert.Null(ex);
        Assert.Equal(0, engine.GraphicsStateStackDepth);
    }

    [Fact]
    public void SaveRestore_CTMRestoredAfterConcatenate()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));
        engine.AddOperator(new ContentStream.Operator.State.Concatenate(engine));

        // Save, apply 2x scale, restore → CTM should be identity again.
        engine.RunStream("q 2 0 0 2 0 0 cm Q");

        Matrix ctm = engine.GetGraphicsState().GetCurrentTransformationMatrix();
        Assert.Equal(1f, ctm.GetScaleX());
        Assert.Equal(1f, ctm.GetScaleY());
    }

    [Fact]
    public void Save_CloneIsIndependentOfOriginal_MutationDoesNotAffectSaved()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Concatenate(engine));

        // After save+concatenate the active CTM should differ from the saved one.
        engine.RunStream("q 3 0 0 3 0 0 cm");

        Assert.Equal(1, engine.GraphicsStateStackDepth);
        Matrix ctm = engine.GetGraphicsState().GetCurrentTransformationMatrix();
        Assert.Equal(3f, ctm.GetScaleX());
    }

    // ── Text-matrix management ────────────────────────────────────────────────

    [Fact]
    public void BeginText_ResetsTextMatrixToIdentity()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.BeginText(engine));

        // SetMatrix sets non-identity, then BT should reset.
        engine.RunStream("1 0 0 1 100 200 Tm BT");

        Matrix tm = engine.GetTextMatrix();
        Assert.Equal(1f, tm.GetScaleX());
        Assert.Equal(0f, tm.GetTranslateX());
        Assert.Equal(0f, tm.GetTranslateY());
    }

    [Fact]
    public void EndText_ResetsTextMatrixToIdentity()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.EndText(engine));

        engine.RunStream("1 0 0 1 50 60 Tm ET");

        Matrix tm = engine.GetTextMatrix();
        Assert.Equal(0f, tm.GetTranslateX());
        Assert.Equal(0f, tm.GetTranslateY());
    }

    [Fact]
    public void SetMatrix_Tm_SetsBothTextAndTextLineMatrices()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));

        engine.RunStream("1 0 0 1 72 500 Tm");

        Assert.Equal(72f, engine.GetTextMatrix().GetTranslateX());
        Assert.Equal(500f, engine.GetTextMatrix().GetTranslateY());
        Assert.Equal(72f, engine.GetTextLineMatrix().GetTranslateX());
        Assert.Equal(500f, engine.GetTextLineMatrix().GetTranslateY());
    }

    [Fact]
    public void MoveText_Td_TranslatesTextAndLineMatrix()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        engine.RunStream("10 20 Td");

        Assert.Equal(10f, engine.GetTextMatrix().GetTranslateX());
        Assert.Equal(20f, engine.GetTextMatrix().GetTranslateY());
        Assert.Equal(10f, engine.GetTextLineMatrix().GetTranslateX());
        Assert.Equal(20f, engine.GetTextLineMatrix().GetTranslateY());
    }

    [Fact]
    public void MoveText_Td_AccumulatesFromCurrentTextLineMatrix()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        // Two sequential moves: (10,20) then (5,0) should give (15,20).
        engine.RunStream("10 20 Td 5 0 Td");

        Assert.Equal(15f, engine.GetTextMatrix().GetTranslateX());
        Assert.Equal(20f, engine.GetTextMatrix().GetTranslateY());
    }

    [Fact]
    public void MoveTextSetLeading_TD_SetsLeadingAndTranslates()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.MoveTextSetLeading(engine));

        engine.RunStream("0 -14 TD");

        Assert.Equal(14f, engine.GetGraphicsState().GetTextState().GetLeading());
        Assert.Equal(-14f, engine.GetTextMatrix().GetTranslateY());
    }

    [Fact]
    public void NextLine_TStar_MovesDownByLeading()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextLeading(engine));
        engine.AddOperator(new ContentStream.Operator.Text.NextLine(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        engine.RunStream("12 TL T*");

        Assert.Equal(-12f, engine.GetTextMatrix().GetTranslateY());
    }

    [Fact]
    public void SetMatrix_ThenMoveText_MoveTextIsRelativeToLineMatrix()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        engine.RunStream("1 0 0 1 100 700 Tm 0 -14 Td");

        // After Tm: text line matrix at (100, 700).
        // After Td(0, -14): text matrix at (100, 686).
        Assert.Equal(100f, engine.GetTextMatrix().GetTranslateX());
        Assert.Equal(686f, engine.GetTextMatrix().GetTranslateY());
    }

    // ── Text state ────────────────────────────────────────────────────────────

    [Fact]
    public void SetFontAndSize_Tf_UpdatesFontSizeInTextState()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetFontAndSize(engine));

        engine.RunStream("/Helvetica 14 Tf");

        Assert.Equal(14f, engine.GetGraphicsState().GetTextState().GetFontSize());
    }

    [Fact]
    public void SetCharSpacing_Tc_UpdatesCharacterSpacing()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetCharSpacing(engine));

        engine.RunStream("2 Tc");

        Assert.Equal(2f, engine.GetGraphicsState().GetTextState().GetCharacterSpacing());
    }

    [Fact]
    public void SetWordSpacing_Tw_UpdatesWordSpacing()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetWordSpacing(engine));

        engine.RunStream("3.5 Tw");

        Assert.Equal(3.5f, engine.GetGraphicsState().GetTextState().GetWordSpacing());
    }

    [Fact]
    public void SetTextLeading_TL_UpdatesLeading()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextLeading(engine));

        engine.RunStream("14 TL");

        Assert.Equal(14f, engine.GetGraphicsState().GetTextState().GetLeading());
    }

    [Fact]
    public void SetTextHorizontalScaling_Tz_UpdatesHorizontalScaling()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextHorizontalScaling(engine));

        engine.RunStream("80 Tz");

        Assert.Equal(80f, engine.GetGraphicsState().GetTextState().GetHorizontalScaling());
    }

    [Fact]
    public void SetTextRenderingMode_Tr_UpdatesRenderingMode()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextRenderingMode(engine));

        engine.RunStream("2 Tr");

        Assert.Equal(2, engine.GetGraphicsState().GetTextState().GetRenderingMode());
    }

    [Fact]
    public void SetTextRise_Ts_UpdatesRise()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.Text.SetTextRise(engine));

        engine.RunStream("5 Ts");

        Assert.Equal(5f, engine.GetGraphicsState().GetTextState().GetRise());
    }

    [Fact]
    public void Concatenate_cm_MultiplesCTMByOperandMatrix()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Concatenate(engine));

        engine.RunStream("2 0 0 3 10 20 cm");

        Matrix ctm = engine.GetGraphicsState().GetCurrentTransformationMatrix();
        Assert.Equal(2f, ctm.GetScaleX());
        Assert.Equal(3f, ctm.GetScaleY());
        Assert.Equal(10f, ctm.GetTranslateX());
        Assert.Equal(20f, ctm.GetTranslateY());
    }

    // ── Simple content stream executed end-to-end ─────────────────────────────

    [Fact]
    public void SimpleContentStream_AllOperatorsDispatched()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));
        engine.AddOperator(new ContentStream.Operator.State.Concatenate(engine));
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.BeginText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.EndText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.SetFontAndSize(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        // A realistic minimal page content stream.
        engine.RunStream("q 1 0 0 1 0 0 cm BT /F1 12 Tf 72 700 Td ET Q");

        Assert.Contains("q", engine.DispatchedOperators);
        Assert.Contains("cm", engine.DispatchedOperators);
        Assert.Contains("BT", engine.DispatchedOperators);
        Assert.Contains("Tf", engine.DispatchedOperators);
        Assert.Contains("Td", engine.DispatchedOperators);
        Assert.Contains("ET", engine.DispatchedOperators);
        Assert.Contains("Q", engine.DispatchedOperators);
    }

    [Fact]
    public void SimpleContentStream_StateConsistentAfterFullSequence()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.BeginText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.EndText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.SetFontAndSize(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        engine.RunStream("q BT /F1 10 Tf 100 200 Td ET Q");

        // After Q, graphics-state stack is empty.
        Assert.Equal(0, engine.GraphicsStateStackDepth);
        // After ET, text matrices are reset to identity.
        Assert.Equal(0f, engine.GetTextMatrix().GetTranslateX());
        Assert.Equal(0f, engine.GetTextMatrix().GetTranslateY());
        // After Q (Restore), the saved graphics state is popped; Tf ran inside the q/Q block
        // so the font size is rolled back to the pre-save default of 0.
        Assert.Equal(0f, engine.GetGraphicsState().GetTextState().GetFontSize());
    }

    [Fact]
    public void BeginEndTextBlock_TextMatrixSurvivesSaveRestoreOutsideBlock()
    {
        var engine = new TrackingEngine();
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));
        engine.AddOperator(new ContentStream.Operator.State.SetMatrix(engine));
        engine.AddOperator(new ContentStream.Operator.Text.BeginText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.EndText(engine));
        engine.AddOperator(new ContentStream.Operator.Text.SetFontAndSize(engine));
        engine.AddOperator(new ContentStream.Operator.Text.MoveText(engine));

        // Set text matrix inside BT block, ET resets it, but graphics state
        // save/restore is unrelated.
        engine.RunStream("q BT 1 0 0 1 50 100 Tm ET Q");

        Assert.Equal(0, engine.GraphicsStateStackDepth);
        Assert.Equal(0f, engine.GetTextMatrix().GetTranslateX());
    }

    [Fact]
    public void ProcessPage_DrawObject_TraversesFormXObjectContentStream()
    {
        var engine = new TrackingEngine();

        // Page content stream invokes form /Fm0.
        COSStream pageContents = new();
        using (Stream output = pageContents.CreateOutputStream())
        using (StreamWriter writer = new(output))
        {
            writer.Write("/Fm0 Do");
        }

        // Form stream contains q/Q operators that should be traversed by the engine.
        COSStream formStream = new();
        formStream.SetName(COSName.GetPDFName("Subtype"), "Form");
        using (Stream output = formStream.CreateOutputStream())
        using (StreamWriter writer = new(output))
        {
            writer.Write("q Q");
        }

        COSDictionary xObjects = new();
        xObjects.SetItem(COSName.GetPDFName("Fm0"), formStream);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("XObject"), xObjects);

        COSDictionary pageDict = new();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);
        pageDict.SetItem(COSName.CONTENTS, pageContents);
        pageDict.SetItem(COSName.RESOURCES, resources);

        engine.ProcessPage(new PDPage(pageDict));

        Assert.Contains("Do", engine.DispatchedOperators);
        Assert.Contains("q", engine.DispatchedOperators);
        Assert.Contains("Q", engine.DispatchedOperators);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Inline operator processor that delegates to a lambda, used for spy-style
    /// testing without requiring dedicated processor classes.
    /// </summary>
    private sealed class LambdaProcessor : OperatorProcessor
    {
        private readonly Action<PdfBox.Net.ContentStream.Operator.Operator, IList<COSBase>> _action;

        public LambdaProcessor(
            string name,
            PDFStreamEngine engine,
            Action<PdfBox.Net.ContentStream.Operator.Operator, IList<COSBase>> action)
            : base(name, engine)
        {
            _action = action;
        }

        public override void Process(
            PdfBox.Net.ContentStream.Operator.Operator op, IList<COSBase> operands)
        {
            _action(op, operands);
        }
    }
}
