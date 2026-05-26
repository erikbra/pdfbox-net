/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/filetypedetector/FileTypeDetector.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;

namespace PdfBox.Net.Util.FileTypeDetector;

public static class FileTypeDetector
{
    private static readonly ByteTrie<FileType> Root = CreateRoot();

    public static FileType DetectFileType(Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        if (!inputStream.CanSeek)
        {
            throw new IOException("Stream must support seeking.");
        }

        int maxByteCount = Root.GetMaxDepth();
        long originalPosition = inputStream.Position;
        byte[] bytes = new byte[maxByteCount];
        int bytesRead = inputStream.Read(bytes, 0, bytes.Length);
        inputStream.Position = originalPosition;

        if (bytesRead <= 0)
        {
            throw new IOException("Stream ended before file's magic number could be determined.");
        }

        if (bytesRead != bytes.Length)
        {
            Array.Resize(ref bytes, bytesRead);
        }

        return Root.Find(bytes);
    }

    public static FileType DetectFileType(byte[] fileBytes)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);
        return Root.Find(fileBytes);
    }

    private static ByteTrie<FileType> CreateRoot()
    {
        ByteTrie<FileType> root = new();
        root.SetDefaultValue(FileType.UNKNOWN);

        byte[] iiBytes = Encoding.Latin1.GetBytes("II");
        byte[] mmBytes = Encoding.Latin1.GetBytes("MM");
        root.AddPath(FileType.JPEG, [(byte)0xff, (byte)0xd8]);
        root.AddPath(FileType.TIFF, iiBytes, [0x2a, 0x00]);
        root.AddPath(FileType.TIFF, mmBytes, [0x00, 0x2a]);
        root.AddPath(FileType.PSD, Encoding.Latin1.GetBytes("8BPS"));
        root.AddPath(FileType.PNG, [(byte)0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52]);
        root.AddPath(FileType.BMP, Encoding.Latin1.GetBytes("BM"));
        root.AddPath(FileType.GIF, Encoding.Latin1.GetBytes("GIF87a"));
        root.AddPath(FileType.GIF, Encoding.Latin1.GetBytes("GIF89a"));
        root.AddPath(FileType.ICO, [0x00, 0x00, 0x01, 0x00]);
        root.AddPath(FileType.PCX, [0x0A, 0x00, 0x01]);
        root.AddPath(FileType.PCX, [0x0A, 0x02, 0x01]);
        root.AddPath(FileType.PCX, [0x0A, 0x03, 0x01]);
        root.AddPath(FileType.PCX, [0x0A, 0x05, 0x01]);
        root.AddPath(FileType.RIFF, Encoding.Latin1.GetBytes("RIFF"));
        root.AddPath(FileType.CRW, iiBytes, [0x1a, 0x00, 0x00, 0x00], Encoding.Latin1.GetBytes("HEAPCCDR"));
        root.AddPath(FileType.CR2, iiBytes, [0x2a, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52]);
        root.AddPath(FileType.NEF, mmBytes, [0x00, 0x2a, 0x00, 0x00, 0x00, 0x80, 0x00]);
        root.AddPath(FileType.ORF, Encoding.Latin1.GetBytes("IIRO"), [0x08, 0x00]);
        root.AddPath(FileType.ORF, Encoding.Latin1.GetBytes("IIRS"), [0x08, 0x00]);
        root.AddPath(FileType.RAF, Encoding.Latin1.GetBytes("FUJIFILMCCD-RAW"));
        root.AddPath(FileType.RW2, iiBytes, [0x55, 0x00]);
        return root;
    }
}
