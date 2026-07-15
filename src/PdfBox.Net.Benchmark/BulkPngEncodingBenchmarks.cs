/*
 * Copyright (c) 2026 Erik A. Brandstadmoen.
 * Licensed under the Apache License, Version 2.0.
 */

using BenchmarkDotNet.Attributes;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Benchmarks;

[MemoryDiagnoser]
public class BulkPngEncodingBenchmarks
{
    private byte[] _rgb = null!;
    private InterleavedPixelData _pixels;

    [Params(1024)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SkiaRenderingBackend.Register();
        _rgb = GC.AllocateUninitializedArray<byte>(checked(Size * Size * 3));
        for (int offset = 0; offset < _rgb.Length; offset += 3)
        {
            int pixel = offset / 3;
            _rgb[offset] = (byte)pixel;
            _rgb[offset + 1] = (byte)(pixel >> 4);
            _rgb[offset + 2] = (byte)(pixel >> 8);
        }

        _pixels = new InterleavedPixelData(
            _rgb,
            Size,
            Size,
            checked(Size * 3),
            InterleavedPixelFormat.Rgb24);
    }

    [Benchmark(Baseline = true)]
    public byte[] PerPixelUpload()
    {
        using BufferedImage image = new(Size, Size, BufferedImage.TYPE_INT_RGB);
        int offset = 0;
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int red = _rgb[offset++];
                int green = _rgb[offset++];
                int blue = _rgb[offset++];
                image.SetRgb(x, y, unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue);
            }
        }

        return RenderingBackend.Current.ImageCodec.Encode(image, EncodedImageFormat.Png, 100);
    }

    [Benchmark]
    public byte[] BulkUpload()
    {
        return RenderingBackend.Current.ImageCodec.EncodePng(_pixels);
    }
}
