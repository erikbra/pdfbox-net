/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted base image model for PdfBox.Net.
 */

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Graphics.Image;

public abstract class PDImage : COSObjectable
{
    public abstract COSDictionary GetCOSObject();
    COSBase COSObjectable.GetCOSObject() => GetCOSObject();

    public abstract int GetBitsPerComponent();
    public abstract void SetBitsPerComponent(int bitsPerComponent);
    public abstract PDColorSpace GetColorSpace();
    public abstract void SetColorSpace(PDColorSpace? colorSpace);
    public abstract int GetHeight();
    public abstract void SetHeight(int height);
    public abstract int GetWidth();
    public abstract void SetWidth(int width);
    public abstract bool GetInterpolate();
    public abstract void SetInterpolate(bool value);
    public abstract void SetDecode(COSArray? decode);
    public abstract COSArray? GetDecode();
    public abstract bool IsStencil();
    public abstract void SetStencil(bool isStencil);
    public abstract Stream CreateInputStream();
    public abstract Stream CreateInputStream(DecodeOptions options);
    public abstract Stream CreateInputStream(IList<string> stopFilters);
    public abstract bool IsEmpty();
    public abstract byte[] GetData();
}
