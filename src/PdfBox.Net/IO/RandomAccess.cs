// PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccess.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

namespace PdfBox.Net.IO;

/// <summary>
/// An interface to allow data to be stored completely in memory or
/// to use a scratch file on the disk.
/// </summary>
public interface RandomAccess : RandomAccessRead, RandomAccessWrite
{
    // super interface for both read and write
}
