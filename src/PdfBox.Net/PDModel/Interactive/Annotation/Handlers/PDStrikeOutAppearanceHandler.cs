namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDStrikeOutAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDStrikeOutAppearanceHandler(PDAnnotationStrikeOut annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        float[]? quadPoints = ((PDAnnotationStrikeOut)Annotation).GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null)
        {
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetLineWidth(1f);

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            float startX = quadPoints[i + 4];
            float endX = quadPoints[i + 6];
            float bottomY = Math.Min(quadPoints[i + 5], quadPoints[i + 7]);
            float topY = Math.Max(quadPoints[i + 1], quadPoints[i + 3]);
            float y = bottomY + (topY - bottomY) * 0.5f;
            contents.MoveTo(startX, y);
            contents.LineTo(endX, y);
            contents.Stroke();
        }
    }
}
