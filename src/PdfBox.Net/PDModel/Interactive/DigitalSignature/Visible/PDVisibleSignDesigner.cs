/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSignDesigner.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDVisibleSignDesigner
{
    private float _xAxis;
    private float _yAxis;
    private float _imageWidth = 100;
    private float _imageHeight = 50;
    private float _pageHeight;
    private float _pageWidth;
    private string _signatureFieldName = "sig";
    private int[] _formatterRectangleParameters = [0, 0, 100, 50];
    private float _imageSizeInPercents = 100;

    public PDVisibleSignDesigner(Stream imageStream)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
    }

    public PDVisibleSignDesigner(PDDocument document, Stream imageStream, int page)
        : this(imageStream)
    {
        CalculatePageSize(document, page);
    }

    public PDVisibleSignDesigner(string filename, Stream imageStream, int page)
        : this(imageStream)
    {
        using PDDocument document = PDDocument.Load(filename);
        CalculatePageSize(document, page);
    }

    private void CalculatePageSize(PDDocument document, int page)
    {
        if (page < 1)
        {
            throw new ArgumentException("First page of pdf is 1", nameof(page));
        }

        PDRectangle mediaBox = document.GetPage(page - 1).GetMediaBox();
        _pageHeight = mediaBox.GetHeight();
        _pageWidth = mediaBox.GetWidth();
    }

    public PDVisibleSignDesigner Coordinates(float x, float y) => XAxis(x).YAxis(y);

    public float GetxAxis() => _xAxis;
    public PDVisibleSignDesigner XAxis(float xAxis) { _xAxis = xAxis; return this; }
    public float GetyAxis() => _yAxis;
    public PDVisibleSignDesigner YAxis(float yAxis) { _yAxis = yAxis; return this; }

    public float GetWidth() => _imageWidth;
    public PDVisibleSignDesigner Width(float width)
    {
        _imageWidth = width;
        _formatterRectangleParameters[2] = (int)width;
        return this;
    }

    public float GetHeight() => _imageHeight;
    public PDVisibleSignDesigner Height(float height)
    {
        _imageHeight = height;
        _formatterRectangleParameters[3] = (int)height;
        return this;
    }

    public float GetTemplateHeight() => _pageHeight;

    public string GetSignatureFieldName() => _signatureFieldName;
    public PDVisibleSignDesigner SignatureFieldName(string signatureFieldName)
    {
        _signatureFieldName = signatureFieldName;
        return this;
    }

    public int[] GetFormatterRectangleParameters() => _formatterRectangleParameters;
    public PDVisibleSignDesigner FormatterRectangleParameters(int[] formatterRectangleParameters)
    {
        _formatterRectangleParameters = formatterRectangleParameters;
        return this;
    }

    public float GetPageWidth() => _pageWidth;
    public PDVisibleSignDesigner PageWidth(float pageWidth) { _pageWidth = pageWidth; return this; }
    public float GetPageHeight() => _pageHeight;

    public float GetImageSizeInPercents() => _imageSizeInPercents;
    public void ImageSizeInPercents(float imageSizeInPercents) => _imageSizeInPercents = imageSizeInPercents;

    public PDVisibleSignDesigner Zoom(float percent)
    {
        _imageHeight += (_imageHeight * percent) / 100;
        _imageWidth += (_imageWidth * percent) / 100;
        _formatterRectangleParameters[2] = (int)_imageWidth;
        _formatterRectangleParameters[3] = (int)_imageHeight;
        return this;
    }
}
