/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreateGradientShadingPDF.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
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

using System.Buffers.Binary;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Shading;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example creates a PDF with type 2 (axial) and type 3 (radial) shadings with a type 2
/// (exponential) function, and a type 4 (gouraud triangle shading) without function.
/// </summary>
public class CreateGradientShadingPDF
{
    /// <summary>
    /// This will create the PDF and write the contents to a file.
    /// </summary>
    /// <param name="file">The name of the file to write to.</param>
    public void Create(string file)
    {
        using (PDDocument document = new PDDocument())
        {
            PDPage page = new PDPage();
            document.AddPage(page);

            COSDictionary fdict = new COSDictionary();
            fdict.SetInt(COSName.FUNCTION_TYPE, 2);
            COSArray domain = new COSArray();
            domain.Add(COSInteger.ZERO);
            domain.Add(COSInteger.ONE);
            COSArray c0 = new COSArray();
            c0.Add(COSInteger.ONE);
            c0.Add(COSInteger.ZERO);
            c0.Add(COSInteger.ZERO);
            COSArray c1 = new COSArray();
            c1.Add(COSNumber.Get("0.5"));
            c1.Add(COSInteger.ONE);
            c1.Add(COSNumber.Get("0.5"));
            fdict.SetItem(COSName.DOMAIN, domain);
            fdict.SetItem(COSName.C0, c0);
            fdict.SetItem(COSName.C1, c1);
            fdict.SetInt(COSName.N, 1);
            PDFunctionType2 func = new PDFunctionType2(fdict);

            PDShadingType2 axialShading = new PDShadingType2(new COSDictionary());
            axialShading.SetColorSpace(PDDeviceRGB.Instance);
            axialShading.SetShadingType(PDShading.SHADING_TYPE2);
            COSArray coords1 = new COSArray();
            coords1.Add(COSInteger.Get(100));
            coords1.Add(COSInteger.Get(400));
            coords1.Add(COSInteger.Get(400));
            coords1.Add(COSInteger.Get(600));
            axialShading.SetCoords(coords1);
            axialShading.SetFunction(func);

            PDShadingType3 radialShading = new PDShadingType3(new COSDictionary());
            radialShading.SetColorSpace(PDDeviceRGB.Instance);
            radialShading.SetShadingType(PDShading.SHADING_TYPE3);
            COSArray coords2 = new COSArray();
            coords2.Add(COSInteger.Get(100));
            coords2.Add(COSInteger.Get(400));
            coords2.Add(COSInteger.Get(50));
            coords2.Add(COSInteger.Get(400));
            coords2.Add(COSInteger.Get(600));
            coords2.Add(COSInteger.Get(150));
            radialShading.SetCoords(coords2);
            radialShading.SetFunction(func);

            PDShadingType4 gouraudShading = new PDShadingType4(new COSStream());
            gouraudShading.SetShadingType(PDShading.SHADING_TYPE4);
            gouraudShading.SetBitsPerFlag(8);
            gouraudShading.SetBitsPerCoordinate(16);
            gouraudShading.SetBitsPerComponent(8);
            COSArray decodeArray = new COSArray();
            decodeArray.Add(COSInteger.ZERO);
            decodeArray.Add(COSInteger.Get(0xFFFF));
            decodeArray.Add(COSInteger.ZERO);
            decodeArray.Add(COSInteger.Get(0xFFFF));
            decodeArray.Add(COSInteger.ZERO);
            decodeArray.Add(COSInteger.ONE);
            decodeArray.Add(COSInteger.ZERO);
            decodeArray.Add(COSInteger.ONE);
            decodeArray.Add(COSInteger.ZERO);
            decodeArray.Add(COSInteger.ONE);
            gouraudShading.SetDecodeValues(decodeArray);
            gouraudShading.SetColorSpace(PDDeviceRGB.Instance);

            using (Stream output = ((COSStream)gouraudShading.GetCOSObject()).CreateOutputStream())
            {
                WriteVertex(output, 0, 0, 0, 0xFF, 0, 0);
                WriteVertex(output, 0, 100, 100, 0, 0xFF, 0);
                WriteVertex(output, 0, 200, 0, 0, 0, 0xFF);
            }

            using (PDPageContentStream contentStream = new PDPageContentStream(
                document,
                page,
                PDPageContentStream.AppendMode.APPEND,
                false))
            {
                contentStream.ShadingFill(axialShading);
                contentStream.ShadingFill(radialShading);
                contentStream.ShadingFill(gouraudShading);
            }

            document.Save(file);
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: CreateGradientShadingPDF <outputfile.pdf>");
        }
        else
        {
            CreateGradientShadingPDF creator = new CreateGradientShadingPDF();
            creator.Create(args[0]);
        }
    }

    private static void WriteVertex(Stream output, byte flag, ushort x, ushort y, byte r, byte g, byte b)
    {
        output.WriteByte(flag);
        WriteUInt16(output, x);
        WriteUInt16(output, y);
        output.WriteByte(r);
        output.WriteByte(g);
        output.WriteByte(b);
    }

    private static void WriteUInt16(Stream output, ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        output.Write(buffer);
    }
}
