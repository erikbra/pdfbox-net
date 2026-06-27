using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.Measurement;

namespace PdfBox.Net.Tests;

public class Issue507AnnotationFormApiParityTest
{
    [Fact]
    public void AnnotationMarkupMembersRoundTrip()
    {
        PDAnnotationText annotation = new();
        PDAnnotationPopup popup = new();
        PDAnnotationText parent = new();
        DateTimeOffset created = new(2026, 6, 27, 1, 2, 3, TimeSpan.Zero);
        PDExternalDataDictionary externalData = new();
        externalData.SetSubtype("Markup3D");

        annotation.SetPopup(popup);
        annotation.SetRichContents("<p>rich</p>");
        annotation.SetCreationDate(created);
        annotation.SetInReplyTo(parent);
        annotation.SetSubject("subject");
        annotation.SetReplyType(PDAnnotationMarkup.RT_GROUP);
        annotation.SetIntent(PDAnnotationLine.IT_LINE_DIMENSION);
        annotation.SetExternalData(externalData);

        Assert.NotNull(annotation.GetPopup());
        Assert.Equal("<p>rich</p>", annotation.GetRichContents());
        Assert.Equal(created, annotation.GetCreationDate());
        Assert.IsType<PDAnnotationText>(annotation.GetInReplyTo());
        Assert.Equal("subject", annotation.GetSubject());
        Assert.Equal(PDAnnotationMarkup.RT_GROUP, annotation.GetReplyType());
        Assert.Equal(PDAnnotationLine.IT_LINE_DIMENSION, annotation.GetIntent());
        Assert.Equal("Markup3D", annotation.GetExternalData()!.GetSubtype());
        Assert.Equal("ExData", annotation.GetExternalData()!.GetTypeName());
    }

    [Fact]
    public void LineFreeTextCaretAndPolygonMembersRoundTrip()
    {
        PDColor rgb = new([0.2f, 0.4f, 0.6f], PDDeviceRGB.Instance);
        PDBorderEffectDictionary borderEffect = new();
        borderEffect.SetStyle(PDBorderEffectDictionary.STYLE_CLOUDY);
        borderEffect.SetIntensity(1.25f);

        PDAnnotationLine line = new();
        line.SetStartPointEndingStyle(PDAnnotationLine.LE_OPEN_ARROW);
        line.SetEndPointEndingStyle(PDAnnotationLine.LE_CLOSED_ARROW);
        line.SetInteriorColor(rgb);
        line.SetCaption(true);
        line.SetLeaderLineLength(4);
        line.SetLeaderLineExtensionLength(5);
        line.SetLeaderLineOffsetLength(6);
        line.SetCaptionPositioning("Top");
        line.SetCaptionHorizontalOffset(7);
        line.SetCaptionVerticalOffset(8);

        float[]? lineCoordinates = line.GetLine();
        Assert.NotNull(lineCoordinates);
        Assert.Equal([0f, 0f, 0f, 0f], lineCoordinates);
        Assert.Equal(PDAnnotationLine.LE_OPEN_ARROW, line.GetStartPointEndingStyle());
        Assert.Equal(PDAnnotationLine.LE_CLOSED_ARROW, line.GetEndPointEndingStyle());
        Assert.Equal([0.2f, 0.4f, 0.6f], line.GetInteriorColor()!.GetComponents());
        Assert.True(line.HasCaption());
        Assert.Equal(4, line.GetLeaderLineLength());
        Assert.Equal(5, line.GetLeaderLineExtensionLength());
        Assert.Equal(6, line.GetLeaderLineOffsetLength());
        Assert.Equal("Top", line.GetCaptionPositioning());
        Assert.Equal(7, line.GetCaptionHorizontalOffset());
        Assert.Equal(8, line.GetCaptionVerticalOffset());

        PDAnnotationFreeText freeText = new();
        freeText.SetDefaultAppearance("/Helv 12 Tf 0 g");
        freeText.SetDefaultStyleString("font: Helvetica");
        freeText.SetQ(2);
        freeText.SetRectDifferences(1, 2, 3, 4);
        freeText.SetCallout([10, 20, 30, 40, 50, 60]);
        freeText.SetLineEndingStyle(PDAnnotationLine.LE_OPEN_ARROW);
        freeText.SetBorderEffect(borderEffect);

        Assert.Equal("/Helv 12 Tf 0 g", freeText.GetDefaultAppearance());
        Assert.Equal("font: Helvetica", freeText.GetDefaultStyleString());
        Assert.Equal(2, freeText.GetQ());
        Assert.Equal([1f, 2f, 3f, 4f], freeText.GetRectDifferences());
        float[]? callout = freeText.GetCallout();
        Assert.NotNull(callout);
        Assert.Equal([10f, 20f, 30f, 40f, 50f, 60f], callout);
        Assert.Equal(PDAnnotationLine.LE_OPEN_ARROW, freeText.GetLineEndingStyle());
        Assert.Equal(PDBorderEffectDictionary.STYLE_CLOUDY, freeText.GetBorderEffect()!.GetStyle());

        freeText.SetRectDifference(new PDRectangle(1, 2, 3, 4));
        Assert.Equal(1, freeText.GetRectDifference()!.GetLowerLeftX());
        Assert.Equal(6, freeText.GetRectDifference()!.GetUpperRightY());

        PDAnnotationCaret caret = new();
        caret.SetRectDifferences(9);
        Assert.Equal([9f, 9f, 9f, 9f], caret.GetRectDifferences());

        PDAnnotationPolygon polygon = new();
        polygon.SetInteriorColor(rgb);
        polygon.SetBorderEffect(borderEffect);
        COSArray path = new();
        path.Add(COSArray.Of(1, 2));
        path.Add(COSArray.Of(3, 4, 5, 6, 7, 8));
        ((COSDictionary)polygon.GetCOSObject()).SetItem(COSName.GetPDFName("Path"), path);

        Assert.Equal([0.2f, 0.4f, 0.6f], polygon.GetInteriorColor()!.GetComponents());
        Assert.Equal(PDBorderEffectDictionary.STYLE_CLOUDY, polygon.GetBorderEffect()!.GetStyle());
        Assert.Equal([1f, 2f], polygon.GetPath()![0]);
        Assert.Equal([3f, 4f, 5f, 6f, 7f, 8f], polygon.GetPath()![1]);

        PDAnnotationPolyline polyline = new();
        polyline.SetStartPointEndingStyle(PDAnnotationLine.LE_DIAMOND);
        polyline.SetEndPointEndingStyle(PDAnnotationLine.LE_SLASH);
        polyline.SetInteriorColor(rgb);

        Assert.Equal(PDAnnotationLine.LE_DIAMOND, polyline.GetStartPointEndingStyle());
        Assert.Equal(PDAnnotationLine.LE_SLASH, polyline.GetEndPointEndingStyle());
        Assert.Equal([0.2f, 0.4f, 0.6f], polyline.GetInteriorColor()!.GetComponents());
    }

