/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Added as a .NET support enum for encryption key-size selection.
 *
 * PDFBOX_SOURCE_PATH: n/a
 * PDFBOX_SOURCE_COMMIT: n/a
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: n/a
 */

namespace PdfBox.Net.PDModel.Encryption;

public enum AESKeyLength
{
    AES_128 = 128,
    AES_256 = 256
}
