/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/blend/BlendMode.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics;

public enum BlendMode
{
    NORMAL,
    MULTIPLY,
    SCREEN,
    OVERLAY,
    DARKEN,
    LIGHTEN,
    COLOR_DODGE,
    COLOR_BURN,
    HARD_LIGHT,
    SOFT_LIGHT,
    DIFFERENCE,
    EXCLUSION,
    HUE,
    SATURATION,
    COLOR,
    LUMINOSITY
}

internal static class BlendModeExtensions
{
    public static BlendMode FromCos(COSBase? baseValue)
    {
        if (baseValue is COSName cosName && TryGetBlendMode(cosName.GetName(), out BlendMode blendMode))
        {
            return blendMode;
        }

        if (baseValue is COSArray array)
        {
            for (int i = 0; i < array.Size(); i++)
            {
                if (array.GetObject(i) is COSName name && TryGetBlendMode(name.GetName(), out blendMode))
                {
                    return blendMode;
                }
            }
        }

        return BlendMode.NORMAL;
    }

    public static COSName ToCosName(this BlendMode mode)
    {
        return COSName.GetPDFName(mode switch
        {
            BlendMode.MULTIPLY => "Multiply",
            BlendMode.SCREEN => "Screen",
            BlendMode.OVERLAY => "Overlay",
            BlendMode.DARKEN => "Darken",
            BlendMode.LIGHTEN => "Lighten",
            BlendMode.COLOR_DODGE => "ColorDodge",
            BlendMode.COLOR_BURN => "ColorBurn",
            BlendMode.HARD_LIGHT => "HardLight",
            BlendMode.SOFT_LIGHT => "SoftLight",
            BlendMode.DIFFERENCE => "Difference",
            BlendMode.EXCLUSION => "Exclusion",
            BlendMode.HUE => "Hue",
            BlendMode.SATURATION => "Saturation",
            BlendMode.COLOR => "Color",
            BlendMode.LUMINOSITY => "Luminosity",
            _ => "Normal",
        });
    }

    public static bool IsSeparableBlendMode(this BlendMode mode)
    {
        return mode is not (BlendMode.HUE or BlendMode.SATURATION or BlendMode.COLOR or BlendMode.LUMINOSITY);
    }

    internal static float BlendChannel(this BlendMode mode, float src, float dest)
    {
        return mode switch
        {
            BlendMode.MULTIPLY => src * dest,
            BlendMode.SCREEN => src + dest - (src * dest),
            BlendMode.OVERLAY => dest <= 0.5f ? 2f * dest * src : (2f * (src + dest - (src * dest))) - 1f,
            BlendMode.DARKEN => MathF.Min(src, dest),
            BlendMode.LIGHTEN => MathF.Max(src, dest),
            BlendMode.COLOR_DODGE => BlendColorDodge(src, dest),
            BlendMode.COLOR_BURN => BlendColorBurn(src, dest),
            BlendMode.HARD_LIGHT => src <= 0.5f ? 2f * dest * src : (2f * (src + dest - (src * dest))) - 1f,
            BlendMode.SOFT_LIGHT => BlendSoftLight(src, dest),
            BlendMode.DIFFERENCE => MathF.Abs(dest - src),
            BlendMode.EXCLUSION => dest + src - (2f * dest * src),
            _ => src,
        };
    }

    internal static void Blend(this BlendMode mode, ReadOnlySpan<float> src, ReadOnlySpan<float> dest, Span<float> result)
    {
        switch (mode)
        {
            case BlendMode.HUE:
            {
                Span<float> temp = stackalloc float[3];
                GetSaturationRgb(dest, src, temp);
                GetLuminosityRgb(dest, temp, result);
                break;
            }
            case BlendMode.SATURATION:
                GetSaturationRgb(src, dest, result);
                break;
            case BlendMode.COLOR:
                GetLuminosityRgb(dest, src, result);
                break;
            case BlendMode.LUMINOSITY:
                GetLuminosityRgb(src, dest, result);
                break;
            default:
                for (int i = 0; i < Math.Min(src.Length, result.Length); i++)
                {
                    result[i] = BlendChannel(mode, src[i], dest[i]);
                }
                break;
        }
    }

    private static bool TryGetBlendMode(string? name, out BlendMode blendMode)
    {
        switch ((name ?? string.Empty).ToLowerInvariant())
        {
            case "normal":
            case "compatible":
                blendMode = BlendMode.NORMAL;
                return true;
            case "multiply":
                blendMode = BlendMode.MULTIPLY;
                return true;
            case "screen":
                blendMode = BlendMode.SCREEN;
                return true;
            case "overlay":
                blendMode = BlendMode.OVERLAY;
                return true;
            case "darken":
                blendMode = BlendMode.DARKEN;
                return true;
            case "lighten":
                blendMode = BlendMode.LIGHTEN;
                return true;
            case "colordodge":
                blendMode = BlendMode.COLOR_DODGE;
                return true;
            case "colorburn":
                blendMode = BlendMode.COLOR_BURN;
                return true;
            case "hardlight":
                blendMode = BlendMode.HARD_LIGHT;
                return true;
            case "softlight":
                blendMode = BlendMode.SOFT_LIGHT;
                return true;
            case "difference":
                blendMode = BlendMode.DIFFERENCE;
                return true;
            case "exclusion":
                blendMode = BlendMode.EXCLUSION;
                return true;
            case "hue":
                blendMode = BlendMode.HUE;
                return true;
            case "saturation":
                blendMode = BlendMode.SATURATION;
                return true;
            case "color":
                blendMode = BlendMode.COLOR;
                return true;
            case "luminosity":
                blendMode = BlendMode.LUMINOSITY;
                return true;
            default:
                blendMode = BlendMode.NORMAL;
                return false;
        }
    }

