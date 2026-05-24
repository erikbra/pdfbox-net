/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for the PDModel state and resource objects introduced in issue #16:
 * PDGraphicsState, PDTextState, PDMarkedContent, PDResources, PDDictionaryFont,
 * PDPage.GetResources(), and SetFontAndSize font resolution.
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
using PdfBox.Net.ContentStream.Operator.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Text;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for the PDModel state/resource types introduced in issue #16.
/// Covers:
/// <list type="bullet">
///   <item>PDGraphicsState – CTM management and clone/save/restore semantics.</item>
///   <item>PDTextState – property mutations and clone behavior.</item>
///   <item>PDMarkedContent – hierarchical content tree building.</item>
///   <item>PDResources – font and XObject lookup from a COS resource dictionary.</item>
///   <item>PDDictionaryFont – name resolution from BaseFont entry.</item>
///   <item>PDPage.GetResources() – round-trip through the page dictionary.</item>
///   <item>SetFontAndSize – font resolution wired through page resources.</item>
/// </list>
/// </summary>
public class PDModelStateTest
{
    // ── PDGraphicsState ───────────────────────────────────────────────────────

    [Fact]
    public void PDGraphicsState_DefaultCTM_IsIdentity()
    {
        var state = new PDGraphicsState();
        Matrix ctm = state.GetCurrentTransformationMatrix();
        Assert.Equal(1f, ctm.GetScaleX());
        Assert.Equal(1f, ctm.GetScaleY());
        Assert.Equal(0f, ctm.GetTranslateX());
        Assert.Equal(0f, ctm.GetTranslateY());
    }

    [Fact]
    public void PDGraphicsState_SetCTM_RoundTrips()
    {
        var state = new PDGraphicsState();
        var m = new Matrix(2f, 0f, 0f, 3f, 10f, 20f);
        state.SetCurrentTransformationMatrix(m);

        Matrix ctm = state.GetCurrentTransformationMatrix();
        Assert.Equal(2f, ctm.GetScaleX());
        Assert.Equal(3f, ctm.GetScaleY());
        Assert.Equal(10f, ctm.GetTranslateX());
        Assert.Equal(20f, ctm.GetTranslateY());
    }

    [Fact]
    public void PDGraphicsState_SetCTM_Null_SetsIdentity()
    {
        var state = new PDGraphicsState();
        state.SetCurrentTransformationMatrix(null!);
        Matrix ctm = state.GetCurrentTransformationMatrix();
        Assert.Equal(1f, ctm.GetScaleX());
    }

    [Fact]
    public void PDGraphicsState_Clone_ProducesIndependentTextState()
    {
        var original = new PDGraphicsState();
        original.GetTextState().FontSize = 14f;

        PDGraphicsState clone = original.Clone();
        clone.GetTextState().FontSize = 28f;

        // Mutation of the clone must not affect the original
        Assert.Equal(14f, original.GetTextState().FontSize);
        Assert.Equal(28f, clone.GetTextState().FontSize);
    }

    [Fact]
    public void PDGraphicsState_Clone_CTM_IsSameReference()
    {
        // CTM is a value-type-like struct; the clone should use the same matrix value.
        var original = new PDGraphicsState();
        var m = new Matrix(3f, 0f, 0f, 3f, 0f, 0f);
        original.SetCurrentTransformationMatrix(m);

        PDGraphicsState clone = original.Clone();

        Assert.Equal(3f, clone.GetCurrentTransformationMatrix().GetScaleX());
    }

    // ── PDTextState ───────────────────────────────────────────────────────────

    [Fact]
    public void PDTextState_Defaults_MatchPDFSpec()
    {
        var ts = new PDTextState();
        Assert.Equal(0f, ts.FontSize);
        Assert.Equal(100f, ts.HorizontalScaling);
        Assert.Equal(0f, ts.CharacterSpacing);
        Assert.Equal(0f, ts.WordSpacing);
        Assert.Equal(0f, ts.Leading);
        Assert.Equal(0, ts.RenderingMode);
        Assert.Equal(0f, ts.Rise);
        Assert.Null(ts.Font);
    }

