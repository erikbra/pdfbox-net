/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/blend/BlendComposite.java
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

namespace PdfBox.Net.PDModel.Graphics;

public sealed class BlendComposite
{
    private BlendComposite(BlendMode blendMode, float constantAlpha)
    {
        BlendMode = blendMode;
        ConstantAlpha = constantAlpha;
    }

    public BlendMode BlendMode { get; }

    public float ConstantAlpha { get; }

    public static BlendComposite GetInstance(BlendMode blendMode, float constantAlpha)
    {
        return new BlendComposite(blendMode, Clamp01(constantAlpha));
    }

    public float Compose(
        ReadOnlySpan<float> src,
        float srcAlpha,
        ReadOnlySpan<float> dst,
        float dstAlpha,
        Span<float> result,
        bool subtractive = false)
    {
        if (src.Length != dst.Length)
        {
            throw new ArgumentException("Source and destination component counts must match.");
        }

        if (result.Length < dst.Length)
        {
            throw new ArgumentException("Result span must be at least as large as the destination component count.");
        }

        if (!BlendMode.IsSeparableBlendMode() && src.Length < 3)
        {
            throw new ArgumentException("Non-separable blend modes require at least three color components.");
        }

        srcAlpha = Clamp01(srcAlpha) * ConstantAlpha;
        dstAlpha = Clamp01(dstAlpha);

        float resultAlpha = dstAlpha + srcAlpha - (srcAlpha * dstAlpha);
        float srcAlphaRatio = resultAlpha > 0f ? srcAlpha / resultAlpha : 0f;

        if (BlendMode.IsSeparableBlendMode())
        {
            for (int i = 0; i < dst.Length; i++)
            {
                float srcValue = Clamp01(src[i]);
                float dstValue = Clamp01(dst[i]);
                if (subtractive)
                {
                    srcValue = 1f - srcValue;
                    dstValue = 1f - dstValue;
                }

                float value = BlendMode.BlendChannel(srcValue, dstValue);
                value = srcValue + dstAlpha * (value - srcValue);
                value = dstValue + srcAlphaRatio * (value - dstValue);

                if (subtractive)
                {
                    value = 1f - value;
                }

                result[i] = Clamp01(value);
            }
        }
        else
        {
            Span<float> rgbResult = stackalloc float[3];
            BlendMode.Blend(src[..3], dst[..3], rgbResult);

            for (int i = 0; i < 3; i++)
            {
                float srcValue = Clamp01(src[i]);
                float dstValue = Clamp01(dst[i]);
                float value = Clamp01(rgbResult[i]);
                value = srcValue + dstAlpha * (value - srcValue);
                value = dstValue + srcAlphaRatio * (value - dstValue);
                result[i] = Clamp01(value);
            }

            for (int i = 3; i < dst.Length; i++)
            {
                result[i] = Clamp01(dst[i]);
            }
        }

        return resultAlpha;
    }

    private static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
}
