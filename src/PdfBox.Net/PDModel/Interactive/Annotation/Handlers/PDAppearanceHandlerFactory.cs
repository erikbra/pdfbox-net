namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

internal static class PDAppearanceHandlerFactory
{
    internal static PDAppearanceHandler? Create(PDAnnotation annotation)
    {
        return annotation switch
        {
            PDAnnotationText text => new PDTextAppearanceHandler(text),
            PDAnnotationHighlight highlight => new PDHighlightAppearanceHandler(highlight),
            PDAnnotationUnderline underline => new PDUnderlineAppearanceHandler(underline),
            PDAnnotationStrikeOut strikeOut => new PDStrikeOutAppearanceHandler(strikeOut),
            PDAnnotationSquiggly squiggly => new PDSquigglyAppearanceHandler(squiggly),
            PDAnnotationSquare square => new PDSquareAppearanceHandler(square),
            PDAnnotationCircle circle => new PDCircleAppearanceHandler(circle),
            PDAnnotationLine line => new PDLineAppearanceHandler(line),
            _ => null
        };
    }
}
