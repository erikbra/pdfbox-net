/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/imageio/JPEGUtil.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.Rendering;
using SkiaSharp;

namespace PdfBox.Net.Tools.ImageIO;

public static class JPEGUtil
{
    public static bool WriteImage(BufferedImage image, string filename, int quality = 90)
    {
        using SKImage skImage = SKImage.FromBitmap(image.Bitmap);
        using SKData data = skImage.Encode(SKEncodedImageFormat.Jpeg, quality);
        using FileStream output = File.Create(filename);
        data.SaveTo(output);
        return true;
    }
}
