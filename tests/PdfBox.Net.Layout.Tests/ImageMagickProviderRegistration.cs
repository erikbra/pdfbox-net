using System.Runtime.CompilerServices;
using PdfBox.Net.ImageMagick;

#pragma warning disable CA2255
internal static class ImageMagickProviderRegistration
{
    [ModuleInitializer]
    internal static void RegisterImageMagickProvider()
    {
        PdfBoxNetImageMagick.Register();
    }
}
#pragma warning restore CA2255
