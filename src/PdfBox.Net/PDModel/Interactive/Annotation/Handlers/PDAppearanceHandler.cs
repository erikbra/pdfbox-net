namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public interface PDAppearanceHandler
{
    void GenerateAppearanceStreams()
    {
        GenerateNormalAppearance();
        GenerateRolloverAppearance();
        GenerateDownAppearance();
    }

    void GenerateNormalAppearance();

    void GenerateRolloverAppearance();

    void GenerateDownAppearance();
}
