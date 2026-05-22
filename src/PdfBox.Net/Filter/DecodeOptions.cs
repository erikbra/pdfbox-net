/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/DecodeOptions.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

using System.Drawing;

namespace PdfBox.Net.Filter;

/// <summary>
/// Options that may be passed to a <see cref="Filter"/> to request special handling when decoding the stream.
/// Filters may not honor some or all of the specified options, and so callers should check the honored flag if
/// further processing relies on the options being used.
/// </summary>
public class DecodeOptions
{
    /// <summary>
    /// Default decode options. The honored flag for this instance is always true, as it represents
    /// the default behavior.
    /// </summary>
    public static DecodeOptions DEFAULT { get; } = new FinalDecodeOptions(true);

    private Rectangle? _sourceRegion;
    // A value of 1 means no subsampling.
    private int _subsamplingX = 1;
    // A value of 1 means no subsampling.
    private int _subsamplingY = 1;
    private int _subsamplingOffsetX;
    private int _subsamplingOffsetY;
    private bool _filterSubsampled;

    /// <summary>
    /// Constructs an empty <see cref="DecodeOptions"/> instance.
    /// </summary>
    public DecodeOptions()
    {
    }

    /// <summary>
    /// Constructs an instance specifying the region of the image that should be decoded.
    /// </summary>
    /// <param name="sourceRegion">Region of the source image that should be decoded.</param>
    public DecodeOptions(Rectangle sourceRegion)
    {
        _sourceRegion = sourceRegion;
    }

    /// <summary>
    /// Constructs an instance specifying the region of the image that should be decoded.
    /// </summary>
    /// <param name="x">X-coordinate of the top-left corner of the region to be decoded.</param>
    /// <param name="y">Y-coordinate of the top-left corner of the region to be decoded.</param>
    /// <param name="width">Width of the region to be decoded.</param>
    /// <param name="height">Height of the region to be decoded.</param>
    public DecodeOptions(int x, int y, int width, int height) : this(new Rectangle(x, y, width, height))
    {
    }

    /// <summary>
    /// Constructs an instance specifying that the image should be decoded using subsampling.
    /// </summary>
    /// <param name="subsampling">The number of rows and columns to advance in the source for each decoded pixel.</param>
    public DecodeOptions(int subsampling)
    {
        _subsamplingX = subsampling;
        _subsamplingY = subsampling;
    }

    public Rectangle? GetSourceRegion()
    {
        return _sourceRegion;
    }

    public virtual void SetSourceRegion(Rectangle? sourceRegion)
    {
        _sourceRegion = sourceRegion;
    }

    public int GetSubsamplingX()
    {
        return _subsamplingX;
    }

    public virtual void SetSubsamplingX(int ssX)
    {
        _subsamplingX = ssX;
    }

    public int GetSubsamplingY()
    {
        return _subsamplingY;
    }

    public virtual void SetSubsamplingY(int ssY)
    {
        _subsamplingY = ssY;
    }

    public int GetSubsamplingOffsetX()
    {
        return _subsamplingOffsetX;
    }

    public virtual void SetSubsamplingOffsetX(int ssOffsetX)
    {
        _subsamplingOffsetX = ssOffsetX;
    }

    public int GetSubsamplingOffsetY()
    {
        return _subsamplingOffsetY;
    }

    public virtual void SetSubsamplingOffsetY(int ssOffsetY)
    {
        _subsamplingOffsetY = ssOffsetY;
    }

    /// <summary>
    /// Flag used by the filter to specify if it performed subsampling.
    /// </summary>
    public bool IsFilterSubsampled()
    {
        return _filterSubsampled;
    }

    /// <summary>
    /// Used internally by filters to signal they have applied subsampling as requested by this options instance.
    /// </summary>
    /// <param name="filterSubsampled">Value specifying if the filter could meet the requested options.</param>
    internal virtual void SetFilterSubsampled(bool filterSubsampled)
    {
        _filterSubsampled = filterSubsampled;
    }

    private sealed class FinalDecodeOptions : DecodeOptions
    {
        internal FinalDecodeOptions(bool filterSubsampled)
        {
            base.SetFilterSubsampled(filterSubsampled);
        }

        public override void SetSourceRegion(Rectangle? sourceRegion)
        {
            throw new InvalidOperationException("This instance may not be modified.");
        }

        public override void SetSubsamplingX(int ssX)
        {
            throw new InvalidOperationException("This instance may not be modified.");
        }

        public override void SetSubsamplingY(int ssY)
        {
            throw new InvalidOperationException("This instance may not be modified.");
        }

        public override void SetSubsamplingOffsetX(int ssOffsetX)
        {
            throw new InvalidOperationException("This instance may not be modified.");
        }

        public override void SetSubsamplingOffsetY(int ssOffsetY)
        {
            throw new InvalidOperationException("This instance may not be modified.");
        }

        internal override void SetFilterSubsampled(bool filterSubsampled)
        {
            // Intentionally ignored to keep DEFAULT immutable and always honored.
        }
    }
}
