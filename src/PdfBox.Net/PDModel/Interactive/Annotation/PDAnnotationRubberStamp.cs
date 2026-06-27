/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Compatibility class for Apache PDFBox Java source naming.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationRubberStamp.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationRubberStamp : PDAnnotationStamp
{
    public new const string NAME_APPROVED = PDAnnotationStamp.NAME_APPROVED;
    public new const string NAME_EXPERIMENTAL = PDAnnotationStamp.NAME_EXPERIMENTAL;
    public new const string NAME_NOT_APPROVED = PDAnnotationStamp.NAME_NOT_APPROVED;
    public new const string NAME_AS_IS = PDAnnotationStamp.NAME_AS_IS;
    public new const string NAME_EXPIRED = PDAnnotationStamp.NAME_EXPIRED;
    public new const string NAME_NOT_FOR_PUBLIC_RELEASE = PDAnnotationStamp.NAME_NOT_FOR_PUBLIC_RELEASE;
    public new const string NAME_FOR_PUBLIC_RELEASE = PDAnnotationStamp.NAME_FOR_PUBLIC_RELEASE;
    public new const string NAME_DRAFT = PDAnnotationStamp.NAME_DRAFT;
    public new const string NAME_FOR_COMMENT = PDAnnotationStamp.NAME_FOR_COMMENT;
    public new const string NAME_TOP_SECRET = PDAnnotationStamp.NAME_TOP_SECRET;
    public new const string NAME_DEPARTMENTAL = PDAnnotationStamp.NAME_DEPARTMENTAL;
    public new const string NAME_CONFIDENTIAL = PDAnnotationStamp.NAME_CONFIDENTIAL;
    public new const string NAME_FINAL = PDAnnotationStamp.NAME_FINAL;
    public new const string NAME_SOLD = PDAnnotationStamp.NAME_SOLD;
    public new const string SUB_TYPE = PDAnnotationStamp.SUB_TYPE;

    public PDAnnotationRubberStamp()
    {
    }

    public PDAnnotationRubberStamp(COSDictionary field)
        : base(field)
    {
    }

    public new void SetName(string name)
    {
        base.SetName(name);
    }

    public new string GetName()
    {
        return base.GetName();
    }
}
