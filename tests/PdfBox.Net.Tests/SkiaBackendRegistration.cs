using System.Runtime.CompilerServices;
using PdfBox.Net.Rendering;

#pragma warning disable CA2255
internal static class SkiaBackendRegistration
{
    [ModuleInitializer]
    internal static void RegisterSkiaBackend()
    {
        SkiaRenderingBackend.Register();
    }
}
#pragma warning restore CA2255
