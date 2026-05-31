using System.Reflection;
using PdfBox.Net.COS;

namespace PdfBox.Net.Tests;

public class PortLocalAdapterTest
{
    [Fact]
    public void COSArray_InternalCountAdapter_MatchesSize()
    {
        COSArray array = new();
        array.Add(COSInteger.ZERO);
        array.Add(COSInteger.ONE);

        AssertInternalCountMatchesSize(array);
    }

    [Fact]
    public void COSDictionary_InternalCountAdapter_MatchesSize()
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.TYPE, COSName.CATALOG);
        dictionary.SetItem(COSName.PAGES, new COSDictionary());

        AssertInternalCountMatchesSize(dictionary);
    }

    private static void AssertInternalCountMatchesSize(object instance)
    {
        PropertyInfo? countProperty = instance.GetType().GetProperty(
            "Count",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(countProperty);
        Assert.Equal(typeof(int), countProperty.PropertyType);

        MethodInfo? sizeMethod = instance.GetType().GetMethod("Size", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(sizeMethod);

        int size = Assert.IsType<int>(sizeMethod.Invoke(instance, null));
        int count = Assert.IsType<int>(countProperty.GetValue(instance));

        Assert.Equal(size, count);
    }
}
