/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Core FDF document model and load/save smoke tests for issue #69.
 */

using PdfBox.Net;
using PdfBox.Net.COS;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Fdf;
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.Tests;

public class FDFDocumentCoreTest
{
    [Fact]
    public void FDFCoreDictionaryPropertiesRoundTrip()
    {
        using FDFDocument document = new();

        FDFCatalog catalog = document.GetCatalog();
        catalog.SetVersion("1.7");

        FDFDictionary dictionary = catalog.GetFDF();
        dictionary.SetStatus("ready");
        dictionary.SetEncoding("UTF-16LE");

        PDSimpleFileSpecification fileSpecification = new();
        fileSpecification.SetFile("form.pdf");
        dictionary.SetFile(fileSpecification);

        COSArray ids = new();
        ids.Add(new COSString(new byte[] { 0x01, 0x02 }, forceHex: true));
        ids.Add(new COSString(new byte[] { 0x0A, 0x0B }, forceHex: true));
        dictionary.SetID(ids);

        FDFJavaScript javaScript = new();
        javaScript.SetBefore("console.log('before')");
        javaScript.SetAfter("console.log('after')");
        javaScript.SetDoc(new Dictionary<string, PDActionJavaScript>
        {
            ["DocOpen"] = new PDActionJavaScript("app.alert('open')")
        });
        dictionary.SetJavaScript(javaScript);

        FDFNamedPageReference pageReference = new();
        pageReference.SetName("Cover");
        pageReference.SetFileSpecification(fileSpecification);

        Assert.Equal("1.7", catalog.GetVersion());
        Assert.Equal("ready", dictionary.GetStatus());
        Assert.Equal("UTF-16LE", dictionary.GetEncoding());
        Assert.Equal("form.pdf", dictionary.GetFile()?.GetFile());
        Assert.Equal(ids, dictionary.GetID());
        Assert.Equal("console.log('before')", dictionary.GetJavaScript()?.GetBefore());
        Assert.Equal("console.log('after')", dictionary.GetJavaScript()?.GetAfter());
        Assert.True(dictionary.GetJavaScript()?.GetDoc()?.ContainsKey("DocOpen"));
        Assert.Equal("Cover", pageReference.GetName());
        Assert.Equal("form.pdf", pageReference.GetFileSpecification()?.GetFile());
    }

    [Fact]
    public void FixtureBackedLoadAndSaveSmokeTest()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "basic-fdf-fixture.fdf");

        using FDFDocument loaded = Loader.LoadFDF(fixturePath);
        FDFDictionary loadedDictionary = loaded.GetCatalog().GetFDF();

        Assert.Equal("fixture-status", loadedDictionary.GetStatus());
        Assert.Equal("PDFDocEncoding", loadedDictionary.GetEncoding());
        Assert.Equal("before-script", loadedDictionary.GetJavaScript()?.GetBefore());
        Assert.Equal("after-script", loadedDictionary.GetJavaScript()?.GetAfter());

        using MemoryStream output = new();
        loaded.Save(output);
        byte[] saved = output.ToArray();

        Assert.NotEmpty(saved);
        Assert.StartsWith("%FDF-", System.Text.Encoding.ASCII.GetString(saved, 0, 5));

        using FDFDocument reloaded = Loader.LoadFDF(saved);
        FDFDictionary reloadedDictionary = reloaded.GetCatalog().GetFDF();

        Assert.Equal("fixture-status", reloadedDictionary.GetStatus());
        Assert.Equal("before-script", reloadedDictionary.GetJavaScript()?.GetBefore());
    }

    [Fact]
    public void LoaderCanReadFdfFromRandomAccessRead()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "basic-fdf-fixture.fdf");
        byte[] bytes = File.ReadAllBytes(fixturePath);

        using RandomAccessReadBuffer randomAccessRead = new(bytes);
        using FDFDocument loaded = Loader.LoadFDF(randomAccessRead);

        Assert.Equal("fixture-status", loaded.GetCatalog().GetFDF().GetStatus());
    }
}
