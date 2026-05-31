using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public class PDAppearanceStream : PDFormXObject
{
    private static readonly COSName MatrixName = COSName.GetPDFName("Matrix");

    public PDAppearanceStream(PDDocument document)
        : base(new PDStream(document).GetCOSObject())
    {
    }

    public PDAppearanceStream(COSStream stream)
        : base(stream)
    {
    }

    public new PDStream GetStream() => GetContentStream();

    public new void SetMatrix(Matrix matrix)
    {
        COSArray array =
        [
            new COSFloat(matrix.GetScaleX()),
            new COSFloat(matrix.GetShearY()),
            new COSFloat(matrix.GetShearX()),
            new COSFloat(matrix.GetScaleY()),
            new COSFloat(matrix.GetTranslateX()),
            new COSFloat(matrix.GetTranslateY())
        ];
        GetCOSObject()?.SetItem(MatrixName, array);
    }
}
