using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationText : FDFAnnotation
{
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

    public string GetIcon() => Annot.GetNameAsString(COSName.NAME, PDAnnotationText.NAME_NOTE);

    public string? GetState() => Annot.GetString(COSName.STATE);

    public void SetState(string? state) => Annot.SetString(COSName.STATE, state);

    public string? GetStateModel() => Annot.GetString(COSName.STATE_MODEL);

    public void SetStateModel(string? stateModel) => Annot.SetString(COSName.STATE_MODEL, stateModel);
}
