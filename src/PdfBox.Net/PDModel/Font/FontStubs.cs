using PdfBox.Net.COS;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;
using System.IO;

namespace PdfBox.Net.PDModel.Font
{
    public interface PDFontLike
    {
        string GetName();
    }

    public abstract class PDFont : PDFontLike
    {
        public abstract string GetName();

        public virtual bool IsVertical() => false;
        public virtual float GetWidth(int code) => 0f;
        public virtual string? ToUnicode(int code, PdfBox.Net.PDModel.Font.Encoding.GlyphList glyphList) => null;
        public virtual float GetSpaceWidth() => 0f;
        public virtual float GetAverageFontWidth() => 500f;
        public virtual Matrix GetFontMatrix() => new();
        public virtual COSDictionary GetCOSObject() => new();
        public virtual BoundingBox GetBoundingBox() => new();
        public virtual PDFontDescriptor? GetFontDescriptor() => null;
    }

    public abstract class PDVectorFont : PDFont
    {
        public override abstract string GetName();
        public abstract bool HasGlyph(int code);
        public abstract GeneralPath GetNormalizedPath(int code);
    }

    public abstract class PDType0Font : PDVectorFont
    {
        public abstract int CodeToCID(int code);
        public virtual PDCIDFont? GetDescendantFont() => null;
    }

    public abstract class PDSimpleFont : PDVectorFont
    {
        public abstract FontBoxFont GetFontBoxFont();
        public abstract bool IsStandard14();
    }

    public abstract class PDTrueTypeFont : PDSimpleFont
    {
        public virtual TrueTypeFont GetTrueTypeFont() => new();
    }

    public abstract class PDCIDFont : PDFont
    {
    }

    public abstract class PDCIDFontType2 : PDCIDFont
    {
        public virtual TrueTypeFont GetTrueTypeFont() => new();
    }

    public class PDFontDescriptor
    {
        public virtual float GetCapHeight() => 0f;
        public virtual float GetAscent() => 0f;
        public virtual float GetDescent() => 0f;
    }

    public class TrueTypeFont
    {
        public virtual int GetUnitsPerEm() => 1000;
    }

    public class BoundingBox
        : PdfBox.Net.FontBox.Util.BoundingBox
    {
    }

    public class FontBoxFont
    {
        public virtual string GetName() => GetType().Name;
    }
}

namespace PdfBox.Net.PDModel.Font.Encoding
{
    public class GlyphList
    {
        public GlyphList()
        {
        }

        public GlyphList(GlyphList baseGlyphList, Stream? input)
        {
        }

        public static GlyphList GetAdobeGlyphList() => new();
    }
}
