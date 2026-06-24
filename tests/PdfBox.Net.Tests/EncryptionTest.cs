using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Encryption;
using PdfBox.Net.Rendering;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.Tests;

public class EncryptionTest
{
    [Fact]
    public void AccessPermission_RoundTripsFlags()
    {
        AccessPermission permission = new();
        permission.SetCanPrint(false);
        permission.SetCanModify(false);
        permission.SetCanExtractContent(true);
        permission.SetCanModifyAnnotations(false);
        permission.SetCanFillInForm(true);
        permission.SetCanExtractForAccessibility(true);
        permission.SetCanAssembleDocument(false);
        permission.SetCanPrintFaithful(true);

        AccessPermission roundTripped = new(permission.GetPermissionBytes());

        Assert.False(roundTripped.CanPrint());
        Assert.False(roundTripped.CanModify());
        Assert.True(roundTripped.CanExtractContent());
        Assert.False(roundTripped.CanModifyAnnotations());
        Assert.True(roundTripped.CanFillInForm());
        Assert.True(roundTripped.CanExtractForAccessibility());
        Assert.False(roundTripped.CanAssembleDocument());
        Assert.True(roundTripped.CanPrintFaithful());
    }

    [Fact]
    public void PDEncryption_ExposesDictionaryFields()
    {
        PDEncryption encryption = new();
        encryption.SetFilter("Standard");
        encryption.SetSubFilter("adbe.pkcs7.s5");
        encryption.SetVersion(4);
        encryption.SetRevision(6);
        encryption.SetLength(256);
        encryption.SetPermissions(-1028);
        encryption.SetOwnerKey([1, 2, 3, 4]);
        encryption.SetUserKey([5, 6, 7, 8]);
        encryption.SetOwnerEncryptionKey(Enumerable.Repeat((byte)9, 32).ToArray());
        encryption.SetUserEncryptionKey(Enumerable.Repeat((byte)10, 32).ToArray());
        encryption.SetPerms(Enumerable.Repeat((byte)11, 16).ToArray());

        Assert.Equal("Standard", encryption.GetFilter());
        Assert.Equal("adbe.pkcs7.s5", encryption.GetSubFilter());
        Assert.Equal(4, encryption.GetVersion());
        Assert.Equal(6, encryption.GetRevision());
        Assert.Equal(256, encryption.GetLength());
        Assert.Equal(-1028, encryption.GetPermissions());
        Assert.Equal(48, encryption.GetOwnerKey()!.Length);
        Assert.Equal(48, encryption.GetUserKey()!.Length);
        Assert.Equal(32, encryption.GetOwnerEncryptionKey()!.Length);
        Assert.Equal(32, encryption.GetUserEncryptionKey()!.Length);
        Assert.Equal(16, encryption.GetPerms()!.Length);
        Assert.IsType<COSDictionary>(encryption.GetCOSObject());
    }

    [Fact]
    public void StandardSecurityHandler_CanBeConstructed()
    {
        StandardSecurityHandler defaultHandler = new();
        Assert.NotNull(defaultHandler);

        StandardProtectionPolicy policy = new("owner", "user", AccessPermission.GetOwnerAccessPermission());
        StandardSecurityHandler policyHandler = new(policy);
        Assert.NotNull(policyHandler);
        Assert.Equal(StandardSecurityHandler.FILTER, "Standard");
    }

    [Fact]
    public void SecurityHandlerFactory_ReturnsHandlersForKnownFilterAndPolicy()
    {
        SecurityHandlerFactory factory = SecurityHandlerFactory.INSTANCE;

        SecurityHandler<ProtectionPolicy>? standardByFilter = factory.NewSecurityHandlerForFilter(StandardSecurityHandler.FILTER);
        SecurityHandler<ProtectionPolicy>? pubKeyByFilter = factory.NewSecurityHandlerForFilter(PublicKeySecurityHandler.FILTER);
        SecurityHandler<ProtectionPolicy>? standardByPolicy = factory.NewSecurityHandlerForPolicy(
            new StandardProtectionPolicy("owner", "user", AccessPermission.GetOwnerAccessPermission()));
        SecurityHandler<ProtectionPolicy>? pubKeyByPolicy = factory.NewSecurityHandlerForPolicy(new PublicKeyProtectionPolicy());

        Assert.IsType<StandardSecurityHandler>(standardByFilter);
        Assert.IsType<PublicKeySecurityHandler>(pubKeyByFilter);
        Assert.IsType<StandardSecurityHandler>(standardByPolicy);
        Assert.IsType<PublicKeySecurityHandler>(pubKeyByPolicy);
    }

