namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDSquigglyAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDSquigglyAppearanceHandler(PDAnnotationSquiggly annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        float[]? quadPoints = ((PDAnnotationSquiggly)Annotation).GetQuadPoints();
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
            float baseY = Math.Min(quadPoints[i + 5], quadPoints[i + 7]) + 1f;
            float step = Math.Max(2f, (endX - startX) / 8f);

            contents.MoveTo(startX, baseY);
            bool up = true;
            for (float x = startX + step; x <= endX; x += step)
            {
                contents.LineTo(Math.Min(x, endX), baseY + (up ? 1.5f : -1.5f));
                up = !up;
            }
            contents.Stroke();
        }
    }
}
