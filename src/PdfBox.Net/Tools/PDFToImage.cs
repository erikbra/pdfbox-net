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
using PdfBox.Net.Rendering;
using PdfBox.Net.Tools.ImageIO;

namespace PdfBox.Net.Tools;

public static class PDFToImage
{
    public static IReadOnlyList<string> RenderPng(string inputFile, string outputPrefix, float dpi = 96f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPrefix);

        using PDDocument document = Loader.LoadPDF(inputFile);
        PDFRenderer renderer = new(document);
        List<string> output = new(document.GetNumberOfPages());
        for (int i = 0; i < document.GetNumberOfPages(); i++)
        {
            using BufferedImage image = renderer.RenderImageWithDPI(i, dpi);
            string path = $"{outputPrefix}-{i + 1}.png";
            ImageIOUtil.WriteImage(image, path, (int)MathF.Round(dpi));
            output.Add(path);
        }

        return output;
    }
}