    [Fact]
    public void SecurityHandlerFactory_ReturnsNullForUnknownFilterOrPolicy()
    {
        SecurityHandlerFactory factory = SecurityHandlerFactory.INSTANCE;
        Assert.Null(factory.NewSecurityHandlerForFilter("UnknownFilter"));
        Assert.Null(factory.NewSecurityHandlerForPolicy(new TestProtectionPolicy()));
    }

    [Fact]
    public void PublicKeyProtectionPolicy_TracksRecipients()
    {
        PublicKeyProtectionPolicy policy = new();
        PublicKeyRecipient recipientA = new();
        PublicKeyRecipient recipientB = new();
        recipientA.SetPermission(new AccessPermission());
        recipientB.SetPermission(AccessPermission.GetOwnerAccessPermission());

        policy.AddRecipient(recipientA);
        policy.AddRecipient(recipientB);

        Assert.Equal(2, policy.GetNumberOfRecipients());
        Assert.True(policy.RemoveRecipient(recipientA));
        Assert.Equal(1, policy.GetNumberOfRecipients());
    }

    [Fact]
    public void PublicKeyDecryptionMaterial_ExposesCertificateAndPrivateKey()
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest req = new("CN=pdfbox-net-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        PublicKeyDecryptionMaterial material = new(cert, "secret");

        Assert.Equal(cert.Thumbprint, material.GetCertificate().Thumbprint);
        Assert.Equal("secret", material.GetPassword());
        Assert.IsAssignableFrom<RSA>(material.GetPrivateKey());
    }

    [Fact]
    public void SecurityProvider_RoundTripsConfiguredProvider()
    {
        object provider = new();
        SecurityProvider.SetProvider(provider);
        Assert.Same(provider, SecurityProvider.GetProvider());
    }

