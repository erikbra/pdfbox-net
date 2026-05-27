namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDUnderlineAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDUnderlineAppearanceHandler(PDAnnotationUnderline annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        GenerateTextMarkup((PDAnnotationUnderline)Annotation, 0.1f);
    }

    private void GenerateTextMarkup(PDAnnotationTextMarkup annotation, float relativeY)
    {
        float[]? quadPoints = annotation.GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null)
        {
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetLineWidth(1f);
        SetOpacity(contents, annotation.GetConstantOpacity());

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            float startX = quadPoints[i + 4];
            float endX = quadPoints[i + 6];
            float bottomY = Math.Min(quadPoints[i + 5], quadPoints[i + 7]);
            float topY = Math.Max(quadPoints[i + 1], quadPoints[i + 3]);
            float y = bottomY + (topY - bottomY) * relativeY;
            contents.MoveTo(startX, y);
            contents.LineTo(endX, y);
            contents.Stroke();
        }
    }
}
