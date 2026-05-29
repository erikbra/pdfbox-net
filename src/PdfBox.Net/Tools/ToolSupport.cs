namespace PdfBox.Net.Tools;

internal static class ToolSupport
{
    internal static NotSupportedException NotSupported(string toolName)
    {
        return new NotSupportedException($"{toolName} is not yet supported in this PdfBox.Net tools port.");
    }
}
