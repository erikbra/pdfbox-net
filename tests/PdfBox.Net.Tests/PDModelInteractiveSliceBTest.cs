/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Regression tests for interactive slice B destinations, outlines,
 * and page navigation structures.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;
using PdfBox.Net.PDModel.Interactive.PageNavigation;

namespace PdfBox.Net.Tests;

public class PDModelInteractiveSliceBTest
{
    [Fact]
    public void OutlineItemFindDestinationPage_ResolvesNamedDestination()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDPage first = new();
        PDPage second = new();
        document.AddPage(first);
        document.AddPage(second);

        PDPageXYZDestination destination = new();
        destination.SetPage(second);

        PDDestinationNameTreeNode destTree = new();
        destTree.SetNames(new Dictionary<string, PDPageDestination>
        {
            ["chapter-2"] = destination
        });

        PDDocumentNameDictionary names = new(catalog);
        names.SetDests(destTree);
        catalog.SetNames(names);

        PDOutlineItem outlineItem = new();
        outlineItem.SetDestination(new PDNamedDestination("chapter-2"));

        PDPage? resolved = outlineItem.FindDestinationPage(document);
        Assert.NotNull(resolved);
        Assert.Equal(second.GetCOSObject(), resolved!.GetCOSObject());
    }

    [Fact]
    public void ThreadAndBeadDictionaryRoundTrip()
    {
        PDThread thread = new();
        PDDocumentInformation info = new();
        info.SetTitle("Article thread");
        thread.SetThreadInfo(info);

        PDThreadBead firstBead = new();
        PDThreadBead secondBead = new();
        firstBead.AppendBead(secondBead);
        thread.SetFirstBead(firstBead);

        PDRectangle rect = new(10, 20, 30, 40);
        firstBead.SetRectangle(rect);
        firstBead.SetPage(new PDPage());

        PDThreadBead? firstFromThread = thread.GetFirstBead();
        Assert.NotNull(firstFromThread);
        Assert.Equal("Article thread", thread.GetThreadInfo()!.GetTitle());
        Assert.NotNull(firstFromThread!.GetRectangle());
        Assert.Equal(firstBead.GetCOSObject(), firstFromThread.GetPreviousBead()!.GetCOSObject());
        Assert.Equal(secondBead.GetCOSObject(), firstFromThread.GetNextBead()!.GetCOSObject());
    }

    [Fact]
    public void PDPageThreadBeadsRoundTrip()
    {
        PDPage page = new();
        PDThreadBead bead = new();
        bead.SetRectangle(new PDRectangle(1, 2, 3, 4));

        page.SetThreadBeads([bead]);
        List<PDThreadBead> restored = page.GetThreadBeads().ToList();

        Assert.Single(restored);
        Assert.NotNull(restored[0].GetRectangle());
    }

    [Fact]
    public void PDPageTransitionRoundTripWithDuration()
    {
        PDPage page = new();
        PDTransition transition = new(PDTransitionStyle.Fly);
        transition.SetDimension(PDTransitionDimension.V);
        transition.SetMotion(PDTransitionMotion.O);
        transition.SetDirection(PDTransitionDirection.TOP_LEFT_TO_BOTTOM_RIGHT);
        transition.SetDuration(2.5f);
        transition.SetFlyScale(1.2f);
        transition.SetFlyAreaOpaque(true);

        page.SetTransition(transition, 3.75f);

        PDTransition? restored = page.GetTransition();
        Assert.NotNull(restored);
        Assert.Equal("Fly", restored!.GetStyle());
        Assert.Equal("V", restored.GetDimension());
        Assert.Equal("O", restored.GetMotion());
        Assert.Equal(2.5f, restored.GetDuration());
        Assert.Equal(1.2f, restored.GetFlyScale());
        Assert.True(restored.IsFlyAreaOpaque());

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        Assert.Equal(3.75f, pageDictionary.GetFloat(COSName.GetPDFName("Dur"), 0));
    }
}