    [Fact]
    public void PDTextState_Clone_IsIndependent()
    {
        var ts = new PDTextState { FontSize = 12f, WordSpacing = 2f };
        PDTextState clone = ts.Clone();

        clone.FontSize = 24f;
        clone.WordSpacing = 4f;

        Assert.Equal(12f, ts.FontSize);
        Assert.Equal(2f, ts.WordSpacing);
    }

    [Fact]
    public void PDTextState_Clone_SharesFontReference()
    {
        // Font is a reference type; clone should share the same instance.
        var font = new TestDictionaryFont("Helvetica");
        var ts = new PDTextState { Font = font };
        PDTextState clone = ts.Clone();

        Assert.Same(font, clone.Font);
    }

    [Fact]
    public void PDTextState_GettersDelegateToProperties()
    {
        var ts = new PDTextState
        {
            FontSize = 10f,
            HorizontalScaling = 80f,
            CharacterSpacing = 0.5f,
            WordSpacing = 1.0f,
            Leading = 12f,
            RenderingMode = 1,
            Rise = 3f,
        };

        Assert.Equal(10f, ts.GetFontSize());
        Assert.Equal(80f, ts.GetHorizontalScaling());
        Assert.Equal(0.5f, ts.GetCharacterSpacing());
        Assert.Equal(1.0f, ts.GetWordSpacing());
        Assert.Equal(12f, ts.GetLeading());
        Assert.Equal(1, ts.GetRenderingMode());
        Assert.Equal(3f, ts.GetRise());
    }

    // ── PDMarkedContent ───────────────────────────────────────────────────────

    [Fact]
    public void PDMarkedContent_Create_SetsTagAndNullProperties()
    {
        var tag = COSName.GetPDFName("Span");
        var mc = PDMarkedContent.Create(tag, null);

        Assert.Same(tag, mc.Tag);
        Assert.Null(mc.Properties);
    }

    [Fact]
    public void PDMarkedContent_Create_WithProperties_ReturnsActualText()
    {
        var props = new COSDictionary();
        props.SetItem(COSName.GetPDFName("ActualText"), new COSString("Hello"));
        var mc = PDMarkedContent.Create(COSName.GetPDFName("Span"), props);

        Assert.Equal("Hello", mc.GetActualText());
    }

    [Fact]
    public void PDMarkedContent_AddMarkedContent_AppearsInGetMarkedContents()
    {
        var parent = PDMarkedContent.Create(COSName.GetPDFName("Div"), null);
        var child = PDMarkedContent.Create(COSName.GetPDFName("P"), null);
        parent.AddMarkedContent(child);

        Assert.Single(parent.GetMarkedContents());
        Assert.Same(child, parent.GetMarkedContents()[0]);
    }

    [Fact]
    public void PDMarkedContent_AddText_AppearsInGetTexts()
    {
        var mc = PDMarkedContent.Create(COSName.GetPDFName("Span"), null);
        var tp = CreateDummyTextPosition("A");
        mc.AddText(tp);

        Assert.Single(mc.GetTexts());
        Assert.Same(tp, mc.GetTexts()[0]);
    }

    [Fact]
    public void PDMarkedContent_AddXObject_AppearsInGetXObjects()
    {
        var mc = PDMarkedContent.Create(COSName.GetPDFName("Figure"), null);
        var xo = new PDXObject();
        mc.AddXObject(xo);

        Assert.Single(mc.GetXObjects());
        Assert.Same(xo, mc.GetXObjects()[0]);
    }

    // ── PDResources ───────────────────────────────────────────────────────────

    [Fact]
    public void PDResources_GetFont_NoFontSubDict_ReturnsNull()
    {
        var resources = new PDResources();
        PDFont? font = resources.GetFont(COSName.GetPDFName("F1"));
        Assert.Null(font);
    }

