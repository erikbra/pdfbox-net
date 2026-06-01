/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/hexviewer/HexModel.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Hexviewer;

/// <summary>
/// Data model for the hex viewer.  Holds the byte array and notifies listeners on change.
/// Adapted from Apache PDFBox HexModel (Khyrul Bashar).
/// </summary>
public sealed class HexModel : IHexChangeListener
{
    private readonly List<byte> _data;
    private readonly List<IHexModelChangeListener> _modelChangeListeners = [];

    /// <summary>Creates an empty model.</summary>
    public HexModel() : this([]) { }

    /// <param name="bytes">Initial byte array content.</param>
    public HexModel(byte[] bytes)
    {
        _data = new List<byte>(bytes.Length);
        foreach (byte b in bytes)
        {
            _data.Add(b);
        }
    }

    /// <summary>Returns the byte at the given index.</summary>
    public byte GetByte(int index) => _data[index];

    /// <summary>
    /// Returns the printable-ASCII representation of up to 16 bytes on the given line.
    /// Line numbering starts at 1.
    /// </summary>
    public char[] GetLineChars(int lineNumber)
    {
        int start = (lineNumber - 1) * 16;
        int length = Math.Min(_data.Count - start, 16);
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            char c = (char)(_data[start] & 0xFF);
            if (!IsAsciiPrintable(c))
            {
                c = '.';
            }

            chars[i] = c;
            start++;
        }

        return chars;
    }

    /// <summary>Returns the raw bytes for up to 16 bytes on the given line (1-based).</summary>
    public byte[] GetBytesForLine(int lineNumber)
    {
        int index = (lineNumber - 1) * 16;
        int length = Math.Min(_data.Count - index, 16);
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = _data[index];
            index++;
        }

        return bytes;
    }

    /// <summary>Total number of bytes in the model.</summary>
    public int Size() => _data.Count;

    /// <summary>Total number of 16-byte lines.</summary>
    public int TotalLine()
    {
        int size = _data.Count;
        return size % 16 != 0 ? size / 16 + 1 : size / 16;
    }

    /// <summary>Returns the 1-based line number that contains the given byte index.</summary>
    public static int LineNumber(int index)
    {
        int elementNo = index + 1;
        return elementNo % 16 != 0 ? elementNo / 16 + 1 : elementNo / 16;
    }

    /// <summary>Returns the position within its line (0–15) of the byte at the given index.</summary>
    public static int ElementIndexInLine(int index) => index % 16;

    /// <summary>Registers a listener that is notified whenever the model changes.</summary>
    public void AddHexModelChangeListener(IHexModelChangeListener listener)
        => _modelChangeListeners.Add(listener);

    /// <summary>Updates a single byte and notifies listeners when the value actually changed.</summary>
    public void UpdateModel(int index, byte value)
    {
        if (_data[index] != value)
        {
            _data[index] = value;
            FireModelChanged(index);
        }
    }

    /// <inheritdoc/>
    public void HexChanged(HexChangedEvent e)
    {
        int index = e.ByteIndex;
        if (index != -1 && GetByte(index) != e.NewValue)
        {
            _data[index] = e.NewValue;
        }

        FireModelChanged(index);
    }

    private void FireModelChanged(int index)
    {
        var evt = new HexModelChangedEvent(index, HexModelChangedEvent.SingleChange);
        foreach (var listener in _modelChangeListeners)
        {
            listener.HexModelChanged(evt);
        }
    }

    private static bool IsAsciiPrintable(char ch) => ch >= 32 && ch < 127;
}
