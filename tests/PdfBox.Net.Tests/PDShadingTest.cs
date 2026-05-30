/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native test for PDShading hierarchy introduced in issue #53.
 *
 * PORT_MODE: native-test
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public class PDShadingTest
{
    // ── Type constants ────────────────────────────────────────────────────────

    [Fact]
    public void ShadingTypeConstants_HaveCorrectValues()
    {
        Assert.Equal(1, PDShading.SHADING_TYPE1);
        Assert.Equal(2, PDShading.SHADING_TYPE2);
        Assert.Equal(3, PDShading.SHADING_TYPE3);
        Assert.Equal(4, PDShading.SHADING_TYPE4);
        Assert.Equal(5, PDShading.SHADING_TYPE5);
        Assert.Equal(6, PDShading.SHADING_TYPE6);
        Assert.Equal(7, PDShading.SHADING_TYPE7);
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ReturnsCorrectSubtype_ForEachShadingType()
    {
        Assert.IsType<PDShadingType1>(CreateShadingDict(1));
        Assert.IsType<PDShadingType2>(CreateShadingDict(2));
        Assert.IsType<PDShadingType3>(CreateShadingDict(3));
        Assert.IsType<PDShadingType4>(CreateShadingDict(4));
        Assert.IsType<PDShadingType5>(CreateShadingDict(5));
        Assert.IsType<PDShadingType6>(CreateShadingDict(6));
        Assert.IsType<PDShadingType7>(CreateShadingDict(7));
    }

    [Fact]
    public void Create_ThrowsIOException_ForUnknownShadingType()
    {
        COSDictionary dict = new();
        dict.SetInt(COSName.SHADING_TYPE, 99);
        Assert.Throws<IOException>(() => PDShading.Create(dict));
    }

    // ── GetShadingType ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void GetShadingType_ReturnsMatchingValue(int shadingType)
    {
        PDShading shading = CreateShadingDict(shadingType);
        Assert.Equal(shadingType, shading.GetShadingType());
    }

    [Fact]
    public void ToPaint_ReturnsExpectedPaintTypes()
    {
        Matrix matrix = new();
        Assert.IsType<Type1ShadingPaint>(new PDShadingType1(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<AxialShadingPaint>(new PDShadingType2(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<RadialShadingPaint>(new PDShadingType3(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<Type4ShadingPaint>(new PDShadingType4(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<Type5ShadingPaint>(new PDShadingType5(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<Type6ShadingPaint>(new PDShadingType6(new COSDictionary()).ToPaint(matrix));
        Assert.IsType<Type7ShadingPaint>(new PDShadingType7(new COSDictionary()).ToPaint(matrix));
    }

    // ── SetShadingType / GetCOSObject ─────────────────────────────────────────

    [Fact]
    public void SetShadingType_WritesToDictionary()
    {
        PDShadingType2 shading = new(new COSDictionary());
        shading.SetShadingType(2);
        Assert.Equal(2, shading.GetCOSObject().GetInt(COSName.SHADING_TYPE));
    }

    // ── Background ────────────────────────────────────────────────────────────

    [Fact]
    public void Background_RoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetBackground());

        COSArray bg = COSArray.Of(1f, 0f, 0f);
        shading.SetBackground(bg);

        COSArray? result = shading.GetBackground();
        Assert.NotNull(result);
        Assert.Equal(3, result.Size());
        Assert.Equal(1f, ((COSNumber)result.Get(0)!).FloatValue(), 4);
    }

    // ── BBox ─────────────────────────────────────────────────────────────────

    [Fact]
    public void BBox_RoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetBBox());

        PDRectangle bbox = new(0, 0, 100, 200);
        shading.SetBBox(bbox);

        PDRectangle? result = shading.GetBBox();
        Assert.NotNull(result);
        Assert.Equal(100f, result.GetWidth(), 4);
        Assert.Equal(200f, result.GetHeight(), 4);
    }

    [Fact]
    public void SetBBox_Null_RemovesEntry()
    {
        COSDictionary dict = new();
        dict.SetItem(COSName.BBOX, COSArray.Of(0f, 0f, 1f, 1f));
        PDShadingType2 shading = new(dict);

        shading.SetBBox(null);
        Assert.Null(shading.GetBBox());
        Assert.Null(dict.GetDictionaryObject(COSName.BBOX));
    }

    // ── AntiAlias ─────────────────────────────────────────────────────────────

    [Fact]
    public void AntiAlias_DefaultFalse_AndRoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.False(shading.GetAntiAlias());

        shading.SetAntiAlias(true);
        Assert.True(shading.GetAntiAlias());

        shading.SetAntiAlias(false);
        Assert.False(shading.GetAntiAlias());
    }

    // ── GetType ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetType_ReturnsShadingName()
    {
        PDShadingType1 shading = new(new COSDictionary());
        Assert.Equal("Shading", shading.GetType());
    }

    // ── Type 1: Function-based shading ────────────────────────────────────────

    [Fact]
    public void Type1_Domain_RoundTrip()
    {
        PDShadingType1 shading = new(new COSDictionary());
        Assert.Null(shading.GetDomain());

        COSArray domain = COSArray.Of(0f, 1f, 0f, 1f);
        shading.SetDomain(domain);

        COSArray? result = shading.GetDomain();
        Assert.NotNull(result);
        Assert.Equal(4, result.Size());
    }

    [Fact]
    public void Type1_Domain_Null_WhenNotSet()
    {
        PDShadingType1 shading = new(new COSDictionary());
        Assert.Null(shading.GetDomain());
    }

    [Fact]
    public void Type1_Matrix_DefaultIdentity_WhenNotSet()
    {
        PDShadingType1 shading = new(new COSDictionary());
        Util.Matrix m = shading.GetMatrix();
        Assert.Equal(1f, m.GetValue(0, 0), 4);
        Assert.Equal(0f, m.GetValue(0, 1), 4);
        Assert.Equal(0f, m.GetValue(1, 0), 4);
        Assert.Equal(1f, m.GetValue(1, 1), 4);
        Assert.Equal(0f, m.GetValue(2, 0), 4);
        Assert.Equal(0f, m.GetValue(2, 1), 4);
    }

    [Fact]
    public void Type1_Matrix_RoundTrip()
    {
        PDShadingType1 shading = new(new COSDictionary());
        Util.Matrix original = new Util.Matrix(2f, 0f, 0f, 3f, 10f, 20f);
        shading.SetMatrix(original);

        Util.Matrix result = shading.GetMatrix();
        Assert.Equal(2f, result.GetValue(0, 0), 4);
        Assert.Equal(0f, result.GetValue(0, 1), 4);
        Assert.Equal(0f, result.GetValue(1, 0), 4);
        Assert.Equal(3f, result.GetValue(1, 1), 4);
        Assert.Equal(10f, result.GetValue(2, 0), 4);
        Assert.Equal(20f, result.GetValue(2, 1), 4);
    }

    // ── Type 2: Axial shading ─────────────────────────────────────────────────

    [Fact]
    public void Type2_Coords_RoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetCoords());

        COSArray coords = COSArray.Of(0f, 0f, 100f, 0f);
        shading.SetCoords(coords);

        COSArray? result = shading.GetCoords();
        Assert.NotNull(result);
        Assert.Equal(4, result.Size());
        Assert.Equal(100f, ((COSNumber)result.Get(2)!).FloatValue(), 4);
    }

    [Fact]
    public void Type2_GetBounds_ReturnsRectangle()
    {
        PDShadingType2 shading = new(new COSDictionary());
        shading.SetCoords(COSArray.Of(0f, 0f, 100f, 50f));
        Rectangle2D? bounds = shading.GetBounds(new AffineTransform(), new Matrix());
        Assert.NotNull(bounds);
        Assert.Equal(100d, bounds.Width, 4);
        Assert.Equal(50d, bounds.Height, 4);
    }

    [Fact]
    public void Type2_Domain_RoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetDomain());

        shading.SetDomain(COSArray.Of(0f, 1f));
        COSArray? result = shading.GetDomain();
        Assert.NotNull(result);
        Assert.Equal(2, result.Size());
    }

    [Fact]
    public void Type2_Extend_RoundTrip()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetExtend());

        COSArray extend = new();
        extend.Add(COSBoolean.TRUE);
        extend.Add(COSBoolean.FALSE);
        shading.SetExtend(extend);

        COSArray? result = shading.GetExtend();
        Assert.NotNull(result);
        Assert.Equal(2, result.Size());
        Assert.Same(COSBoolean.TRUE, result.Get(0));
        Assert.Same(COSBoolean.FALSE, result.Get(1));
    }

    // ── Type 3: Radial shading inherits Type 2 ───────────────────────────────

    [Fact]
    public void Type3_IsSubtypeOfType2()
    {
        PDShadingType3 shading = new(new COSDictionary());
        Assert.IsAssignableFrom<PDShadingType2>(shading);
        Assert.Equal(PDShading.SHADING_TYPE3, shading.GetShadingType());
    }

    [Fact]
    public void Type3_Coords_ViaInheritedAccessor()
    {
        PDShadingType3 shading = new(new COSDictionary());
        COSArray coords = COSArray.Of(50f, 50f, 10f, 50f, 50f, 80f);
        shading.SetCoords(coords);

        COSArray? result = shading.GetCoords();
        Assert.NotNull(result);
        Assert.Equal(6, result.Size());
    }

    [Fact]
    public void Type3_GetBounds_ReturnsRectangle()
    {
        PDShadingType3 shading = new(new COSDictionary());
        shading.SetCoords(COSArray.Of(50f, 50f, 10f, 75f, 75f, 5f));
        Rectangle2D? bounds = shading.GetBounds(new AffineTransform(), new Matrix());
        Assert.NotNull(bounds);
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    // ── Type 4: Free-form Gouraud ─────────────────────────────────────────────

    [Fact]
    public void Type4_BitsPerFlag_RoundTrip()
    {
        PDShadingType4 shading = new(new COSDictionary());
        Assert.Equal(-1, shading.GetBitsPerFlag());

        shading.SetBitsPerFlag(8);
        Assert.Equal(8, shading.GetBitsPerFlag());
    }

    [Fact]
    public void Type4_BitsPerComponent_RoundTrip()
    {
        PDShadingType4 shading = new(new COSDictionary());
        Assert.Equal(-1, shading.GetBitsPerComponent());

        shading.SetBitsPerComponent(16);
        Assert.Equal(16, shading.GetBitsPerComponent());
    }

    [Fact]
    public void Type4_BitsPerCoordinate_RoundTrip()
    {
        PDShadingType4 shading = new(new COSDictionary());
        Assert.Equal(-1, shading.GetBitsPerCoordinate());

        shading.SetBitsPerCoordinate(24);
        Assert.Equal(24, shading.GetBitsPerCoordinate());
    }

    [Fact]
    public void Type4_Decode_RoundTrip()
    {
        PDShadingType4 shading = new(new COSDictionary());
        Assert.Null(shading.GetDecodeForParameter(0));

        COSArray decode = COSArray.Of(0f, 100f, 0f, 100f, 0f, 1f, 0f, 1f, 0f, 1f);
        shading.SetDecodeValues(decode);

        PDRange? range0 = shading.GetDecodeForParameter(0);
        Assert.NotNull(range0);
        Assert.Equal(0f, range0.GetMin(), 4);
        Assert.Equal(100f, range0.GetMax(), 4);

        PDRange? range1 = shading.GetDecodeForParameter(1);
        Assert.NotNull(range1);
        Assert.Equal(0f, range1.GetMin(), 4);
        Assert.Equal(100f, range1.GetMax(), 4);
    }

    // ── Type 5: Lattice-form Gouraud ─────────────────────────────────────────

    [Fact]
    public void Type5_VerticesPerRow_RoundTrip()
    {
        PDShadingType5 shading = new(new COSDictionary());
        Assert.Equal(-1, shading.GetVerticesPerRow());

        shading.SetVerticesPerRow(4);
        Assert.Equal(4, shading.GetVerticesPerRow());
    }

    [Fact]
    public void Type5_IsSubtypeOfTriangleBased()
    {
        PDShadingType5 shading = new(new COSDictionary());
        Assert.IsAssignableFrom<PDTriangleBasedShadingType>(shading);
    }

    // ── Type 6: Coons patch mesh ──────────────────────────────────────────────

    [Fact]
    public void Type6_IsSubtypeOfMeshBased()
    {
        PDShadingType6 shading = new(new COSDictionary());
        Assert.IsAssignableFrom<PDMeshBasedShadingType>(shading);
        Assert.Equal(PDShading.SHADING_TYPE6, shading.GetShadingType());
    }

    [Fact]
    public void Type6_InheritsType4Accessors()
    {
        PDShadingType6 shading = new(new COSDictionary());
        shading.SetBitsPerFlag(4);
        Assert.Equal(4, shading.GetBitsPerFlag());
    }

    // ── Type 7: Tensor-product patch mesh ────────────────────────────────────

    [Fact]
    public void Type7_IsSubtypeOfMeshBased()
    {
        PDShadingType7 shading = new(new COSDictionary());
        Assert.IsAssignableFrom<PDMeshBasedShadingType>(shading);
        Assert.Equal(PDShading.SHADING_TYPE7, shading.GetShadingType());
    }

    // ── Function wiring ───────────────────────────────────────────────────────

    [Fact]
    public void GetFunction_ReturnsNull_WhenNotSet()
    {
        PDShadingType2 shading = new(new COSDictionary());
        Assert.Null(shading.GetFunction());
    }

    [Fact]
    public void SetGetFunction_RoundTrip_WithType2Function()
    {
        PDShadingType2 shading = new(new COSDictionary());
        COSDictionary funcDict = BuildType2FunctionDict([0f], [1f]);
        PDFunctionType2 function = new(funcDict);

        shading.SetFunction(function);
        PDFunction? retrieved = shading.GetFunction();

        Assert.NotNull(retrieved);
        Assert.Equal(2, retrieved.GetFunctionType());
    }

    [Fact]
    public void EvalFunction_SingleFunction_EvaluatesCorrectly()
    {
        PDShadingType2 shading = new(new COSDictionary());
        COSDictionary funcDict = BuildType2FunctionDict([0f], [1f]);
        funcDict.SetFloat(COSName.N, 1f);
        shading.SetFunction(new PDFunctionType2(funcDict));

        float[] result = shading.EvalFunction(0.5f);
        Assert.Single(result);
        Assert.Equal(0.5f, result[0], 4);
    }

    [Fact]
    public void EvalFunction_MultipleComponents_EvaluatesEachFunction()
    {
        PDShadingType2 shading = new(new COSDictionary());

        COSArray funcArray = new();
        // R: 0 -> 1 linear
        funcArray.Add(BuildType2FunctionDict([0f], [1f]));
        // G: always 0.5 (N=0 so C0=C1=0.5)
        funcArray.Add(BuildType2FunctionDict([0.5f], [0.5f]));
        // B: always 0
        funcArray.Add(BuildType2FunctionDict([0f], [0f]));
        shading.SetFunction(funcArray);

        float[] result = shading.EvalFunction(0.25f);
        Assert.Equal(3, result.Length);
        Assert.Equal(0.25f, result[0], 4);
        Assert.Equal(0.5f, result[1], 4);
        Assert.Equal(0f, result[2], 4);
    }

    [Fact]
    public void EvalFunction_ClampsOutOfRangeValues_ToZeroOne()
    {
        // Build a function that returns values outside [0,1]
        PDShadingType2 shading = new(new COSDictionary());
        COSDictionary funcDict = BuildType2FunctionDict([-1f], [2f]);
        funcDict.SetFloat(COSName.N, 1f);
        shading.SetFunction(new PDFunctionType2(funcDict));

        // x=1.0 -> C0 + 1*(C1-C0) = -1 + 3 = 2 -> clamped to 1
        float[] result = shading.EvalFunction(1.0f);
        Assert.Equal(1f, result[0], 4);

        // x=0.0 -> C0 = -1 -> clamped to 0
        result = shading.EvalFunction(0.0f);
        Assert.Equal(0f, result[0], 4);
    }

    // ── PDResources shading lookup ────────────────────────────────────────────

    [Fact]
    public void PDResources_GetShading_ReturnsNull_WhenNoShadingDict()
    {
        PDResources resources = new();
        Assert.Null(resources.GetShading(COSName.GetPDFName("Sh1")));
    }

    [Fact]
    public void PDResources_GetShading_ReturnsShading_WhenPresent()
    {
        COSDictionary resourceDict = new();
        COSDictionary shadingSubDict = new();
        COSDictionary shadingDict = new();
        shadingDict.SetInt(COSName.SHADING_TYPE, 2);
        shadingDict.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceRGB"));
        shadingDict.SetItem(COSName.COORDS, COSArray.Of(0f, 0f, 100f, 0f));
        shadingSubDict.SetItem(COSName.GetPDFName("Sh1"), shadingDict);
        resourceDict.SetItem(COSName.GetPDFName("Shading"), shadingSubDict);

        PDResources resources = new(resourceDict);
        PDShading? shading = resources.GetShading(COSName.GetPDFName("Sh1"));

        Assert.NotNull(shading);
        Assert.IsType<PDShadingType2>(shading);
    }

    [Fact]
    public void PDResources_GetShadingNames_ReturnsAllShadingNames()
    {
        COSDictionary resourceDict = new();
        COSDictionary shadingSubDict = new();
        shadingSubDict.SetItem(COSName.GetPDFName("Sh1"), new COSDictionary());
        shadingSubDict.SetItem(COSName.GetPDFName("Sh2"), new COSDictionary());
        resourceDict.SetItem(COSName.GetPDFName("Shading"), shadingSubDict);

        PDResources resources = new(resourceDict);
        var names = resources.GetShadingNames().ToList();
        Assert.Equal(2, names.Count);
    }

    // ── COSName constants ─────────────────────────────────────────────────────

    [Fact]
    public void COSName_ShadingConstants_HaveCorrectValues()
    {
        Assert.Equal("ShadingType", COSName.SHADING_TYPE.GetName());
        Assert.Equal("Background", COSName.BACKGROUND.GetName());
        Assert.Equal("BBox", COSName.BBOX.GetName());
        Assert.Equal("AntiAlias", COSName.ANTI_ALIAS.GetName());
        Assert.Equal("CS", COSName.CS.GetName());
        Assert.Equal("Shading", COSName.SHADING.GetName());
        Assert.Equal("Coords", COSName.COORDS.GetName());
        Assert.Equal("Extend", COSName.EXTEND.GetName());
        Assert.Equal("BitsPerCoordinate", COSName.BITS_PER_COORDINATE.GetName());
        Assert.Equal("BitsPerFlag", COSName.BITS_PER_FLAG.GetName());
        Assert.Equal("VerticesPerRow", COSName.VERTICES_PER_ROW.GetName());
        Assert.Equal("Matrix", COSName.MATRIX.GetName());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PDShading CreateShadingDict(int shadingType)
    {
        COSDictionary dict = new();
        dict.SetInt(COSName.SHADING_TYPE, shadingType);
        return PDShading.Create(dict);
    }

    private static COSDictionary BuildType2FunctionDict(float[] c0, float[] c1)
    {
        COSDictionary dict = new();
        dict.SetInt(COSName.FUNCTION_TYPE, 2);
        dict.SetItem(COSName.DOMAIN, COSArray.Of(0f, 1f));
        dict.SetItem(COSName.RANGE, COSArray.Of(0f, 1f));

        COSArray c0Array = new();
        foreach (float v in c0) c0Array.Add(new COSFloat(v));
        dict.SetItem(COSName.C0, c0Array);

        COSArray c1Array = new();
        foreach (float v in c1) c1Array.Add(new COSFloat(v));
        dict.SetItem(COSName.C1, c1Array);

        dict.SetFloat(COSName.N, 1f);
        return dict;
    }
}
