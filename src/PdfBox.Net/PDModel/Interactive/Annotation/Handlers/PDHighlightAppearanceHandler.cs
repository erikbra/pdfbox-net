namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDHighlightAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDHighlightAppearanceHandler(PDAnnotationHighlight annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationHighlight annotation = (PDAnnotationHighlight)Annotation;
        float[]? quadPoints = annotation.GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null || Color.GetComponents().Length == 0)
        {
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetNonStrokingColor(Color!);
        SetOpacity(contents, annotation.GetConstantOpacity());

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            contents.MoveTo(quadPoints[i + 4], quadPoints[i + 5]);
            contents.LineTo(quadPoints[i + 0], quadPoints[i + 1]);
            contents.LineTo(quadPoints[i + 2], quadPoints[i + 3]);
            contents.LineTo(quadPoints[i + 6], quadPoints[i + 7]);
            contents.ClosePath();
            contents.Fill();
        }
    }
}
