/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationFileAttachment.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationFileAttachment
{
    public string AttachmentName
    {
        get => GetAttachmentName();
        set => SetAttachmentName(value);
    }

    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }
}
