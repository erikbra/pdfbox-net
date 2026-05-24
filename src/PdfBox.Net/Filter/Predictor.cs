/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox predictor decoding logic.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/Predictor.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

internal static class Predictor
{
    public static byte[] Decode(byte[] data, COSDictionary decodeParams)
    {
        int predictor = decodeParams.GetInt(COSName.PREDICTOR, 1);
        if (predictor <= 1)
        {
            return data;
        }

        int colors = Math.Clamp(decodeParams.GetInt(COSName.COLORS, 1), 1, int.MaxValue);
        int bitsPerComponent = decodeParams.GetInt(COSName.BITS_PER_COMPONENT, 8);
        int columns = Math.Clamp(decodeParams.GetInt(COSName.COLUMNS, 1), 1, int.MaxValue);
        int rowLength = CalculateRowLength(colors, bitsPerComponent, columns);
        if (rowLength <= 0)
        {
            return data;
        }

        using MemoryStream output = new(data.Length);
        byte[] currentRow = new byte[rowLength];
        byte[] previousRow = new byte[rowLength];

        int offset = 0;
        while (offset < data.Length)
        {
            int rowPredictor = predictor;
            if (predictor >= 10)
            {
                if (offset >= data.Length)
                {
                    break;
                }

                rowPredictor = data[offset] + 10;
                offset++;
            }

            int available = Math.Min(rowLength, data.Length - offset);
            Array.Clear(currentRow, 0, currentRow.Length);
            if (available > 0)
            {
                Buffer.BlockCopy(data, offset, currentRow, 0, available);
                offset += available;
            }

            DecodePredictorRow(rowPredictor, colors, bitsPerComponent, columns, currentRow, previousRow);
            output.Write(currentRow, 0, currentRow.Length);

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return output.ToArray();
    }

    private static int CalculateRowLength(int colors, int bitsPerComponent, int columns)
    {
        int bitsPerPixel = colors * bitsPerComponent;
        return (columns * bitsPerPixel + 7) / 8;
    }

    private static void DecodePredictorRow(int predictor, int colors, int bitsPerComponent, int columns, byte[] actline, byte[] lastline)
    {
        if (predictor == 1 || predictor == 10)
        {
            return;
        }

        int bitsPerPixel = colors * bitsPerComponent;
        int bytesPerPixel = (bitsPerPixel + 7) / 8;
        int rowLength = actline.Length;

        switch (predictor)
        {
            case 2:
            case 11:
                for (int p = bytesPerPixel; p < rowLength; p++)
                {
                    int sub = actline[p] & 0xff;
                    int left = actline[p - bytesPerPixel] & 0xff;
                    actline[p] = (byte)((sub + left) & 0xff);
                }

                break;
            case 12:
                for (int p = 0; p < rowLength; p++)
                {
                    int up = actline[p] & 0xff;
                    int prior = lastline[p] & 0xff;
                    actline[p] = (byte)((up + prior) & 0xff);
                }

                break;
            case 13:
                for (int p = 0; p < rowLength; p++)
                {
                    int avg = actline[p] & 0xff;
                    int left = p - bytesPerPixel >= 0 ? actline[p - bytesPerPixel] & 0xff : 0;
                    int up = lastline[p] & 0xff;
                    actline[p] = (byte)((avg + (left + up) / 2) & 0xff);
                }

                break;
            case 14:
                for (int p = 0; p < rowLength; p++)
                {
                    int paeth = actline[p] & 0xff;
                    int a = p - bytesPerPixel >= 0 ? actline[p - bytesPerPixel] & 0xff : 0;
                    int b = lastline[p] & 0xff;
                    int c = p - bytesPerPixel >= 0 ? lastline[p - bytesPerPixel] & 0xff : 0;
                    int value = a + b - c;
                    int absa = Math.Abs(value - a);
                    int absb = Math.Abs(value - b);
                    int absc = Math.Abs(value - c);
                    int paethValue = absa <= absb && absa <= absc ? a : (absb <= absc ? b : c);
                    actline[p] = (byte)((paeth + paethValue) & 0xff);
                }

                break;
        }
    }
}