    [Fact]
    public void PDResources_GetFont_KnownName_ReturnsPDDictionaryFont()
    {
        var resources = BuildResourcesWithFont("F1", "Helvetica");
        PDFont? font = resources.GetFont(COSName.GetPDFName("F1"));

        Assert.NotNull(font);
        Assert.Equal("Helvetica", font.GetName());
    }

    [Fact]
    public void PDResources_GetFont_UnknownName_ReturnsNull()
    {
        var resources = BuildResourcesWithFont("F1", "Helvetica");
        PDFont? font = resources.GetFont(COSName.GetPDFName("F99"));

        Assert.Null(font);
    }

    [Fact]
    public void PDResources_GetFontNames_ListsAvailableFonts()
    {
        var resources = BuildResourcesWithFont("F1", "Helvetica");
        var names = resources.GetFontNames().Select(n => n.GetName()).ToList();

        Assert.Contains("F1", names);
    }

    [Fact]
    public void PDResources_GetFontNames_EmptyResources_ReturnsEmpty()
    {
        var resources = new PDResources();
        Assert.Empty(resources.GetFontNames());
    }

    [Fact]
    public void PDResources_GetXObject_NoXObjectSubDict_ReturnsNull()
    {
        var resources = new PDResources();
        Assert.Null(resources.GetXObject(COSName.GetPDFName("Im1")));
    }

    [Fact]
    public void PDResources_GetXObjectNames_EmptyResources_ReturnsEmpty()
    {
        var resources = new PDResources();
        Assert.Empty(resources.GetXObjectNames());
    }

    // ── PDDictionaryFont ──────────────────────────────────────────────────────

    [Fact]
    public void PDDictionaryFont_GetName_ReturnsBaseFont()
    {
        var dict = new COSDictionary();
        dict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Times-Roman"));
        var font = PDDictionaryFont.Create(dict);

        Assert.Equal("Times-Roman", font.GetName());
    }

    [Fact]
    public void PDDictionaryFont_GetName_NoBaseFont_ReturnsUnknown()
    {
        var dict = new COSDictionary();
        var font = PDDictionaryFont.Create(dict);

        Assert.Equal("Unknown", font.GetName());
    }

    [Fact]
    public void PDDictionaryFont_GetCOSObject_ReturnsSameDict()
    {
        var dict = new COSDictionary();
        var font = PDDictionaryFont.Create(dict);

        Assert.Same(dict, font.GetCOSObject());
    }

