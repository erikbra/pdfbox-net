/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/interactive/annotation/AppearanceGenerationTest.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tests;

public class AppearanceGenerationTest
{
    [Fact(Skip = "Upstream parity gap: FreeText/callout, Caret and Polygon appearance handlers are stubs; Line endings/caption, Highlight and Square geometry also differ. #954 baseline: 111001 differing pixels; see reports/issue-954-upstream-sync-assessment.md.")]
    public void TestAnnotationAppearancesGeneration()
    {
        string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Annotations.pdf");
        using PDDocument expectedDocument = Loader.LoadPDF(fixture);
        using BufferedImage expected = new PDFRenderer(expectedDocument).RenderImage(0);
        using PDDocument actualDocument = Loader.LoadPDF(fixture);
        IList<PDAnnotation> annotations = actualDocument.GetPage(0).GetAnnotations();
        Assert.All(annotations, annotation => Assert.NotNull(annotation.GetAppearance()));
        foreach (PDAnnotation annotation in annotations)
        {
            annotation.SetAppearance(null);
        }

        PDAnnotationFreeText freeText = Assert.IsType<PDAnnotationFreeText>(annotations[6]);
        PDRectangle rectangle = freeText.GetRectangle()!;
        rectangle.SetLowerLeftX(72);
        rectangle.SetLowerLeftY(216);
        freeText.SetRectangle(rectangle);
        PDAnnotationCaret caret = Assert.IsType<PDAnnotationCaret>(annotations[8]);
        caret.SetRectangle(new PDRectangle(300, 50, 100, 100));

        using BufferedImage actual = new PDFRenderer(actualDocument).RenderImage(0);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        int differingPixels = 0;
        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                if (expected.GetRgb(x, y) != actual.GetRgb(x, y))
                {
                    differingPixels++;
                }
            }
        }
        Assert.Equal(0, differingPixels);
    }
}
