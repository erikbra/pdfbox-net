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
    public void PDFormXObject_RetainsResourceWrapper_ForDirectNestedFontResources()
    {
        RecordingResourceCache cache = new();
        COSName formName = COSName.GetPDFName("Fm1");
        COSName fontName = COSName.GetPDFName("F1");
        PDResources parentResources = CreateParentResources(
            CreateFormStream(CreateFontResources(fontName, "Helvetica", null)),
            formName,
            cache,
            10);

        PDFormXObject form = Assert.IsType<PDFormXObject>(parentResources.GetXObject(formName));
        Assert.Same(form, parentResources.GetXObject(formName));

        PDResources firstResources = Assert.IsType<PDResources>(form.GetResources());
        PDFont firstFont = Assert.IsAssignableFrom<PDFont>(firstResources.GetFont(fontName));
        PDResources secondResources = Assert.IsType<PDResources>(form.GetResources());
        PDFont secondFont = Assert.IsAssignableFrom<PDFont>(secondResources.GetFont(fontName));

        Assert.Same(firstResources, secondResources);
        Assert.Same(cache, firstResources.GetResourceCache());
        Assert.Same(cache, secondResources.GetResourceCache());
        Assert.Same(firstFont, secondFont);
        Assert.Equal(0, cache.FontPutCount);
    }

    [Fact]
    public void PDTransparencyGroup_PropagatesParentResourceCache_ToNestedFontResources()
    {
        RecordingResourceCache cache = new();
        COSName formName = COSName.GetPDFName("Tr1");
        COSName fontName = COSName.GetPDFName("F1");
        COSStream formStream = CreateFormStream(CreateFontResources(fontName, "Helvetica", 40));
        COSDictionary group = new();
        group.SetItem(COSName.GetPDFName("S"), COSName.GetPDFName("Transparency"));
        formStream.SetItem(COSName.GetPDFName("Group"), group);
        PDResources parentResources = CreateParentResources(formStream, formName, cache, 30);

        PDTransparencyGroup transparencyGroup = Assert.IsType<PDTransparencyGroup>(parentResources.GetXObject(formName));
        PDFont firstFont = Assert.IsAssignableFrom<PDFont>(transparencyGroup.GetResources()!.GetFont(fontName));
        PDFont secondFont = Assert.IsAssignableFrom<PDFont>(transparencyGroup.GetResources()!.GetFont(fontName));

        Assert.Same(cache, transparencyGroup.GetResources()!.GetResourceCache());
        Assert.Same(firstFont, secondFont);
        Assert.Equal(1, cache.FontPutCount);
    }

    [Fact]
    public void PDFormXObject_SetResources_ReplacesDictionaryAndRetainsParentResourceCache()
    {
        RecordingResourceCache cache = new();
        COSName formName = COSName.GetPDFName("Fm1");
        COSName fontName = COSName.GetPDFName("F1");
        COSDictionary originalResources = CreateFontResources(fontName, "Helvetica", 60);
        COSDictionary replacementResources = CreateFontResources(fontName, "Courier", 61);
        PDResources parentResources = CreateParentResources(
            CreateFormStream(originalResources),
            formName,
            cache,
            50);
        PDFormXObject form = Assert.IsType<PDFormXObject>(parentResources.GetXObject(formName));

        PDFont originalFont = Assert.IsAssignableFrom<PDFont>(form.GetResources()!.GetFont(fontName));
        form.SetResources(new PDResources(replacementResources));
        PDResources resourcesAfterReplacement = Assert.IsType<PDResources>(form.GetResources());
        PDFont replacementFont = Assert.IsAssignableFrom<PDFont>(resourcesAfterReplacement.GetFont(fontName));

        Assert.Same(cache, resourcesAfterReplacement.GetResourceCache());
        Assert.NotSame(originalFont, replacementFont);
        Assert.Equal("Courier", replacementFont.GetName());
        Assert.Same(replacementFont, form.GetResources()!.GetFont(fontName));
        Assert.Equal(2, cache.FontPutCount);

        form.SetResources(null);
        Assert.Null(form.GetResources());
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

    private static PDResources CreateParentResources(
        COSStream formStream,
        COSName formName,
        ResourceCache resourceCache,
        long objectNumber)
    {
        COSDictionary xObjects = new();
        xObjects.SetItem(formName, new COSObject(formStream, new COSObjectKey(objectNumber, 0)));
        COSDictionary parentDictionary = new();
        parentDictionary.SetItem(COSName.GetPDFName("XObject"), xObjects);
        return new PDResources(parentDictionary, resourceCache);
    }

    private static COSStream CreateFormStream(COSDictionary resources)
    {
        COSStream stream = new();
        stream.SetName(COSName.TYPE, "XObject");
        stream.SetName(COSName.GetPDFName("Subtype"), "Form");
        stream.SetItem(COSName.RESOURCES, resources);
        return stream;
    }

    private static COSDictionary CreateFontResources(COSName fontName, string baseFont, long? objectNumber)
    {
        COSDictionary font = new();
        font.SetName(COSName.TYPE, "Font");
        font.SetName(COSName.GetPDFName("Subtype"), "Type1");
        font.SetName(COSName.GetPDFName("BaseFont"), baseFont);

        COSDictionary fonts = new();
        fonts.SetItem(
            fontName,
            objectNumber.HasValue ? new COSObject(font, new COSObjectKey(objectNumber.Value, 0)) : font);
        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
    }

    private sealed class RecordingResourceCache : ResourceCache
    {
        public int FontPutCount { get; private set; }
        public int XObjectPutCount { get; private set; }
        public int XObjectRemoveCount { get; private set; }

        private readonly Dictionary<COSObject, PDFont> _fonts = [];
        private readonly Dictionary<COSObject, PDXObject> _xObjects = [];

        public PDFont? GetFont(COSObject indirect) => _fonts.TryGetValue(indirect, out PDFont? font) ? font : null;
        public PDCIDFont? GetCIDFont(COSObject indirect) => null;
        public PDFontDescriptor? GetFontDescriptor(COSObject indirect) => null;
        public PDColorSpace? GetColorSpace(COSObject indirect) => null;
        public PDExtendedGraphicsState? GetExtGState(COSObject indirect) => null;
        public PDShading? GetShading(COSObject indirect) => null;
        public PDAbstractPattern? GetPattern(COSObject indirect) => null;
        public PDPropertyList? GetProperties(COSObject indirect) => null;
        public PDXObject? GetXObject(COSObject indirect) => _xObjects.TryGetValue(indirect, out PDXObject? xObject) ? xObject : null;

        public void Put(COSObject indirect, PDFont font)
        {
            _fonts[indirect] = font;
            FontPutCount++;
        }

        public void Put(COSObject indirect, PDCIDFont cidFont)
        {
        }

        public void Put(COSObject indirect, PDFontDescriptor fontDescriptor)
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
        public PDCIDFont? RemoveCIDFont(COSObject indirect) => null;
        public PDFontDescriptor? RemoveFontDescriptor(COSObject indirect) => null;
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
