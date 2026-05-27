using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDTextAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDTextAppearanceHandler(PDAnnotationText annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationText annotation = (PDAnnotationText)Annotation;
        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        if (!contents.SetNonStrokingColorOnDemand(annotation.GetColor()))
        {
            contents.SetNonStrokingColor(1f, 1f, 0.6f);
        }

        SetOpacity(contents, annotation.GetConstantOpacity());
        contents.SetLineWidth(0.6f);

        PDRectangle rect = Rectangle;
        float width = rect.GetWidth();
        float height = rect.GetHeight();
        float llx = rect.GetLowerLeftX();
        float lly = rect.GetLowerLeftY();

        contents.AddRect(llx + 1, lly + 1, Math.Max(0, width - 2), Math.Max(0, height - 2));
        contents.FillAndStroke();

        contents.MoveTo(llx + width * 0.25f, lly + height * 0.3f);
        contents.LineTo(llx + width * 0.75f, lly + height * 0.3f);
        contents.MoveTo(llx + width * 0.25f, lly + height * 0.5f);
        contents.LineTo(llx + width * 0.75f, lly + height * 0.5f);
        contents.MoveTo(llx + width * 0.25f, lly + height * 0.7f);
        contents.LineTo(llx + width * 0.75f, lly + height * 0.7f);
        contents.Stroke();
    }
}
