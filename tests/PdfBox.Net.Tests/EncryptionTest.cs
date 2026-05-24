using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Encryption;

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
}