    [Fact]
    public void PDDictionaryFont_Create_NullDict_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PDDictionaryFont.Create(null!));
    }

    // ── PDPage.GetResources() ─────────────────────────────────────────────────

    [Fact]
    public void PDPage_GetResources_NoResourceEntry_ReturnsNull()
    {
        var page = new PDPage();
        // A new default page has no /Resources entry
        Assert.Null(page.GetResources());
    }

    [Fact]
    public void PDPage_GetResources_WithResourceEntry_ReturnsPDResources()
    {
        var pageDict = new COSDictionary();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);
        var resourceDict = new COSDictionary();
        pageDict.SetItem(COSName.RESOURCES, resourceDict);

        var page = new PDPage(pageDict);
        PDResources? resources = page.GetResources();

        Assert.NotNull(resources);
        Assert.Same(resourceDict, resources.GetCOSObject());
    }

    [Fact]
    public void PDPage_GetResources_WithFont_FontLookupWorks()
    {
        var pageDict = new COSDictionary();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);

        var fontDict = new COSDictionary();
        fontDict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Courier"));

        var fontSubDict = new COSDictionary();
        fontSubDict.SetItem(COSName.GetPDFName("F1"), fontDict);

        var resourceDict = new COSDictionary();
        resourceDict.SetItem(COSName.GetPDFName("Font"), fontSubDict);
        pageDict.SetItem(COSName.RESOURCES, resourceDict);

        var page = new PDPage(pageDict);
        PDFont? font = page.GetResources()?.GetFont(COSName.GetPDFName("F1"));

        Assert.NotNull(font);
        Assert.Equal("Courier", font.GetName());
    }

    // ── SetFontAndSize with resource resolution ───────────────────────────────

    [Fact]
    public void SetFontAndSize_Tf_ResolvesFont_WhenResourcesAvailable()
    {
        // Build a page with a /Font resource.
        var (engine, page) = BuildEngineWithPage("F1", "Arial", 12f);
        _ = page; // page is set via ProcessPage which is not called here; we test direct stream execution

        // Exercise the engine with a Tf instruction; the font must be resolved.
        engine.RunStream("/F1 12 Tf");

        PDFont? resolvedFont = engine.GetGraphicsState().GetTextState().GetFont();
        Assert.NotNull(resolvedFont);
        Assert.Equal("Arial", resolvedFont.GetName());
    }

    [Fact]
    public void SetFontAndSize_Tf_SetsFontSize()
    {
        var engine = new ObservingEngine();
        engine.AddOperator(new SetFontAndSize(engine));

        engine.RunStream("/F1 14 Tf");

        Assert.Equal(14f, engine.GetGraphicsState().GetTextState().GetFontSize());
    }

    [Fact]
    public void SetFontAndSize_Tf_UnknownFont_SetsNull()
    {
        // Build an engine with a page that has /Font resources, but /F99 is not in them.
        var pageDict = new COSDictionary();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);
        var resourceDict = new COSDictionary();
        var fontSubDict = new COSDictionary();
        var fontEntryDict = new COSDictionary();
        fontEntryDict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));
        fontSubDict.SetItem(COSName.GetPDFName("F1"), fontEntryDict);
        resourceDict.SetItem(COSName.GetPDFName("Font"), fontSubDict);
        pageDict.SetItem(COSName.RESOURCES, resourceDict);

        var engine = new EngineWithPage(new PDPage(pageDict));
        engine.AddOperator(new SetFontAndSize(engine));

        engine.RunStream("/F99 12 Tf");

        Assert.Null(engine.GetGraphicsState().GetTextState().GetFont());
    }

    [Fact]
    public void SetFontAndSize_Tf_NoPage_SetsNullFont()
    {
        // When no page has been set (GetCurrentPage() returns null) the font must be null.
        var engine = new ObservingEngine();
        engine.AddOperator(new SetFontAndSize(engine));

        engine.RunStream("/F1 10 Tf");

        Assert.Null(engine.GetGraphicsState().GetTextState().GetFont());
        Assert.Equal(10f, engine.GetGraphicsState().GetTextState().GetFontSize());
    }

    // ── Content-stream exercises the new model objects ────────────────────────

    [Fact]
    public void ContentStream_GraphicsStateSaveRestore_PreservesFont()
    {
        var pageDict = new COSDictionary();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);
        var resourceDict = new COSDictionary();
        var fontSubDict = new COSDictionary();
        var fontEntryDict = new COSDictionary();
        fontEntryDict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));
        fontSubDict.SetItem(COSName.GetPDFName("F1"), fontEntryDict);
        resourceDict.SetItem(COSName.GetPDFName("Font"), fontSubDict);
        pageDict.SetItem(COSName.RESOURCES, resourceDict);

        var engine = new EngineWithPage(new PDPage(pageDict));
        engine.AddOperator(new SetFontAndSize(engine));
        engine.AddOperator(new ContentStream.Operator.State.Save(engine));
        engine.AddOperator(new ContentStream.Operator.State.Restore(engine));

        // Set font, save, clear font by setting a non-existent name, then restore.
        engine.RunStream("/F1 12 Tf q /F99 8 Tf Q");

        PDFont? restoredFont = engine.GetGraphicsState().GetTextState().GetFont();
        Assert.NotNull(restoredFont);
        Assert.Equal("Helvetica", restoredFont.GetName());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Builds a PDResources with a single font entry.</summary>
    private static PDResources BuildResourcesWithFont(string resourceName, string baseFontName)
    {
        var fontEntryDict = new COSDictionary();
        fontEntryDict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName(baseFontName));

        var fontSubDict = new COSDictionary();
        fontSubDict.SetItem(COSName.GetPDFName(resourceName), fontEntryDict);

        var resourceDict = new COSDictionary();
        resourceDict.SetItem(COSName.GetPDFName("Font"), fontSubDict);

        return new PDResources(resourceDict);
    }

    /// <summary>Builds an engine with a page that contains a font resource and runs a stream.</summary>
    private static (EngineWithPage engine, PDPage page) BuildEngineWithPage(
        string fontResName, string baseFontName, float fontSize)
    {
        var pageDict = new COSDictionary();
        pageDict.SetItem(COSName.TYPE, COSName.PAGE);

        var fontEntryDict = new COSDictionary();
        fontEntryDict.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName(baseFontName));

        var fontSubDict = new COSDictionary();
        fontSubDict.SetItem(COSName.GetPDFName(fontResName), fontEntryDict);

        var resourceDict = new COSDictionary();
        resourceDict.SetItem(COSName.GetPDFName("Font"), fontSubDict);

        pageDict.SetItem(COSName.RESOURCES, resourceDict);

        var page = new PDPage(pageDict);
        var engine = new EngineWithPage(page);
        engine.AddOperator(new SetFontAndSize(engine));
        return (engine, page);
    }

    private static TextPosition CreateDummyTextPosition(string unicode)
    {
        var textMatrix = new Matrix();
        return new TextPosition(0, 612f, 792f, textMatrix, 10f, 780f, 12f, 8f, 4f,
            unicode, [65], null!, 12f, 12);
    }

    // ── Minimal test engines ──────────────────────────────────────────────────

    /// <summary>
    /// An engine that exposes the protected accessors for assertion and runs raw content streams.
    /// No current page is set.
    /// </summary>
    private sealed class ObservingEngine : PDFStreamEngine
    {
        public new PDGraphicsState GetGraphicsState() => base.GetGraphicsState();

        public void RunStream(string content)
        {
            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }
    }

    /// <summary>
    /// An engine with a pre-set current page so that SetFontAndSize can resolve fonts
    /// from the page's resource dictionary.
    /// </summary>
    private sealed class EngineWithPage : PDFStreamEngine
    {
        public EngineWithPage(PDPage page)
        {
            // Inject the page by running an empty ProcessPage to set _currentPage.
            // We override ProcessPage so we can skip stream processing.
            _page = page;
        }

        private readonly PDPage _page;

        public override void ProcessPage(PDPage page)
        {
            // Delegate to base but use our stored page for resource access.
            base.ProcessPage(page);
        }

        // Expose GetCurrentPage via the engine's protected method, overriding to return _page.
        public new PDGraphicsState GetGraphicsState() => base.GetGraphicsState();

        public void RunStream(string content)
        {
            // Set _currentPage by temporarily calling ProcessPage with an empty COSStream.
            var emptyPageDict = new COSDictionary();
            emptyPageDict.SetItem(COSName.TYPE, COSName.PAGE);
            // We can't directly call ProcessPage without a valid stream, so instead
            // we call ProcessStream directly after manually setting the page field via
            // the public ProcessPage API with a page that has no contents.
            var pageContents = new COSDictionary();
            pageContents.SetItem(COSName.GetPDFName("Contents"), _page.GetContents()!);

            // Manually inject the current page using ProcessPage (which calls SetCurrentPage
            // internally in PDFStreamEngine.ProcessPage).
            base.ProcessPage(_page);

            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }
    }

    /// <summary>Minimal concrete PDFont for sharing-reference tests.</summary>
    private sealed class TestDictionaryFont : PDFont
    {
        private readonly string _name;
        public TestDictionaryFont(string name) => _name = name;
        public override string GetName() => _name;
    }
}
