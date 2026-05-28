namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

internal static class PDAppearanceHandlerFactory
{
    internal static PDAppearanceHandler? Create(PDAnnotation annotation, PDDocument? document = null)
    {
        return annotation switch
        {
            PDAnnotationText text => new PDTextAppearanceHandler(text, document),
            PDAnnotationHighlight highlight => new PDHighlightAppearanceHandler(highlight, document),
            PDAnnotationUnderline underline => new PDUnderlineAppearanceHandler(underline, document),
            PDAnnotationStrikeOut strikeOut => new PDStrikeoutAppearanceHandler(strikeOut, document),
            PDAnnotationSquiggly squiggly => new PDSquigglyAppearanceHandler(squiggly, document),
            PDAnnotationSquare square => new PDSquareAppearanceHandler(square, document),
            PDAnnotationCircle circle => new PDCircleAppearanceHandler(circle, document),
            PDAnnotationLine line => new PDLineAppearanceHandler(line, document),
            _ => null
        };
    }
}