    private static float BlendColorDodge(float src, float dest)
    {
        if (dest == 0f)
        {
            return 0f;
        }

        if (dest >= 1f - src)
        {
            return 1f;
        }

        return dest / (1f - src);
    }

    private static float BlendColorBurn(float src, float dest)
    {
        if (dest == 1f)
        {
            return 1f;
        }

        if (1f - dest >= src)
        {
            return 0f;
        }

        return 1f - ((1f - dest) / src);
    }

    private static float BlendSoftLight(float src, float dest)
    {
        if (src <= 0.5f)
        {
            return dest - ((1f - (2f * src)) * dest * (1f - dest));
        }

        float d = dest <= 0.25f ? (((16f * dest) - 12f) * dest + 4f) * dest : MathF.Sqrt(dest);
        return dest + ((2f * src) - 1f) * (d - dest);
    }

    private static int Get255Value(float value)
    {
        return (int)MathF.Floor(value >= 1.0f ? 255f : value * 255.0f);
    }

    private static void GetSaturationRgb(ReadOnlySpan<float> srcValues, ReadOnlySpan<float> dstValues, Span<float> result)
    {
        int rd = Get255Value(dstValues[0]);
        int gd = Get255Value(dstValues[1]);
        int bd = Get255Value(dstValues[2]);

        int minb = Math.Min(rd, Math.Min(gd, bd));
        int maxb = Math.Max(rd, Math.Max(gd, bd));
        if (minb == maxb)
        {
            result[0] = gd / 255.0f;
            result[1] = gd / 255.0f;
            result[2] = gd / 255.0f;
            return;
        }

        int rs = Get255Value(srcValues[0]);
        int gs = Get255Value(srcValues[1]);
        int bs = Get255Value(srcValues[2]);

        int mins = Math.Min(rs, Math.Min(gs, bs));
        int maxs = Math.Max(rs, Math.Max(gs, bs));

        int scale = ((maxs - mins) << 16) / (maxb - minb);
        int y = (rd * 77 + gd * 151 + bd * 28 + 0x80) >> 8;
        int r = y + ((((rd - y) * scale) + 0x8000) >> 16);
        int g = y + ((((gd - y) * scale) + 0x8000) >> 16);
        int b = y + ((((bd - y) * scale) + 0x8000) >> 16);

        if (((r | g | b) & 0x100) == 0x100)
        {
            int min = Math.Min(r, Math.Min(g, b));
            int max = Math.Max(r, Math.Max(g, b));
            int scalemin = min < 0 ? (y << 16) / (y - min) : 0x10000;
            int scalemax = max > 255 ? ((255 - y) << 16) / (max - y) : 0x10000;
            scale = Math.Min(scalemin, scalemax);
            r = y + (((r - y) * scale + 0x8000) >> 16);
            g = y + (((g - y) * scale + 0x8000) >> 16);
            b = y + (((b - y) * scale + 0x8000) >> 16);
        }

        result[0] = r / 255.0f;
        result[1] = g / 255.0f;
        result[2] = b / 255.0f;
    }

    private static void GetLuminosityRgb(ReadOnlySpan<float> srcValues, ReadOnlySpan<float> dstValues, Span<float> result)
    {
        int rd = Get255Value(dstValues[0]);
        int gd = Get255Value(dstValues[1]);
        int bd = Get255Value(dstValues[2]);
        int rs = Get255Value(srcValues[0]);
        int gs = Get255Value(srcValues[1]);
        int bs = Get255Value(srcValues[2]);
        int delta = ((rs - rd) * 77 + (gs - gd) * 151 + (bs - bd) * 28 + 0x80) >> 8;
        int r = rd + delta;
        int g = gd + delta;
        int b = bd + delta;

        if (((r | g | b) & 0x100) == 0x100)
        {
            int y = (rs * 77 + gs * 151 + bs * 28 + 0x80) >> 8;
            int scale;
            if (delta > 0)
            {
                int max = Math.Max(r, Math.Max(g, b));
                scale = max == y ? 0 : ((255 - y) << 16) / (max - y);
            }
            else
            {
                int min = Math.Min(r, Math.Min(g, b));
                scale = y == min ? 0 : (y << 16) / (y - min);
            }

            r = y + (((r - y) * scale + 0x8000) >> 16);
            g = y + (((g - y) * scale + 0x8000) >> 16);
            b = y + (((b - y) * scale + 0x8000) >> 16);
        }

        result[0] = r / 255.0f;
        result[1] = g / 255.0f;
        result[2] = b / 255.0f;
    }
}
