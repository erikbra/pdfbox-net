// PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessWrite.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

namespace PdfBox.Net.IO;

/// <summary>
/// An interface allowing random access write operations.
/// </summary>
public interface RandomAccessWrite
{
    void Write(int b);

    void Write(byte[] b);

    void Write(byte[] b, int offset, int length);

    void Clear();

    void Close();
}
