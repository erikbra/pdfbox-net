/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused incremental-save behavior parity tests. The asserted structure follows Apache
 * PDFBox saveIncremental semantics: preserve the original bytes and append a new update
 * section with /Prev pointing at the previous startxref.
 */

using System.Globalization;
using System.Text;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class PDDocumentIncrementalSaveParityTest
{
    private const string ProbeKey = "PdfBoxNetIncrementalProbe";
    private const string ProbeValue = "updated";

    [Theory]
    [InlineData("classic-xref-fixture.pdf")]
    [InlineData("xref-stream-fixture.pdf")]
    public void SaveIncrementalPreservesOriginalBytesAndAppendsPrevChain(string fixtureName)
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);
        byte[] original = File.ReadAllBytes(fixturePath);
        long originalStartxref = LastStartxref(original);

        byte[] incremental;
        using (PDDocument document = PDDocument.Load(fixturePath))
        {
            document.GetDocumentInformation().SetCustomMetadataValue(ProbeKey, ProbeValue);
            using MemoryStream output = new();
            document.SaveIncremental(output);
            incremental = output.ToArray();
        }

        Assert.True(incremental.Length > original.Length);
        Assert.True(
            original.AsSpan().SequenceEqual(incremental.AsSpan(0, original.Length)),
            "Incremental save should copy the original source bytes before appending the update.");

        string serialized = Encoding.Latin1.GetString(incremental);
        Assert.Equal(2, CountOccurrences(serialized, "%%EOF"));
        Assert.Equal(2, CountOccurrences(serialized, "startxref"));
        Assert.Contains($"/Prev {originalStartxref.ToString(CultureInfo.InvariantCulture)}", serialized, StringComparison.Ordinal);

        long appendedXrefOffset = LastXrefTableOffset(incremental);
        Assert.True(appendedXrefOffset >= original.Length, "The appended xref table should be written after the original file bytes.");
        Assert.Equal(appendedXrefOffset, LastStartxref(incremental));

        using PDDocument reloaded = PDDocument.Load(new MemoryStream(incremental));
        Assert.Equal(1, reloaded.GetNumberOfPages());
        Assert.Equal(ProbeValue, reloaded.GetDocumentInformation().GetCustomMetadataValue(ProbeKey));
    }

    [Fact]
    public void FullSaveRewritesInsteadOfAppendingIncrementalMarkers()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "classic-xref-fixture.pdf");
        byte[] original = File.ReadAllBytes(fixturePath);

        byte[] saved;
        using (PDDocument document = PDDocument.Load(fixturePath))
        {
            document.GetDocumentInformation().SetCustomMetadataValue(ProbeKey, ProbeValue);
            using MemoryStream output = new();
            document.Save(output);
            saved = output.ToArray();
        }

        string serialized = Encoding.Latin1.GetString(saved);
        Assert.Equal(1, CountOccurrences(serialized, "%%EOF"));
        Assert.Equal(1, CountOccurrences(serialized, "startxref"));
        Assert.DoesNotContain("/Prev", serialized, StringComparison.Ordinal);
        Assert.False(
            saved.Length >= original.Length && original.AsSpan().SequenceEqual(saved.AsSpan(0, original.Length)),
            "Full save should rewrite the document instead of preserving the original file as a prefix.");

        using PDDocument reloaded = PDDocument.Load(new MemoryStream(saved));
        Assert.Equal(1, reloaded.GetNumberOfPages());
        Assert.Equal(ProbeValue, reloaded.GetDocumentInformation().GetCustomMetadataValue(ProbeKey));
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static long LastStartxref(byte[] pdf)
    {
        string text = Encoding.Latin1.GetString(pdf);
        int index = text.LastIndexOf("startxref", StringComparison.Ordinal);
        Assert.True(index >= 0, "PDF should contain a startxref marker.");
        index += "startxref".Length;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        int start = index;
        while (index < text.Length && char.IsDigit(text[index]))
        {
            index++;
        }

        Assert.True(start < index, "startxref should be followed by a byte offset.");
        return long.Parse(text.AsSpan(start, index - start), CultureInfo.InvariantCulture);
    }

    private static long LastXrefTableOffset(byte[] pdf)
    {
        string text = Encoding.Latin1.GetString(pdf);
        int newlineXref = text.LastIndexOf("\nxref\n", StringComparison.Ordinal);
        if (newlineXref >= 0)
        {
            return newlineXref + 1;
        }

        int firstXref = text.IndexOf("xref\n", StringComparison.Ordinal);
        Assert.True(firstXref >= 0, "PDF should contain a classic xref table.");
        return firstXref;
    }
}
