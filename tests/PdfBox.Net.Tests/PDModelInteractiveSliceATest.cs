/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Regression tests for interactive slice A utilities, names dictionaries,
 * and viewer preferences.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.ViewerPreferences;

namespace PdfBox.Net.Tests;

public class PDModelInteractiveSliceATest
{
    [Fact]
    public void ViewerPreferences_RoundTrip_ThroughCatalog()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDViewerPreferences preferences = new();
        preferences.SetHideToolbar(true);
        preferences.SetDisplayDocTitle(true);
        preferences.SetReadingDirection(PDViewerPreferences.ReadingDirection.R2L);
        preferences.SetPrintScaling(PDViewerPreferences.PrintScaling.None);
        preferences.SetViewArea(PDViewerPreferences.Boundary.ArtBox);

        catalog.SetViewerPreferences(preferences);

        PDViewerPreferences? restored = catalog.GetViewerPreferences();
        Assert.NotNull(restored);
        Assert.True(restored!.HideToolbar());
        Assert.True(restored.DisplayDocTitle());
        Assert.Equal(PDViewerPreferences.ReadingDirection.R2L, restored.GetReadingDirection());
        Assert.Equal(PDViewerPreferences.PrintScaling.None, restored.GetPrintScaling());
        Assert.Equal(PDViewerPreferences.Boundary.ArtBox, restored.GetViewArea());
    }

    [Fact]
    public void NameDictionary_EmbeddedFiles_RoundTrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDDocumentNameDictionary names = new(catalog);
        PDEmbeddedFilesNameTreeNode embeddedFiles = new();

        PDComplexFileSpecification fileSpecification = new();
        fileSpecification.SetFile("a.txt");
        embeddedFiles.SetNames(new Dictionary<string, PDComplexFileSpecification>
        {
            ["a.txt"] = fileSpecification
        });

        names.SetEmbeddedFiles(embeddedFiles);
        catalog.SetNames(names);

        PDDocumentNameDictionary? restoredNames = catalog.GetNames();
        Assert.NotNull(restoredNames);

        PDEmbeddedFilesNameTreeNode? restoredEmbeddedFiles = restoredNames!.GetEmbeddedFiles();
        Assert.NotNull(restoredEmbeddedFiles);

        PDComplexFileSpecification? restoredSpec = restoredEmbeddedFiles!.GetValue("a.txt");
        Assert.NotNull(restoredSpec);
        Assert.Equal("a.txt", restoredSpec!.GetFile());
    }

    [Fact]
    public void FindNamedDestinationPage_UsesNameTree()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDPage page = new();
        document.AddPage(page);

        PDPageXYZDestination destination = new();
        destination.SetPage(page);

        PDDestinationNameTreeNode destTree = new();
        destTree.SetNames(new Dictionary<string, PDPageDestination>
        {
            ["first"] = destination
        });

        PDDocumentNameDictionary names = new(catalog);
        names.SetDests(destTree);
        catalog.SetNames(names);

        PDPageDestination? resolved = catalog.FindNamedDestinationPage(new PDNamedDestination("first"));
        Assert.NotNull(resolved);
        Assert.NotNull(resolved!.GetPage());
    }
}
