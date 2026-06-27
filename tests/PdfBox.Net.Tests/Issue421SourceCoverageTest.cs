/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Focused coverage for issue #421 source-coverage compatibility classes.
 *
 * PORT_MODE: native-test
 */

using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.ContentStream.Operator.Color;
using PdfBox.Net.ContentStream.Operator.Graphics;
using PdfBox.Net.ContentStream.Operator.State;
using PdfBox.Net.ContentStream.Operator.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class Issue421SourceCoverageTest
{
    [Fact]
    public void JavaNamedStateProcessors_UpdateGraphicsState()
    {
        ProbeEngine engine = new();
        engine.AddOperator(new SetLineCapStyle(engine));
        engine.AddOperator(new SetLineJoinStyle(engine));
        engine.AddOperator(new SetLineMiterLimit(engine));

        engine.RunStream("1 J 2 j 6.5 M");

        PDGraphicsState graphicsState = engine.GetGraphicsState();
        Assert.Equal(1, graphicsState.GetLineCap());
        Assert.Equal(2, graphicsState.GetLineJoin());
        Assert.Equal(6.5f, graphicsState.GetMiterLimit(), precision: 3);
    }

    [Fact]
    public void ContentStreamOperatorsExposeJavaGetName()
    {
        ProbeEngine engine = new();

        Assert.Equal(OperatorName.BEGIN_TEXT, new BeginText(engine).GetName());
        Assert.Equal(OperatorName.LINE_TO, new LineTo(engine).GetName());
        Assert.Equal(OperatorName.SET_LINE_CAPSTYLE, new SetLineCapStyle(engine).GetName());
        Assert.Equal(OperatorName.NON_STROKING_COLOR, new SetNonStrokingColor(engine).GetName());
    }

    [Fact]
    public void ColorOperatorsExposeProtectedColorHelpers()
    {
        ProbeEngine engine = new();
        NonStrokingColorProbe op = new(engine);
        PDColor color = new([0.1f, 0.2f, 0.3f], PDDeviceRGB.Instance);

        op.Apply(color);

        Assert.Same(color, op.Current());
    }

    [Fact]
    public void JavaNamedAnnotations_MatchExistingAnnotationBehavior()
    {
        PDAnnotationStrikeout strikeout = new();
        PDAnnotationRubberStamp rubberStamp = new();

        Assert.Equal(PDAnnotationStrikeOut.SUB_TYPE, strikeout.GetSubtype());
        Assert.Equal(PDAnnotationStamp.SUB_TYPE, rubberStamp.GetSubtype());
        Assert.Equal(PDAnnotationStamp.NAME_DRAFT, rubberStamp.GetName());

        rubberStamp.SetName(PDAnnotationStamp.NAME_APPROVED);

        Assert.Equal(PDAnnotationStamp.NAME_APPROVED, rubberStamp.GetName());
        Assert.IsType<PDAnnotationStrikeOut>(PDAnnotation.CreateAnnotation(strikeout.GetCOSObject()));
        Assert.IsType<PDAnnotationStamp>(PDAnnotation.CreateAnnotation(rubberStamp.GetCOSObject()));
    }

    [Fact]
    public void FontMapperImpl_DelegatesToDefaultProvider()
    {
        FontMapper mapper = new FontMapperImpl();

        Assert.Null(mapper.FindFontFile(""));
    }

    [Fact]
    public void BaseParser_ExposesExpectedUtilitySemantics()
    {
        ParserProbe parser = ParserProbe.FromAscii(" \t\r\n-42 /Name");

        parser.SkipWhiteSpacesPublic();
        Assert.Equal(-42, parser.ReadIntPublic());
        Assert.Equal(' ', parser.ReadChar());
        Assert.True(ParserProbe.IsEndOfNamePublic('/'));
        Assert.True(ParserProbe.IsDigitPublic('7'));
        Assert.False(ParserProbe.IsDigitPublic('x'));
    }

    [Fact]
    public void BaseParser_ReadExpectedStringAndCharConsumeInput()
    {
        ParserProbe parser = ParserProbe.FromAscii("obj\n");

        parser.ReadExpectedStringPublic("obj");
        parser.ReadExpectedCharPublic('\n');

        Assert.True(parser.IsEOFPublic());
    }

    private sealed class ProbeEngine : PDFStreamEngine
    {
        public void RunStream(string content)
        {
            using MemoryStream stream = new(Encoding.Latin1.GetBytes(content));
            ProcessStream(stream);
        }

        public new PDGraphicsState GetGraphicsState() => base.GetGraphicsState();
    }

    private sealed class NonStrokingColorProbe : SetNonStrokingColor
    {
        public NonStrokingColorProbe(PDFStreamEngine context)
            : base(context)
        {
        }

        public PDColor Current() => GetColor();

        public void Apply(PDColor color) => SetColor(color);
    }

    private sealed class ParserProbe : BaseParser
    {
        private ParserProbe(Stream source)
            : base(source)
        {
        }

        public static ParserProbe FromAscii(string value)
        {
            return new ParserProbe(new MemoryStream(Encoding.ASCII.GetBytes(value)));
        }

        public void SkipWhiteSpacesPublic() => SkipWhiteSpaces();

        public int ReadIntPublic() => ReadInt();

        public void ReadExpectedStringPublic(string value) => ReadExpectedString(value);

        public void ReadExpectedCharPublic(char value) => ReadExpectedChar(value);

        public bool IsEOFPublic() => IsEOF();

        public char ReadChar() => (char)Source.ReadByte();

        public static bool IsEndOfNamePublic(int ch) => IsEndOfName(ch);

        public static bool IsDigitPublic(int ch) => IsDigit(ch);
    }
}
