/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateStructure.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDFTemplateStructure
{
    public PDDocument? Template { get; set; }
    public PDPage? Page { get; set; }
    public PDAcroForm? AcroForm { get; set; }
    public PDSignatureField? SignatureField { get; set; }
    public PDSignature? Signature { get; set; }
    public PDRectangle? SignatureRectangle { get; set; }
    public COSDocument? VisualSignature { get; set; }
}
