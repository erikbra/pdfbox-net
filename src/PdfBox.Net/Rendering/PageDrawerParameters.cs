/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawerParameters.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Parameters for a <c>PageDrawer</c>. This class allows <see cref="PDFRenderer"/> and a page drawer
/// implementation to share private implementation data in a future-proof manner.
/// </summary>
public sealed class PageDrawerParameters
{
    private readonly PDFRenderer _renderer;
    private readonly PDPage _page;
    private readonly bool _subsamplingAllowed;
    private readonly RenderDestination _destination;
    private readonly RenderingHints? _renderingHints;
    private readonly float _imageDownscalingOptimizationThreshold;
    private readonly Dictionary<RenderingIntent, PDColorManagementContext?> _colorManagementContexts = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDrawerParameters"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="page">The page.</param>
    /// <param name="subsamplingAllowed">Whether image subsampling is allowed.</param>
    /// <param name="destination">The render destination.</param>
    /// <param name="renderingHints">The rendering hints.</param>
    /// <param name="imageDownscalingOptimizationThreshold">The image downscaling optimization threshold.</param>
    internal PageDrawerParameters(
        PDFRenderer renderer,
        PDPage page,
        bool subsamplingAllowed,
        RenderDestination destination,
        RenderingHints? renderingHints,
        float imageDownscalingOptimizationThreshold)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _subsamplingAllowed = subsamplingAllowed;
        _destination = destination;
        _renderingHints = renderingHints;
        _imageDownscalingOptimizationThreshold = imageDownscalingOptimizationThreshold;
    }

    /// <summary>
    /// Returns the page.
    /// </summary>
    /// <returns>The page.</returns>
    public PDPage GetPage()
    {
        return _page;
    }

    internal PDFRenderer GetRenderer()
    {
        return _renderer;
    }

    internal PDColorManagementContext? GetColorManagementContext(RenderingIntent renderingIntent)
    {
        if (!_colorManagementContexts.TryGetValue(renderingIntent, out PDColorManagementContext? context))
        {
            context = PDColorManagementContext.Create(_renderer.GetDocument(), renderingIntent);
            _colorManagementContexts[renderingIntent] = context;
        }

        return context;
    }

    /// <summary>
    /// Returns whether to allow subsampling of images.
    /// </summary>
    /// <returns><see langword="true"/> if subsampling of images is allowed.</returns>
    public bool IsSubsamplingAllowed()
    {
        return _subsamplingAllowed;
    }

    /// <summary>
    /// Returns the destination.
    /// </summary>
    /// <returns>The destination.</returns>
    public RenderDestination GetDestination()
    {
        return _destination;
    }

    /// <summary>
    /// Returns the rendering hints.
    /// </summary>
    /// <returns>The rendering hints.</returns>
    public RenderingHints? GetRenderingHints()
    {
        return _renderingHints;
    }

    /// <summary>
    /// Returns the image downscaling optimization threshold.
    /// </summary>
    /// <returns>The image downscaling optimization threshold.</returns>
    public float GetImageDownscalingOptimizationThreshold()
    {
        return _imageDownscalingOptimizationThreshold;
    }
}

/// <summary>
/// Minimal rendering hints wrapper used by the rendering package.
/// </summary>
public sealed class RenderingHints : Dictionary<object, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderingHints"/> class.
    /// </summary>
    public RenderingHints()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderingHints"/> class from existing values.
    /// </summary>
    /// <param name="dictionary">The initial hint values.</param>
    public RenderingHints(IDictionary<object, object> dictionary)
        : base(dictionary)
    {
    }
}
