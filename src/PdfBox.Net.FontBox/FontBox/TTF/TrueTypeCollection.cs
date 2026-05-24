/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TrueTypeCollection.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// A TrueType Collection, now more properly known as a "Font Collection" as it may contain either
/// TrueType or OpenType fonts.
/// </summary>
public class TrueTypeCollection : IDisposable
{
    private readonly TTFDataStream _stream;
    private readonly int _numFonts;
    private readonly long[] _fontOffsets;

    /// <summary>
    /// Creates a new TrueTypeCollection from a .ttc file.
    /// </summary>
    /// <param name="path">The path to the TTC file.</param>
    public TrueTypeCollection(string path)
        : this(CreateBufferedDataStream(new RandomAccessReadBufferedFile(path), closeAfterReading: true))
    {
    }

    /// <summary>
    /// Creates a new TrueTypeCollection from a .ttc input stream.
    /// </summary>
    /// <param name="inputStream">A TTC input stream.</param>
    public TrueTypeCollection(Stream inputStream)
        : this(CreateBufferedDataStream(new RandomAccessReadBuffer(inputStream), closeAfterReading: false))
    {
    }

    /// <summary>
    /// Creates a new TrueTypeCollection from a TTFDataStream.
    /// </summary>
    private TrueTypeCollection(TTFDataStream stream)
    {
        _stream = stream;

        // TTC header
        string tag = stream.ReadTag();
        if (tag != "ttcf")
        {
            throw new IOException("Missing TTC header");
        }
        float version = stream.Read32Fixed();
        _numFonts = (int)stream.ReadUnsignedInt();
        if (_numFonts <= 0 || _numFonts > 1024)
        {
            throw new IOException($"Invalid number of fonts {_numFonts}");
        }
        _fontOffsets = new long[_numFonts];
        for (int i = 0; i < _numFonts; i++)
        {
            _fontOffsets[i] = stream.ReadUnsignedInt();
        }
        if (version >= 2)
        {
            // not used at this time
            int ulDsigTag = stream.ReadUnsignedShort();
            int ulDsigLength = stream.ReadUnsignedShort();
            int ulDsigOffset = stream.ReadUnsignedShort();
        }
    }

    private static TTFDataStream CreateBufferedDataStream(RandomAccessRead randomAccessRead, bool closeAfterReading)
    {
        try
        {
            return new RandomAccessReadDataStream(randomAccessRead);
        }
        finally
        {
            if (closeAfterReading)
            {
                try { randomAccessRead.Close(); } catch { /* ignore */ }
            }
        }
    }

    /// <summary>
    /// Run the callback for each TT font in the collection.
    /// </summary>
    /// <param name="trueTypeFontProcessor">The object with the callback method.</param>
    public void ProcessAllFonts(TrueTypeFontProcessor trueTypeFontProcessor)
    {
        for (int i = 0; i < _numFonts; i++)
        {
            TrueTypeFont font = GetFontAtIndex(i);
            trueTypeFontProcessor(font);
        }
    }

    /// <summary>
    /// Run the callback for each TT font header in the collection.
    /// </summary>
    public static void ProcessAllFontHeaders(string ttcFilePath, TrueTypeFontHeadersProcessor trueTypeFontProcessor)
    {
        using RandomAccessRead read = new RandomAccessReadBufferedFile(ttcFilePath);
        using TTFDataStream stream = new RandomAccessReadUnbufferedDataStream(read);
        using var ttc = new TrueTypeCollection(stream);
        for (int i = 0; i < ttc._numFonts; i++)
        {
            TTFParser parser = ttc.CreateFontParserAtIndexAndSeek(i);
            FontHeaders headers = parser.ParseTableHeaders(new TTCDataStream(ttc._stream));
            trueTypeFontProcessor(headers);
        }
    }

    private TrueTypeFont GetFontAtIndex(int idx)
    {
        TTFParser parser = CreateFontParserAtIndexAndSeek(idx);
        return parser.Parse(new TTCDataStream(_stream));
    }

    private TTFParser CreateFontParserAtIndexAndSeek(int idx)
    {
        _stream.Seek(_fontOffsets[idx]);
        TTFParser parser;
        if (_stream.ReadTag() == "OTTO")
        {
            parser = new OTFParser(false);
        }
        else
        {
            parser = new TTFParser(false);
        }
        _stream.Seek(_fontOffsets[idx]);
        return parser;
    }

    /// <summary>
    /// Get a TT font from a collection.
    /// </summary>
    /// <param name="name">The postscript name of the font.</param>
    /// <returns>The found font, or null if none is found.</returns>
    public TrueTypeFont? GetFontByName(string name)
    {
        for (int i = 0; i < _numFonts; i++)
        {
            TrueTypeFont font = GetFontAtIndex(i);
            if (font.GetName() == name)
            {
                return font;
            }
        }
        return null;
    }

    /// <summary>
    /// Callback for processing each TrueType font in the collection.
    /// </summary>
    public delegate void TrueTypeFontProcessor(TrueTypeFont ttf);

    /// <summary>
    /// Callback for processing each TrueType font header in the collection.
    /// </summary>
    public delegate void TrueTypeFontHeadersProcessor(FontHeaders fontHeaders);

    public void Dispose()
    {
        _stream.Close();
    }
}
