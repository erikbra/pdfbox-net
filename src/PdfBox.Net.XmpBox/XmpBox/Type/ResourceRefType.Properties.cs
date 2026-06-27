/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/ResourceRefType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class ResourceRefType
{
    public string? DocumentID
    {
        get => GetDocumentID();
        set => SetDocumentID(value!);
    }

    public string? FilePath
    {
        get => GetFilePath();
        set => SetFilePath(value!);
    }

    public string? FromPart
    {
        get => GetFromPart();
        set => SetFromPart(value!);
    }

    public string? InstanceID
    {
        get => GetInstanceID();
        set => SetInstanceID(value!);
    }

    public string? ManageTo
    {
        get => GetManageTo();
        set => SetManageTo(value!);
    }

    public string? ManageUI
    {
        get => GetManageUI();
        set => SetManageUI(value!);
    }

    public string? Manager
    {
        get => GetManager();
        set => SetManager(value!);
    }

    public string? ManagerVariant
    {
        get => GetManagerVariant();
        set => SetManagerVariant(value!);
    }

    public string? MaskMarkers
    {
        get => GetMaskMarkers();
        set => SetMaskMarkers(value!);
    }

    public string? PartMapping
    {
        get => GetPartMapping();
        set => SetPartMapping(value!);
    }

    public string? RenditionClass
    {
        get => GetRenditionClass();
        set => SetRenditionClass(value!);
    }

    public string? RenditionParams
    {
        get => GetRenditionParams();
        set => SetRenditionParams(value!);
    }

    public string? ToPart
    {
        get => GetToPart();
        set => SetToPart(value!);
    }

    public string? VersionID
    {
        get => GetVersionID();
        set => SetVersionID(value!);
    }
}
