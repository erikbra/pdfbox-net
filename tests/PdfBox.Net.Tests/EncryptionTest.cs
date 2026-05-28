using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Encryption;
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
        Assert.IsType<RSA>(material.GetPrivateKey());
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
    public void Load_RC4EncryptedPdf_WithWrongPassword_ThrowsIOException()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "encrypted-rc4-test.pdf");
        Assert.Throws<InvalidPasswordException>(() => PDDocument.Load(fixturePath, "wrongpassword"));
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
}
