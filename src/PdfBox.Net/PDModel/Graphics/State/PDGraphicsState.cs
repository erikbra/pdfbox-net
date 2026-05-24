/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDGraphicsState.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
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

using PdfBox.Net.Util;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Graphics.State;

/// <summary>
/// The graphics state of the current page content stream.
/// Holds the current transformation matrix, text state, and other rendering
/// parameters that are pushed/popped by the PDF q/Q operators.
/// </summary>
public class PDGraphicsState
{
    private Matrix _currentTransformationMatrix;
    private PDTextState _textState;
    private float _lineWidth;
    private int _lineCap;
    private int _lineJoin;
    private float _miterLimit;
    private PDLineDashPattern _lineDashPattern;
    private float _flatness;
    private string _renderingIntent;
    private PDColorSpace _strokingColorSpace;
    private PDColorSpace _nonStrokingColorSpace;
    private PDColor _strokingColor;
    private PDColor _nonStrokingColor;
    private int _clippingWindingRule;

    /// <summary>Creates a new graphics state with identity CTM and default text state.</summary>
    public PDGraphicsState()
    {
        _currentTransformationMatrix = new Matrix();
        _textState = new PDTextState();
        _lineWidth = 1f;
        _lineCap = 0;
        _lineJoin = 0;
        _miterLimit = 10f;
        _lineDashPattern = new PDLineDashPattern();
        _flatness = 1f;
        _renderingIntent = string.Empty;
        _strokingColorSpace = new PDColorSpace("DeviceGray", 1);
        _nonStrokingColorSpace = new PDColorSpace("DeviceGray", 1);
        _strokingColor = new PDColor([0f], _strokingColorSpace);
        _nonStrokingColor = new PDColor([0f], _nonStrokingColorSpace);
        _clippingWindingRule = 1;
    }

    private PDGraphicsState(
        Matrix ctm,
        PDTextState textState,
        float lineWidth,
        int lineCap,
        int lineJoin,
        float miterLimit,
        PDLineDashPattern lineDashPattern,
        float flatness,
        string renderingIntent,
        PDColorSpace strokingColorSpace,
        PDColorSpace nonStrokingColorSpace,
        PDColor strokingColor,
        PDColor nonStrokingColor,
        int clippingWindingRule)
    {
        _currentTransformationMatrix = ctm;
        _textState = textState;
        _lineWidth = lineWidth;
        _lineCap = lineCap;
        _lineJoin = lineJoin;
        _miterLimit = miterLimit;
        _lineDashPattern = lineDashPattern;
        _flatness = flatness;
        _renderingIntent = renderingIntent;
        _strokingColorSpace = strokingColorSpace;
        _nonStrokingColorSpace = nonStrokingColorSpace;
        _strokingColor = strokingColor;
        _nonStrokingColor = nonStrokingColor;
        _clippingWindingRule = clippingWindingRule;
    }

    /// <summary>Returns the current transformation matrix.</summary>
    public Matrix GetCurrentTransformationMatrix() => _currentTransformationMatrix;

    /// <summary>Sets the current transformation matrix.</summary>
    public void SetCurrentTransformationMatrix(Matrix ctm) =>
        _currentTransformationMatrix = ctm ?? new Matrix();

    /// <summary>Returns the current text state.</summary>
    public PDTextState GetTextState() => _textState;

    public float GetLineWidth() => _lineWidth;
    public void SetLineWidth(float width) => _lineWidth = width;

    public int GetLineCap() => _lineCap;
    public void SetLineCap(int lineCap) => _lineCap = lineCap;

    public int GetLineJoin() => _lineJoin;
    public void SetLineJoin(int lineJoin) => _lineJoin = lineJoin;

    public float GetMiterLimit() => _miterLimit;
    public void SetMiterLimit(float miterLimit) => _miterLimit = miterLimit;

    public PDLineDashPattern GetLineDashPattern() => _lineDashPattern;
    public void SetLineDashPattern(PDLineDashPattern lineDashPattern) => _lineDashPattern = lineDashPattern ?? new PDLineDashPattern();

    public float GetFlatness() => _flatness;
    public void SetFlatness(float flatness) => _flatness = flatness;

    public string GetRenderingIntent() => _renderingIntent;
    public void SetRenderingIntent(string renderingIntent) => _renderingIntent = renderingIntent ?? string.Empty;

    public PDColorSpace GetStrokingColorSpace() => _strokingColorSpace;
    public void SetStrokingColorSpace(PDColorSpace colorSpace) => _strokingColorSpace = colorSpace ?? new PDColorSpace();

    public PDColorSpace GetNonStrokingColorSpace() => _nonStrokingColorSpace;
    public void SetNonStrokingColorSpace(PDColorSpace colorSpace) => _nonStrokingColorSpace = colorSpace ?? new PDColorSpace();

    public PDColor GetStrokingColor() => _strokingColor;
    public void SetStrokingColor(PDColor color) => _strokingColor = color ?? new PDColor();

    public PDColor GetNonStrokingColor() => _nonStrokingColor;
    public void SetNonStrokingColor(PDColor color) => _nonStrokingColor = color ?? new PDColor();

    public int GetClippingWindingRule() => _clippingWindingRule;
    public void SetClippingWindingRule(int clippingWindingRule) => _clippingWindingRule = clippingWindingRule;

    /// <summary>
    /// Creates a deep copy of this graphics state (as required by the PDF "q" operator).
    /// </summary>
    public PDGraphicsState Clone() =>
        new PDGraphicsState(
            _currentTransformationMatrix,
            _textState.Clone(),
            _lineWidth,
            _lineCap,
            _lineJoin,
            _miterLimit,
            new PDLineDashPattern((float[])_lineDashPattern.GetDashArray().Clone(), _lineDashPattern.GetPhaseStart()),
            _flatness,
            _renderingIntent,
            _strokingColorSpace,
            _nonStrokingColorSpace,
            new PDColor((float[])_strokingColor.GetComponents().Clone(), _strokingColor.GetColorSpace()),
            new PDColor((float[])_nonStrokingColor.GetComponents().Clone(), _nonStrokingColor.GetColorSpace()),
            _clippingWindingRule);
}
