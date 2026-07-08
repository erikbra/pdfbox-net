using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.Rendering;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDFGraphicsStreamEngineCallbackApiTest
{
    [Fact]
    public void GraphicsOperatorsDispatchToPDFGraphicsStreamEngineCallbacks()
    {
        var engine = new RecordingGraphicsEngine(new PDPage());

        engine.RunStream("0 0 m 10 0 l 10 10 20 20 30 30 c h 0 0 10 10 re W* n 0 0 m 10 0 l S 0 0 m 10 0 l f 0 0 m 10 0 l B /Sh1 sh");

        Assert.True(engine.AppendRectangleCalls > 0);
        Assert.True(engine.MoveToCalls > 0);
        Assert.True(engine.LineToCalls > 0);
        Assert.True(engine.CurveToCalls > 0);
        Assert.True(engine.ClosePathCalls > 0);
        Assert.True(engine.ClipCalls > 0);
        Assert.True(engine.EndPathCalls > 0);
        Assert.True(engine.StrokePathCalls > 0);
        Assert.True(engine.FillPathCalls > 0);
        Assert.True(engine.FillAndStrokePathCalls > 0);
        Assert.Equal(1, engine.ShadingFillCalls);
    }

    [Fact]
    public void DrawImageCallbackIsPubliclyInvokable()
    {
        var engine = new RecordingGraphicsEngine(new PDPage());
        engine.DrawImage(new FakeImage());
        Assert.Equal(1, engine.DrawImageCalls);
    }

    [Fact]
    public void InlineImageOperatorDispatchesToDrawImageCallback()
    {
        var engine = new RecordingGraphicsEngine(new PDPage());

        engine.RunStream(InlineImageStream());

        Assert.Equal(1, engine.DrawImageCalls);
    }

    private static byte[] InlineImageStream()
    {
        using MemoryStream stream = new();
        WriteAscii(stream, "q\n1 0 0 1 0 0 cm\nBI\n/W 1 /H 1 /BPC 8 /CS /RGB\nID\n");
        stream.Write([255, 0, 0]);
        WriteAscii(stream, "\nEI\nQ\n");
        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        byte[] bytes = Encoding.Latin1.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private sealed class RecordingGraphicsEngine : PDFGraphicsStreamEngine
    {
        public int AppendRectangleCalls { get; private set; }
        public int DrawImageCalls { get; private set; }
        public int ClipCalls { get; private set; }
        public int MoveToCalls { get; private set; }
        public int LineToCalls { get; private set; }
        public int CurveToCalls { get; private set; }
        public int ClosePathCalls { get; private set; }
        public int EndPathCalls { get; private set; }
        public int StrokePathCalls { get; private set; }
        public int FillPathCalls { get; private set; }
        public int FillAndStrokePathCalls { get; private set; }
        public int ShadingFillCalls { get; private set; }

        public RecordingGraphicsEngine(PDPage page) : base(page)
        {
        }

        public void RunStream(string content)
        {
            using var ms = new MemoryStream(Encoding.Latin1.GetBytes(content));
            ProcessStream(ms);
        }

        public void RunStream(byte[] content)
        {
            using var ms = new MemoryStream(content);
            ProcessStream(ms);
        }

        public override void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
        {
            AppendRectangleCalls++;
            base.AppendRectangle(p0, p1, p2, p3);
        }

        public override void DrawImage(PDImage pdImage)
        {
            DrawImageCalls++;
        }

        public override void Clip(int windingRule)
        {
            ClipCalls++;
            base.Clip(windingRule);
        }

        public override void MoveTo(float x, float y)
        {
            MoveToCalls++;
            base.MoveTo(x, y);
        }

        public override void LineTo(float x, float y)
        {
            LineToCalls++;
            base.LineTo(x, y);
        }

        public override void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            CurveToCalls++;
            base.CurveTo(x1, y1, x2, y2, x3, y3);
        }

        public override Point2D? GetCurrentPoint()
        {
            return base.GetCurrentPoint();
        }

        public override void ClosePath()
        {
            ClosePathCalls++;
            base.ClosePath();
        }

        public override void EndPath()
        {
            EndPathCalls++;
            base.EndPath();
        }

        public override void StrokePath()
        {
            StrokePathCalls++;
            base.StrokePath();
        }

        public override void FillPath(int windingRule)
        {
            FillPathCalls++;
            base.FillPath(windingRule);
        }

        public override void FillAndStrokePath(int windingRule)
        {
            FillAndStrokePathCalls++;
            base.FillAndStrokePath(windingRule);
        }

        public override void ShadingFill(COSName shadingName)
        {
            ShadingFillCalls++;
        }
    }

    private sealed class FakeImage : PDImage
    {
        private readonly COSDictionary _dictionary = new();

        public override COSDictionary GetCOSObject() => _dictionary;
        public override int GetBitsPerComponent() => 8;
        public override void SetBitsPerComponent(int bitsPerComponent) { }
        public override PDColorSpace GetColorSpace() => PDDeviceRGB.Instance;
        public override void SetColorSpace(PDColorSpace? colorSpace) { }
        public override int GetHeight() => 1;
        public override void SetHeight(int height) { }
        public override int GetWidth() => 1;
        public override void SetWidth(int width) { }
        public override bool GetInterpolate() => false;
        public override void SetInterpolate(bool value) { }
        public override void SetDecode(COSArray? decode) { }
        public override COSArray? GetDecode() => null;
        public override bool IsStencil() => false;
        public override void SetStencil(bool isStencil) { }
        public override Stream CreateInputStream() => new MemoryStream(Array.Empty<byte>());
        public override Stream CreateInputStream(DecodeOptions options) => new MemoryStream(Array.Empty<byte>());
        public override Stream CreateInputStream(IList<string> stopFilters) => new MemoryStream(Array.Empty<byte>());
        public override bool IsEmpty() => true;
        public override byte[] GetData() => Array.Empty<byte>();
    }
}
