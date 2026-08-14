using PdfBox.Net.Examples.Signature;

namespace PdfBox.Net.Examples.Tests.Signature;

public class TestSigUtils
{
    [Fact]
    public void OpenURL_RejectsNonHttpProtocol()
    {
        IOException exception = Assert.Throws<IOException>(
            () => SigUtils.OpenURL("ftp://example.com/test.pdf"));

        Assert.Equal("ftp protocol not supported", exception.Message);
    }
}
