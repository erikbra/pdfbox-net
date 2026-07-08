using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Resources;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDModelResourceContentStreamGapTest
{
    [Fact]
    public void PDPage_UsesDocumentResourceCache_ForResourceLookupAndEviction()
    {
        using PDDocument document = new();
        RecordingResourceCache cache = new();
        document.SetResourceCache(cache);

        COSName xObjectName = COSName.GetPDFName("X1");
        COSStream xObjectStream = new();
        xObjectStream.SetName(COSName.TYPE, "XObject");
        xObjectStream.SetName(COSName.GetPDFName("Subtype"), "Form");
        COSObject indirect = new(xObjectStream, new COSObjectKey(10, 0));

        COSDictionary xObjectDictionary = new();
        xObjectDictionary.SetItem(xObjectName, indirect);
        COSDictionary resourcesDictionary = new();
        resourcesDictionary.SetItem(COSName.GetPDFName("XObject"), xObjectDictionary);

        PDPage page = new();
        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.RESOURCES, resourcesDictionary);
        document.AddPage(page);

        PDPage readPage = document.GetPage(0);
        PDResources resources = readPage.GetResources()!;
        Assert.NotNull(resources.GetXObject(xObjectName));
        Assert.Equal(1, cache.XObjectPutCount);

        readPage.RemovePageResourceFromCache();
        Assert.Equal(1, cache.XObjectRemoveCount);
    }

    [Fact]
    public void PDDocumentNameDestinationDictionary_ResolvesArrayAndDictionaryDEntry()
    {
        COSDictionary pageDictionary = new();
        pageDictionary.SetItem(COSName.TYPE, COSName.PAGE);

        COSArray destinationArray = new();
        destinationArray.Add(pageDictionary);
        destinationArray.Add(COSName.GetPDFName("Fit"));

        COSDictionary destinationDictionary = new();
        destinationDictionary.SetItem(COSName.D, destinationArray);

        COSDictionary dictionary = new();
        dictionary.SetItem("arrayDest", destinationArray);
        dictionary.SetItem("dictDest", destinationDictionary);

        PDDocumentNameDestinationDictionary nameDestinationDictionary = new(dictionary);
        Assert.IsAssignableFrom<PDPageDestination>(nameDestinationDictionary.GetDestination("arrayDest"));
        Assert.IsAssignableFrom<PDPageDestination>(nameDestinationDictionary.GetDestination("dictDest"));
        Assert.Null(nameDestinationDictionary.GetDestination("missing"));
    }

    [Fact]
    public void PDFormAndPatternContentStreams_CreateMissingResources()
    {
        PDFormXObject form = new(new COSStream());
        form.SetResources(null);
        using (PDFormContentStream content = new(form))
        {
            content.MoveTo(0, 0);
            content.LineTo(10, 10);
            content.Stroke();
        }

        Assert.NotNull(form.GetResources());

        PDTilingPattern pattern = new();
        pattern.SetResources(null);
        using (PDPatternContentStream content = new(pattern))
        {
            content.AddRect(0, 0, 4, 4);
            content.Fill();
        }

        Assert.NotNull(pattern.GetResources());
    }

    private sealed class RecordingResourceCache : ResourceCache
    {
        public int XObjectPutCount { get; private set; }
        public int XObjectRemoveCount { get; private set; }

        private readonly Dictionary<COSObject, PDXObject> _xObjects = [];

        public PDFont? GetFont(COSObject indirect) => null;
        public PDColorSpace? GetColorSpace(COSObject indirect) => null;
        public PDExtendedGraphicsState? GetExtGState(COSObject indirect) => null;
        public PDShading? GetShading(COSObject indirect) => null;
        public PDAbstractPattern? GetPattern(COSObject indirect) => null;
        public PDPropertyList? GetProperties(COSObject indirect) => null;
        public PDXObject? GetXObject(COSObject indirect) => _xObjects.TryGetValue(indirect, out PDXObject? xObject) ? xObject : null;

        public void Put(COSObject indirect, PDFont font)
        {
        }

        public void Put(COSObject indirect, PDColorSpace colorSpace)
        {
        }

        public void Put(COSObject indirect, PDExtendedGraphicsState extGState)
        {
        }

        public void Put(COSObject indirect, PDShading shading)
        {
        }

        public void Put(COSObject indirect, PDAbstractPattern pattern)
        {
        }

        public void Put(COSObject indirect, PDPropertyList propertyList)
        {
        }

        public void Put(COSObject indirect, PDXObject xobject)
        {
            _xObjects[indirect] = xobject;
            XObjectPutCount++;
        }

        public PDColorSpace? RemoveColorSpace(COSObject indirect) => null;
        public PDExtendedGraphicsState? RemoveExtState(COSObject indirect) => null;
        public PDFont? RemoveFont(COSObject indirect) => null;
        public PDShading? RemoveShading(COSObject indirect) => null;
        public PDAbstractPattern? RemovePattern(COSObject indirect) => null;
        public PDPropertyList? RemoveProperties(COSObject indirect) => null;

        public PDXObject? RemoveXObject(COSObject indirect)
        {
            if (_xObjects.Remove(indirect, out PDXObject? xObject))
            {
                XObjectRemoveCount++;
                return xObject;
            }

            return null;
        }
    }
}