    [Fact]
    public void LinkTextFileWidgetAndAppearanceMembersRoundTrip()
    {
        PDAnnotationFileAttachment attachment = new();
        attachment.SetAttachmentName(PDAnnotationFileAttachment.ATTACHMENT_NAME_PAPERCLIP);
        Assert.Equal(PDAnnotationFileAttachment.ATTACHMENT_NAME_PAPERCLIP, attachment.GetAttachmentName());

        PDAnnotationLink link = new();
        PDActionURI previousUri = new();
        previousUri.SetURI("https://example.invalid/previous");
        link.SetPreviousURI(previousUri);

        Assert.Equal(PDAnnotationLink.HIGHLIGHT_MODE_INVERT, link.GetHighlightMode());
        Assert.Equal("https://example.invalid/previous", link.GetPreviousURI()!.GetURI());

        PDAnnotationText text = new();
        text.SetName(PDAnnotationText.NAME_STAR);
        text.SetState("Accepted");
        text.SetStateModel("Review");

        Assert.Equal(PDAnnotationText.NAME_STAR, text.GetName());
        Assert.Equal("Accepted", text.GetState());
        Assert.Equal("Review", text.GetStateModel());

        PDAnnotationWidget widget = new();
        Assert.Equal(PDAnnotationLink.HIGHLIGHT_MODE_INVERT, widget.GetHighlightingMode());
        widget.SetHighlightingMode("T");
        Assert.Equal("T", widget.GetHighlightingMode());
        Assert.Throws<ArgumentException>(() => widget.SetHighlightingMode("bad"));

        COSDictionary appearanceDictionary = new();
        appearanceDictionary.SetItem(COSName.GetPDFName("I"), new COSStream());
        appearanceDictionary.SetItem(COSName.GetPDFName("RI"), new COSStream());
        appearanceDictionary.SetItem(COSName.GetPDFName("IX"), new COSStream());
        PDAppearanceCharacteristicsDictionary appearance = new(appearanceDictionary);

        Assert.NotNull(appearance.GetNormalIcon());
        Assert.NotNull(appearance.GetRolloverIcon());
        Assert.NotNull(appearance.GetAlternateIcon());
    }

    [Fact]
    public void NonTerminalFieldPushButtonAndMeasurementMembersRoundTrip()
    {
        using PDDocument document = new();
        PDAcroForm form = new(document);
        PDNonTerminalField nonTerminal = new(form);
        nonTerminal.SetReadOnly(true);
        nonTerminal.SetValue(new COSString("cos"));
        nonTerminal.SetDefaultValue(new COSString("default"));

        Assert.Equal(1, nonTerminal.GetFieldFlags() & 1);
        Assert.Equal("cos", ((COSString)nonTerminal.GetValue()!).GetString());
        Assert.Equal("default", ((COSString)nonTerminal.GetDefaultValue()!).GetString());
        Assert.Empty(nonTerminal.GetWidgets());

        nonTerminal.SetValue("plain");
        Assert.Equal("plain", ((COSString)nonTerminal.GetValue()!).GetString());
        Assert.Contains("plain", nonTerminal.GetValueAsString(), StringComparison.Ordinal);

        PDPushButton button = new(form);
        Assert.Equal(string.Empty, button.GetValueAsString());

        Assert.Equal("Measure", new PDMeasureDictionary(new COSDictionary()).GetTypeName());
        Assert.Equal("NumberFormat", new PDNumberFormatDictionary().GetTypeName());
        Assert.Equal("Viewport", new PDViewportDictionary().GetTypeName());
    }
}
