/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/Loader.java
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

using PdfBox.Net.IO;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Encryption;
using PdfBox.Net.PDModel.Fdf;

namespace PdfBox.Net;

/// <summary>
/// Utility methods to load different types of documents.
/// </summary>
public static class Loader
{
    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(byte[] input)
    {
        return LoadPDF(input, password: null);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(byte[] input, string? password)
    {
        ArgumentNullException.ThrowIfNull(input);

        using MemoryStream stream = new(input, writable: false);
        return PDDocument.Load(stream, password);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(byte[] input, string? password, Stream? keyStore, string? alias)
    {
        return LoadPDFWithKeyStore(input, password, keyStore, alias);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(byte[] input, string? password, Stream? keyStore, string? alias,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDFWithKeyStore(input, password, keyStore, alias);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(byte[] input, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(input, password);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="filePath">The path to the file to load.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(string filePath)
    {
        return LoadPDF(filePath, password: null);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="filePath">The path to the file to load.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(string filePath, string? password)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return PDDocument.Load(filePath, password);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering PDF streams.
    /// </summary>
    /// <param name="filePath">The path to the file to load.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(string filePath, string? password, Stream? keyStore, string? alias)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return LoadPDFWithKeyStore(File.ReadAllBytes(filePath), password, keyStore, alias);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="filePath">The path to the file to load.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(string filePath, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(filePath, password);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="filePath">The path to the file to load.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(string filePath, string? password, Stream? keyStore, string? alias,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        ArgumentNullException.ThrowIfNull(filePath);
        return LoadPDFWithKeyStore(File.ReadAllBytes(filePath), password, keyStore, alias);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering new/altered PDF streams.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead)
    {
        return LoadPDF(randomAccessRead, password: null);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering new/altered PDF streams.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password)
    {
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        byte[] bytes = ReadAllBytes(randomAccessRead);
        return LoadPDF(bytes, password);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(randomAccessRead, password: null);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering new/altered PDF streams.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(randomAccessRead, password);
    }

    /// <summary>
    /// Parses a PDF. Unrestricted main memory will be used for buffering new/altered PDF streams.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password,
        Stream? keyStore, string? alias)
    {
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        return LoadPDFWithKeyStore(ReadAllBytes(randomAccessRead), password, keyStore, alias);
    }

    /// <summary>
    /// Parses a PDF.
    /// </summary>
    /// <param name="randomAccessRead">Random access read representing the PDF to be loaded.</param>
    /// <param name="password">Password to be used for decryption.</param>
    /// <param name="keyStore">Key store to be used for decryption when using public key security.</param>
    /// <param name="alias">Alias to be used for decryption when using public key security.</param>
    /// <param name="streamCacheCreateFunction">A function to create an instance of a stream cache.</param>
    /// <returns>Loaded document.</returns>
    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password,
        Stream? keyStore, string? alias,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        return LoadPDFWithKeyStore(ReadAllBytes(randomAccessRead), password, keyStore, alias);
    }

    /// <summary>
    /// This will load a document from a file.
    /// </summary>
    /// <param name="filePath">The path of the file to load.</param>
    /// <returns>The document that was loaded.</returns>
    public static FDFDocument LoadFDF(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return FDFDocument.Load(filePath);
    }

    /// <summary>
    /// This will load an FDF document from an input stream.
    /// </summary>
    /// <param name="input">Byte array that contains the document.</param>
    /// <returns>The document that was loaded.</returns>
    public static FDFDocument LoadFDF(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        using MemoryStream stream = new(input, writable: false);
        return FDFDocument.Load(stream);
    }

    /// <summary>
    /// This will load an FDF document from a random access read.
    /// </summary>
    /// <param name="randomAccessRead">The random access read to load from.</param>
    /// <returns>The document that was loaded.</returns>
    public static FDFDocument LoadFDF(RandomAccessRead randomAccessRead)
    {
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        byte[] bytes = ReadAllBytes(randomAccessRead);
        return LoadFDF(bytes);
    }

    /// <summary>
    /// This will load a document from a file path. The XFDF format is an XML representation of FDF.
    /// </summary>
    /// <param name="filePath">The path of the XFDF file to load.</param>
    /// <returns>The document that was loaded.</returns>
    public static FDFDocument LoadXFDF(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return FDFDocument.LoadXFDF(filePath);
    }

    /// <summary>
    /// This will load an XFDF document from an input stream. The XFDF format is an XML representation of FDF.
    /// </summary>
    /// <param name="input">The stream that contains the XFDF document.</param>
    /// <returns>The document that was loaded.</returns>
    public static FDFDocument LoadXFDF(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return FDFDocument.LoadXFDF(input);
    }

    private static byte[] ReadAllBytes(RandomAccessRead randomAccessRead)
    {
        long originalPosition = randomAccessRead.GetPosition();
        try
        {
            long length = randomAccessRead.Length();
            if (length > int.MaxValue)
            {
                throw new IOException("Random access source is too large to buffer in memory.");
            }

            randomAccessRead.Seek(0);
            byte[] bytes = new byte[checked((int)length)];
            int totalRead = 0;
            while (totalRead < bytes.Length)
            {
                int read = randomAccessRead.Read(bytes, totalRead, bytes.Length - totalRead);
                if (read <= 0)
                {
                    break;
                }

                totalRead += read;
            }

            if (totalRead == bytes.Length)
            {
                return bytes;
            }

            Array.Resize(ref bytes, totalRead);
            return bytes;
        }
        finally
        {
            if (!randomAccessRead.IsClosed())
            {
                randomAccessRead.Seek(originalPosition);
            }
        }
    }

    private static PDDocument LoadPDFWithKeyStore(byte[] input, string? password, Stream? keyStore, string? alias)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (keyStore is null)
        {
            return LoadPDF(input, password);
        }

        PublicKeyDecryptionMaterial material =
            PublicKeySecurityProvider.Current.LoadDecryptionMaterial(keyStore, password, alias);
        using MemoryStream stream = new(input, writable: false);
        return PDDocument.Load(stream, material);
    }
}
