/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFToImage.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;
using PdfBox.Net.Tools.ImageIO;

namespace PdfBox.Net.Tools;

public static class PDFToImage
{
    public static IReadOnlyList<string> RenderPng(string inputFile, string outputPrefix, float dpi = 96f)
    {
        return Render(
            inputFile,
            outputPrefix,
            "png",
            1,
            int.MaxValue,
            dpi,
            ImageType.RGB,
            password: null,
            subsampling: false,
            quality: 100,
            cropBox: null);
    }

    public static IReadOnlyList<string> Render(
        string inputFile,
        string outputPrefix,
        string imageFormat,
        int startPage,
        int endPage,
        float dpi,
        ImageType imageType,
        string? password,
        bool subsampling,
        int quality,
        float[]? cropBox)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPrefix);
        string normalizedFormat = NormalizeFormat(imageFormat);

        using PDDocument document = Loader.LoadPDF(inputFile, password);
        if (cropBox is not null)
        {
            ChangeCropBox(document, cropBox);
        }

        PDFRenderer renderer = new(document);
        renderer.SetSubsamplingAllowed(subsampling);
        int firstPage = Math.Max(1, startPage);
        int lastPage = Math.Min(document.GetNumberOfPages(), endPage);
        List<string> output = new(Math.Max(0, lastPage - firstPage + 1));
        for (int page = firstPage; page <= lastPage; page++)
        {
            using BufferedImage image = renderer.RenderImageWithDPI(page - 1, dpi, imageType);
            string path = $"{outputPrefix}-{page}.{normalizedFormat}";
            ImageIOUtil.WriteImage(image, path, (int)MathF.Round(dpi), quality);
            output.Add(path);
        }

        return output;
    }

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            RenderOptions options = ParseOptions(args);
            Render(
                options.InputFile,
                options.OutputPrefix,
                options.Format,
                options.StartPage,
                options.EndPage,
                options.Dpi,
                options.ImageType,
                options.Password,
                options.Subsampling,
                options.Quality,
                options.CropBox);
            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or NotSupportedException or PlatformNotSupportedException)
        {
            return ToolSupport.IoError(error, "rendering PDF pages", ex);
        }
    }

    private static RenderOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? outputPrefix = null;
        string? password = null;
        string format = "jpg";
        int startPage = 1;
        int endPage = int.MaxValue;
        float dpi = 96f;
        int quality = 100;
        ImageType imageType = ImageType.RGB;
        bool subsampling = false;
        float[]? cropBox = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-i":
                case "--input":
                    input = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-password":
                    password = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-format":
                    format = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-prefix":
                case "-outputPrefix":
                    outputPrefix = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-page":
                    startPage = endPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-startPage":
                    startPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-endPage":
                    endPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-dpi":
                case "-resolution":
                    dpi = ToolSupport.ReadFloatOption(args, ref i, arg);
                    break;
                case "-color":
                    if (!Enum.TryParse(ToolSupport.ReadOptionValue(args, ref i, arg), ignoreCase: true, out imageType))
                    {
                        throw new ArgumentException("Color must be one of BINARY, GRAY, RGB, ARGB, or BGR.");
                    }
                    break;
                case "-subsampling":
                    subsampling = true;
                    break;
                case "-quality":
                    quality = ConvertQuality(ToolSupport.ReadFloatOption(args, ref i, arg));
                    break;
                case "-cropbox":
                    cropBox = new float[4];
                    for (int j = 0; j < cropBox.Length; j++)
                    {
                        cropBox[j] = ToolSupport.ReadFloatOption(args, ref i, arg);
                    }
                    break;
                case "-time":
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        outputPrefix ??= Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(input)) ?? string.Empty,
            Path.GetFileNameWithoutExtension(input));

        _ = NormalizeFormat(format);
        return new RenderOptions(input, outputPrefix, password, format, startPage, endPage, dpi, quality, imageType, subsampling, cropBox);
    }

    private static void ChangeCropBox(PDDocument document, float[] cropBox)
    {
        foreach (PDPage page in document.GetPages())
        {
            PDRectangle rectangle = new();
            rectangle.SetLowerLeftX(cropBox[0]);
            rectangle.SetLowerLeftY(cropBox[1]);
            rectangle.SetUpperRightX(cropBox[2]);
            rectangle.SetUpperRightY(cropBox[3]);
            page.SetCropBox(rectangle);
        }
    }

    private static string NormalizeFormat(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "jpg",
            "png" => "png",
            _ => throw new NotSupportedException("The PdfBox.Net render CLI currently supports PNG and JPEG output only.")
        };
    }

    private static int ConvertQuality(float value)
    {
        if (value is >= 0f and <= 1f)
        {
            return Math.Clamp((int)MathF.Round(value * 100f), 1, 100);
        }

        if (value is >= 1f and <= 100f)
        {
            return Math.Clamp((int)MathF.Round(value), 1, 100);
        }

        throw new ArgumentException("Value for -quality must be between 0 and 1, or between 1 and 100.");
    }

    private sealed record RenderOptions(
        string InputFile,
        string OutputPrefix,
        string? Password,
        string Format,
        int StartPage,
        int EndPage,
        float Dpi,
        int Quality,
        ImageType ImageType,
        bool Subsampling,
        float[]? CropBox);
}
