using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDCircleAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDCircleAppearanceHandler(PDAnnotationCircle annotation, PDDocument? document = null)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationCircle annotation = (PDAnnotationCircle)Annotation;
        float lineWidth = ResolveLineWidth(annotation);

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        bool hasStroke = contents.SetStrokingColorOnDemand(Color);
        bool hasFill = contents.SetNonStrokingColorOnDemand(annotation.GetInteriorColor());
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());

        PDRectangle box = new(
            Rectangle.GetLowerLeftX() + lineWidth / 2f,
            Rectangle.GetLowerLeftY() + lineWidth / 2f,
            Math.Max(0, Rectangle.GetWidth() - lineWidth),
            Math.Max(0, Rectangle.GetHeight() - lineWidth));
        DrawCircle(contents, box);
        contents.DrawShape(lineWidth, hasStroke, hasFill);
    }
}
