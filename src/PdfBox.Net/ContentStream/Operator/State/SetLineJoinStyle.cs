/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Compatibility class for Apache PDFBox Java source naming.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/state/SetLineJoinStyle.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineJoinStyle : SetLineJoin
{
    public SetLineJoinStyle(PDFStreamEngine context)
        : base(context)
    {
    }
}
