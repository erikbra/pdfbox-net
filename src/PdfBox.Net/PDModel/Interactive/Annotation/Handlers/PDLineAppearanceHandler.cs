namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDLineAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDLineAppearanceHandler(PDAnnotationLine annotation)
        : base(annotation)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationLine annotation = (PDAnnotationLine)Annotation;
        float[]? line = annotation.GetLine();
        if (line == null || line.Length < 4 || Color == null)
        {
            return;
        }

        float lineWidth = ResolveLineWidth(annotation);
        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());
        contents.MoveTo(line[0], line[1]);
        contents.LineTo(line[2], line[3]);
        contents.Stroke();
    }
}
