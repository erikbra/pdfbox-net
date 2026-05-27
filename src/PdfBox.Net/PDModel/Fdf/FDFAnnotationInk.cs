using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationInk : FDFAnnotation
{
    public const string Subtype = "Ink";

    public FDFAnnotationInk()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationInk(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetInkList(IList<float[]> inkList)
    {
        COSArray value = new();
        foreach (float[] item in inkList)
        {
            value.Add(COSArray.Of(item));
        }

        Annot.SetItem(COSName.INKLIST, value);
    }

    public List<float[]>? GetInkList()
    {
        COSArray? array = Annot.GetCOSArray(COSName.INKLIST);
        if (array is null)
        {
            return null;
        }

        List<float[]> result = new(array.Size());
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSArray values)
            {
                result.Add(values.ToFloatArray());
            }
        }

        return result;
    }
}
