using PdfBox.Net.COS;
namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationText : FDFAnnotation
{
    private static readonly COSName StateName = COSName.GetPDFName("State");
    private static readonly COSName StateModelName = COSName.GetPDFName("StateModel");

    public const string Subtype = "Text";

    public FDFAnnotationText()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationText(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetIcon(string? icon) => Annot.SetName(COSName.NAME, icon);

    public string GetIcon() => Annot.GetNameAsString(COSName.NAME, "Note");

    public string? GetState() => Annot.GetString(StateName);

    public void SetState(string? state) => Annot.SetString(StateName, state);

    public string? GetStateModel() => Annot.GetString(StateModelName);

    public void SetStateModel(string? stateModel) => Annot.SetString(StateModelName, stateModel);
}
