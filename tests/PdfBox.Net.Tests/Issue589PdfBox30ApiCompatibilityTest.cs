using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class Issue589PdfBox30ApiCompatibilityTest
{
    [Fact]
#pragma warning disable CS0618
    public void COSObjectDeprecatedNumberAccessorsForwardToObjectKey()
    {
        COSObject obj = new(COSInteger.Get(7), new COSObjectKey(42, 3));

        Assert.Equal(42, obj.GetObjectNumber());
        Assert.Equal(3, obj.GetGenerationNumber());
    }

    [Fact]
    public void COSContainersCollectIndirectObjectKeysWithoutResettingThem()
    {
        COSDictionary nestedDictionary = new();
        nestedDictionary.SetKey(new COSObjectKey(2, 0));
        nestedDictionary.SetItem(COSName.GetPDFName("Leaf"), new COSObject(COSInteger.Get(7), new COSObjectKey(3, 0)));

        COSArray array = new();
        array.SetKey(new COSObjectKey(1, 0));
        array.Add(nestedDictionary);

        List<COSObjectKey> keys = [];
        array.GetIndirectObjectKeys(keys);

        Assert.Equal([new COSObjectKey(1, 0), new COSObjectKey(2, 0), new COSObjectKey(3, 0)], keys);
        Assert.Equal(new COSObjectKey(1, 0), array.GetKey());
        Assert.Equal(new COSObjectKey(2, 0), nestedDictionary.GetKey());
    }

    [Fact]
    public void DeprecatedColorSpaceArrayConstructorsForwardToResourceAwareConstructors()
    {
        PDFunction tintTransform = CreateType2Function(
            domain: [0f, 1f],
            range: [0f, 1f],
            c0: [1f],
            c1: [0f]);
        PDSeparation separation = new("Spot", PDDeviceGray.Instance, tintTransform);
        PDDeviceN deviceN = new(["Spot"], PDDeviceGray.Instance, tintTransform);

        PDSeparation separationFromArray = new((COSArray)separation.GetCOSObject());
        PDDeviceN deviceNFromArray = new((COSArray)deviceN.GetCOSObject());

        Assert.Equal("Spot", separationFromArray.GetColorantName());
        Assert.Equal(["Spot"], deviceNFromArray.GetColorantNames());
    }
#pragma warning restore CS0618

    [Fact]
    public void XrefStreamParserAcceptsJava30DocumentConstructorArgument()
    {
        using COSDocument document = new();
        COSStream stream = new();

        PDFXRefStream xrefStream = new PDFXrefStreamParser(stream, document).Parse();

        Assert.NotNull(xrefStream);
    }

    private static PDFunctionType2 CreateType2Function(float[] domain, float[] range, float[] c0, float[] c1)
    {
        COSDictionary dictionary = new();
        dictionary.SetInt(COSName.FUNCTION_TYPE, 2);
        dictionary.SetItem(COSName.DOMAIN, COSArray.Of(domain));
        dictionary.SetItem(COSName.RANGE, COSArray.Of(range));
        dictionary.SetItem(COSName.C0, COSArray.Of(c0));
        dictionary.SetItem(COSName.C1, COSArray.Of(c1));
        dictionary.SetInt(COSName.N, 1);
        return new PDFunctionType2(dictionary);
    }
}
