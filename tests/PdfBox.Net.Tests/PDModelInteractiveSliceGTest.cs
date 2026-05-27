/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Regression tests for interactive slice G digital signatures,
 * visible signatures, and measurement dictionaries.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.Measurement;

namespace PdfBox.Net.Tests;

public class PDModelInteractiveSliceGTest
{
    [Fact]
    public void SignatureDictionary_RoundTripProperties()
    {
        PDSignature signature = new();
        signature.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
        signature.SetSubFilter(PDSignature.SUBFILTER_ADBE_PKCS7_DETACHED);
        signature.SetName("Signer");
        signature.SetLocation("Oslo");
        signature.SetReason("Approval");
        signature.SetContactInfo("signer@example.com");
        signature.SetByteRange([0, 20, 40, 10]);

        Assert.Equal("Adobe.PPKLite", signature.GetFilter());
        Assert.Equal("adbe.pkcs7.detached", signature.GetSubFilter());
        Assert.Equal("Signer", signature.GetName());
        Assert.Equal("Oslo", signature.GetLocation());
        Assert.Equal("Approval", signature.GetReason());
        Assert.Equal("signer@example.com", signature.GetContactInfo());
        Assert.Equal([0, 20, 40, 10], signature.GetByteRange());
    }

    [Fact]
    public void SeedValue_DigestValidationAndTimestamp()
    {
        PDSeedValue seed = new();
        seed.SetDigestMethod(["SHA256", "SHA512"]);
        PDSeedValueTimeStamp timestamp = new();
        timestamp.SetURL("https://tsa.example");
        timestamp.SetTimestampRequired(true);
        seed.SetTimeStamp(timestamp);

        Assert.Equal(["SHA256", "SHA512"], seed.GetDigestMethod());
        Assert.True(seed.GetTimeStamp()!.IsTimestampRequired());
        Assert.Equal("https://tsa.example", seed.GetTimeStamp()!.GetURL());

        Assert.Throws<ArgumentException>(() => seed.SetDigestMethod(["MD5"]));
    }

    [Fact]
    public void SignatureField_CanStoreSignatureAndSeedValue()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = new(document);
        PDSignatureField field = new(acroForm);

        PDSignature signature = new();
        signature.SetName("Signed");
        field.SetValue(signature);

        PDSeedValue seed = new();
        seed.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
        field.SetSeedValue(seed);

        Assert.Equal("Signed", field.GetSignature()!.GetName());
        Assert.Equal("Adobe.PPKLite", field.GetSeedValue()!.GetFilter());
    }

    [Fact]
    public void MeasurementDictionaries_RoundTripValues()
    {
        PDNumberFormatDictionary numberFormat = new();
        numberFormat.SetUnits("cm");
        numberFormat.SetConversionFactor(2.54f);

        PDRectlinearMeasureDictionary rectlinear = new();
        rectlinear.SetScaleRatio("1 in = 2.54 cm");
        rectlinear.SetDistances([numberFormat]);
        rectlinear.SetCoordSystemOrigin([10, 20]);

        Assert.Equal("1 in = 2.54 cm", rectlinear.GetScaleRatio());
        Assert.Single(rectlinear.GetDistances()!);
        Assert.Equal("cm", rectlinear.GetDistances()![0].GetUnits());
        Assert.Equal([10f, 20f], rectlinear.GetCoordSystemOrigin()!);

        PDViewportDictionary viewport = new();
        viewport.SetName("view-1");
        viewport.SetMeasure(rectlinear);

        Assert.Equal("view-1", viewport.GetName());
        Assert.Equal("RL", viewport.GetMeasure()!.GetSubtype());
    }

    [Fact]
    public void VisibleSignatureProperties_BuildsTemplateStream()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());

        using MemoryStream imageStream = new([1, 2, 3, 4]);
        PDVisibleSignDesigner designer = new PDVisibleSignDesigner(document, imageStream, 1)
            .SignatureFieldName("sigVisible")
            .Coordinates(10, 20)
            .Width(120)
            .Height(60);

        PDVisibleSigProperties properties = new PDVisibleSigProperties()
            .SetPdVisibleSignature(designer)
            .SignerName("Signer")
            .VisualSignEnabled(true)
            .Page(1);

        properties.BuildSignature();

        using Stream visibleStream = properties.GetVisibleSignature();
        Assert.True(visibleStream.Length > 0);

        using SignatureOptions options = new();
        options.SetVisualSignature(properties);
        Assert.NotNull(options.GetVisualSignature());
    }
}
