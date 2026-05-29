/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/imageio/TIFFUtil.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tools.ImageIO;

public static class TIFFUtil
{
    public static bool WriteImage(BufferedImage image, string filename)
    {
        return ImageIOUtil.WriteImage(image, filename, 300);
    }
}
