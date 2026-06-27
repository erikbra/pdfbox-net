/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/StandardProtectionPolicy.java
 */

namespace PdfBox.Net.PDModel.Encryption;

public sealed partial class StandardProtectionPolicy
{
    public string OwnerPassword
    {
        get => GetOwnerPassword();
        set => SetOwnerPassword(value);
    }

    public AccessPermission Permissions
    {
        get => GetPermissions();
        set => SetPermissions(value);
    }

    public string UserPassword
    {
        get => GetUserPassword();
        set => SetUserPassword(value);
    }
}
