/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added coverage for the C# port of printing classes (PDFPrintable, PDFPageable,
 * Orientation, Scaling). Render assertions are skipped on headless CI.
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

using PdfBox.Net.Printing;
using PdfBox.Net.PDModel;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Basic compile-and-instantiate tests for printing classes.
/// Rendering assertions are not included since they require a display and printer.
/// </summary>
public class PrintingTest
{
    [Fact]
    public void TestOrientationEnumValues()
    {
        // Just verify all enum values are defined and distinct
        Assert.NotEqual(Orientation.Auto, Orientation.Landscape);
        Assert.NotEqual(Orientation.Landscape, Orientation.ReverseLandscape);
        Assert.NotEqual(Orientation.ReverseLandscape, Orientation.Portrait);
    }

    [Fact]
    public void TestScalingEnumValues()
    {
        Assert.NotEqual(Scaling.ActualSize, Scaling.ShrinkToFit);
        Assert.NotEqual(Scaling.ShrinkToFit, Scaling.StretchToFit);
        Assert.NotEqual(Scaling.StretchToFit, Scaling.ScaleToFit);
    }

    [Fact]
    public void TestPdfPrintableConstants()
    {
        Assert.Equal(0f, PDFPrintable.RasterizeOff);
        Assert.Equal(-1f, PDFPrintable.RasterizeDpiAuto);
    }

    [Fact]
    public void TestPdfPageFormatInitialization()
    {
        PdfPageFormat format = new PdfPageFormat
        {
            PaperWidth = 595,
            PaperHeight = 842,
            ImageableX = 0,
            ImageableY = 0,
            ImageableWidth = 595,
            ImageableHeight = 842,
            Orientation = Orientation.Portrait
        };

        Assert.Equal(595f, format.PaperWidth);
        Assert.Equal(842f, format.PaperHeight);
        Assert.Equal(Orientation.Portrait, format.Orientation);
    }

    [Fact]
    public void TestPdfPrinterDefaultsAndUnsupportedBackend()
    {
        using PDDocument document = new();
        PDFPrinter printer = new(document);

        Assert.Null(printer.PrintBackend);
        Assert.Equal(Scaling.ShrinkToFit, printer.Scaling);
        Assert.True(printer.Center);
        Assert.Equal(PDFPrintable.RasterizeOff, printer.Dpi);
        Assert.False(printer.PrintToFile);
        Assert.Null(printer.PrintFileName);

        PlatformNotSupportedException ex = Assert.Throws<PlatformNotSupportedException>(() => printer.Print());
        Assert.Contains("No PdfBox.Net print backend is registered", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TestPdfPrinterAcceptsPrintToFileConfiguration()
    {
        using PDDocument document = new();
        PDFPrinter printer = new(document)
        {
            PrinterName = "Microsoft Print to PDF",
            PrintToFile = true,
            PrintFileName = Path.Combine(Path.GetTempPath(), "pdfbox-net-print-test.pdf")
        };

        Assert.Equal("Microsoft Print to PDF", printer.PrinterName);
        Assert.True(printer.PrintToFile);
        Assert.EndsWith("pdfbox-net-print-test.pdf", printer.PrintFileName, StringComparison.Ordinal);
    }

    [Fact]
    public void TestPdfPrinterDelegatesToConfiguredBackendForDeterministicPrintToFile()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), $"pdfbox-net-{Guid.NewGuid():N}.pdf");
        RecordingPrintBackend backend = new();
        try
        {
            using PDDocument document = new();
            document.AddPage(new PDPage());
            PDFPrinter printer = new(document)
            {
                PrintBackend = backend,
                PrinterName = "deterministic-test-printer",
                PrintToFile = true,
                PrintFileName = outputPath,
                Scaling = Scaling.ScaleToFit,
                ShowPageBorder = true
            };

            printer.Print();

            Assert.NotNull(backend.LastJob);
            Assert.Equal("deterministic-test-printer", backend.LastJob.PrinterName);
            Assert.Equal(1, backend.LastJob.NumberOfPages);
            Assert.Equal(Scaling.ScaleToFit, backend.LastJob.Scaling);
            Assert.True(backend.LastJob.ShowPageBorder);
            Assert.True(File.Exists(outputPath));
            Assert.NotEqual(0, new FileInfo(outputPath).Length);
            Assert.Contains("pages=1", File.ReadAllText(outputPath), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    private sealed class RecordingPrintBackend : IPDFPrintBackend
    {
        public string Name => "Recording";

        public bool IsSupported => true;

        public PDFPrintJob? LastJob { get; private set; }

        public void Print(PDFPrintJob job)
        {
            LastJob = job;
            if (job.PrintToFile)
            {
                File.WriteAllText(
                    job.PrintFileName!,
                    $"backend={Name};pages={job.NumberOfPages};scaling={job.Scaling};center={job.Center}");
            }
        }
    }
}