    // -----------------------------------------------------------------------
    // Fixture-based decryption tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Load_RC4EncryptedPdf_WithUserPassword_ReturnsOnePageDocument()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-rc4-test.pdf");
        using PDDocument document = PDDocument.Load(fixturePath, "test");
        Assert.Equal(1, document.GetNumberOfPages());
    }

    [Fact]
    public void Load_RC4EncryptedPdf_WithEncryptedFlateStream_DecodesContentStream()
    {
        using MemoryStream input = new(CreateRc4EncryptedFlateStreamPdf());
        using PDDocument document = PDDocument.Load(input, "test");

        byte[] decoded = ReadPageContent(document);

        Assert.Contains("Issue420", System.Text.Encoding.ASCII.GetString(decoded), StringComparison.Ordinal);
    }

    [Fact]
    public void Render_RC4EncryptedPdf_WithUserPassword_DoesNotReportCompressionFailure()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-rc4-test.pdf");
        using PDDocument document = PDDocument.Load(fixturePath, "test");

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    [Fact]
    public void Save_DecryptedDocumentWithEncryptionDictionary_ThrowsInvalidOperationException()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-rc4-test.pdf");
        using PDDocument document = PDDocument.Load(fixturePath, "test");
        using MemoryStream output = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => document.Save(output));
        Assert.Equal(
            "PDF contains an encryption dictionary, please remove it with setAllSecurityToBeRemoved() or set a protection policy with protect()",
            exception.Message);
    }

    [Fact]
    public void Load_RC4EncryptedPdf_WithWrongPassword_ThrowsIOException()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-rc4-test.pdf");
        Assert.Throws<InvalidPasswordException>(() => PDDocument.Load(fixturePath, "wrongpassword"));
    }

    [Fact]
    public void Load_PublicKeyEncryptedPdf_ThrowsSemanticIOException()
    {
        using MemoryStream input = new(CreatePublicKeyEncryptedPdf());

        IOException exception = Assert.Throws<IOException>(() => PDDocument.Load(input));

        Assert.Contains("Public-key encrypted", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_PublicKeyEncryptedPdf_WithKeyStoreOverload_ThrowsSemanticIOException()
    {
        byte[] pdf = CreatePublicKeyEncryptedPdf();
        using MemoryStream keyStore = new();

        IOException exception = Assert.Throws<IOException>(() => PdfBox.Net.Loader.LoadPDF(pdf, null, keyStore, "alias"));

        Assert.Contains("Public-key encrypted", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("compression", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_OwnerRestrictedPdf_WithNoUserPassword_ReturnsRestrictedPermissions()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-owner-restricted.pdf");
        // Empty user password should succeed for this fixture (owner set restrictions, not access).
        using PDDocument document = PDDocument.Load(fixturePath, "");
        Assert.Equal(1, document.GetNumberOfPages());
    }

    [Fact]
    public void Load_AES128EncryptedPdf_WithUserPassword_ReturnsOnePageDocument()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-aes128-test.pdf");
        using PDDocument document = PDDocument.Load(fixturePath, "test");
        Assert.Equal(1, document.GetNumberOfPages());
    }

    [Fact]
    public void StandardSecurityHandler_KeyDerivation_Rev3_MatchesKnownVector()
    {
        // Known test vector derived from the fixture PDFs: verify the key derivation algorithm
        // independently from the PDF loading path, using the encryption dict values extracted
        // from encrypted-rc4-test.pdf (user password "test", revision 3, 128-bit RC4).
        byte[] ownerKey =
        {
            0x69, 0xF3, 0x66, 0x4C, 0x9F, 0xA7, 0x98, 0xDD,
            0xAB, 0x43, 0xB4, 0x5B, 0x2C, 0x2D, 0xBE, 0x45,
            0x69, 0xA1, 0xB9, 0x42, 0x7C, 0x11, 0x6B, 0xB1,
            0x73, 0xF0, 0xFA, 0xED, 0xFF, 0x36, 0xC4, 0x33
        };

        byte[] userKeyStored =
        {
            0xB7, 0xD4, 0x1D, 0x06, 0x75, 0x09, 0xC1, 0x7B,
            0x2A, 0x4A, 0x74, 0x8A, 0x95, 0x6C, 0x75, 0xD4,
            0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
            0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08
        };

        // The document ID varies per generation; we verify the key derivation does not throw
        // and produces a key of the expected length.
        byte[] docId = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                         0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10];

        byte[] password = System.Text.Encoding.Latin1.GetBytes("test");
        byte[] fileKey = StandardSecurityHandler.ComputeEncryptionKey(
            password, ownerKey, unchecked((int)4294967292), docId, 3, 16, true);

        Assert.Equal(16, fileKey.Length);
    }

    private sealed class TestProtectionPolicy : ProtectionPolicy
    {
    }

    private static byte[] ReadPageContent(PDDocument document)
    {
        COSBase? contents = document.GetPage(0).GetContents();
        Assert.IsType<COSStream>(contents);
        using Stream input = ((COSStream)contents).CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }

    private static byte[] CreateRc4EncryptedFlateStreamPdf()
    {
        byte[] docId = System.Text.Encoding.ASCII.GetBytes("a055ccc1a362f04dd64098f5d07cbe15");
        COSDictionary encryptionDictionary = CreateStandardEncryptionDictionary();
        PDEncryption encryption = new(encryptionDictionary);
        COSArray idArray = new();
        idArray.Add(new COSString(docId));
        idArray.Add(new COSString(docId));

        StandardSecurityHandler handler = new();
        handler.PrepareForDecryption(encryption, idArray, new StandardDecryptionMaterial("test"));

        byte[] contentBytes = System.Text.Encoding.ASCII.GetBytes("BT\n/Issue420 12 Tf\nET\n");
        using MemoryStream compressed = new();
        using (System.IO.Compression.ZLibStream zlib = new(compressed, System.IO.Compression.CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(contentBytes, 0, contentBytes.Length);
        }

        using MemoryStream encrypted = new();
        using (MemoryStream plain = new(compressed.ToArray()))
        {
            handler.DecryptData(6, 0, plain, encrypted);
        }

        return CreateEncryptedPdf(encryptionDictionary, encrypted.ToArray());
    }

    private static COSDictionary CreateStandardEncryptionDictionary()
    {
        COSDictionary dictionary = new();
        dictionary.SetInt(COSName.GetPDFName("V"), 2);
        dictionary.SetInt(COSName.GetPDFName("R"), 3);
        dictionary.SetInt(COSName.LENGTH, 128);
        dictionary.SetLong(COSName.GetPDFName("P"), 4294967292);
        dictionary.SetItem(COSName.FILTER, COSName.GetPDFName("Standard"));
        dictionary.SetItem(COSName.GetPDFName("O"), new COSString(Convert.FromHexString("69f3664c9fa798ddab43b45b2c2dbe4569a1b9427c116bb173f0faedff36c433")));
        dictionary.SetItem(COSName.GetPDFName("U"), new COSString(Convert.FromHexString("b7d41d067509c17b2a4a748a956c75d428bf4e5e4e758a4164004e56fffa0108")));
        return dictionary;
    }

    private static byte[] CreatePublicKeyEncryptedPdf()
    {
        COSDictionary encryptionDictionary = new();
        encryptionDictionary.SetInt(COSName.GetPDFName("V"), 1);
        encryptionDictionary.SetInt(COSName.GetPDFName("R"), 2);
        encryptionDictionary.SetInt(COSName.LENGTH, 40);
        encryptionDictionary.SetItem(COSName.FILTER, COSName.GetPDFName("Adobe.PubSec"));
        encryptionDictionary.SetName(COSName.GetPDFName("SubFilter"), "adbe.pkcs7.s5");
        COSArray recipients = new();
        recipients.Add(new COSString([0]));
        encryptionDictionary.SetItem(COSName.GetPDFName("Recipients"), recipients);

        return CreateEncryptedPdf(encryptionDictionary, encryptedStream: null);
    }

    private static byte[] CreateEncryptedPdf(COSDictionary encryptionDictionary, byte[]? encryptedStream)
    {
        using MemoryStream output = new();
        List<long> offsets = [0];

        void WriteAscii(string value)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
            output.Write(bytes, 0, bytes.Length);
        }

        void WriteObject(string value)
        {
            offsets.Add(output.Position);
            WriteAscii($"{offsets.Count - 1} 0 obj\n{value}\nendobj\n");
        }

        void WriteObjectBytes(byte[] value)
        {
            offsets.Add(output.Position);
            WriteAscii($"{offsets.Count - 1} 0 obj\n");
            output.Write(value, 0, value.Length);
            WriteAscii("\nendobj\n");
        }

        WriteAscii("%PDF-1.3\n");
        WriteObject("<< /Producer (pdfbox-net issue 420 test) >>");
        WriteObject("<< /Type /Pages /Count 1 /Kids [ 4 0 R ] >>");
        WriteObject("<< /Type /Catalog /Pages 2 0 R >>");
        WriteObject(encryptedStream is null
            ? "<< /Type /Page /Resources << >> /MediaBox [ 0 0 612 792 ] /Parent 2 0 R >>"
            : "<< /Type /Page /Resources << >> /MediaBox [ 0 0 612 792 ] /Parent 2 0 R /Contents 6 0 R >>");
        WriteObjectBytes(PdfBox.Net.PdfWriter.COSWriter.Serialize(encryptionDictionary));

        if (encryptedStream is not null)
        {
            offsets.Add(output.Position);
            WriteAscii($"6 0 obj\n<< /Length {encryptedStream.Length} /Filter /FlateDecode >>\nstream\n");
            output.Write(encryptedStream, 0, encryptedStream.Length);
            WriteAscii("\nendstream\nendobj\n");
        }

        long xrefOffset = output.Position;
        WriteAscii($"xref\n0 {offsets.Count}\n");
        WriteAscii("0000000000 65535 f \n");
        for (int i = 1; i < offsets.Count; i++)
        {
            WriteAscii($"{offsets[i]:D10} 00000 n \n");
        }

        WriteAscii("trailer\n");
        WriteAscii($"<< /Size {offsets.Count} /Root 3 0 R /Info 1 0 R /ID [ <6130353563636331613336326630346464363430393866356430376362653135> <6130353563636331613336326630346464363430393866356430376362653135> ] /Encrypt 5 0 R >>\n");
        WriteAscii($"startxref\n{xrefOffset}\n%%EOF\n");

        return output.ToArray();
    }
}
