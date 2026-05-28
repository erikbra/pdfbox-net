using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDSquareAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDSquareAppearanceHandler(PDAnnotationSquare annotation, PDDocument? document = null)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationSquare annotation = (PDAnnotationSquare)Annotation;
        float lineWidth = ResolveLineWidth(annotation);

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        bool hasStroke = contents.SetStrokingColorOnDemand(Color);
        bool hasFill = contents.SetNonStrokingColorOnDemand(annotation.GetInteriorColor());
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());

        PDRectangle box = Shrink(Rectangle, Math.Max(0.5f, lineWidth / 2f));
        contents.AddRect(box.GetLowerLeftX(), box.GetLowerLeftY(), box.GetWidth(), box.GetHeight());
        contents.DrawShape(lineWidth, hasStroke, hasFill);
    }

    private static PDRectangle Shrink(PDRectangle rect, float inset)
    {
        return new PDRectangle(rect.GetLowerLeftX() + inset, rect.GetLowerLeftY() + inset,
            Math.Max(0, rect.GetWidth() - inset * 2), Math.Max(0, rect.GetHeight() - inset * 2));
    }
}
