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

    /// <summary>Creates a new graphics state with identity CTM and default text state.</summary>
    public PDGraphicsState()
    {
        _currentTransformationMatrix = new Matrix();
        _textState = new PDTextState();
    }

    private PDGraphicsState(Matrix ctm, PDTextState textState)
    {
        _currentTransformationMatrix = ctm;
        _textState = textState;
    }

    /// <summary>Returns the current transformation matrix.</summary>
    public Matrix GetCurrentTransformationMatrix() => _currentTransformationMatrix;

    /// <summary>Sets the current transformation matrix.</summary>
    public void SetCurrentTransformationMatrix(Matrix ctm) =>
        _currentTransformationMatrix = ctm ?? new Matrix();

    /// <summary>Returns the current text state.</summary>
    public PDTextState GetTextState() => _textState;

    /// <summary>
    /// Creates a deep copy of this graphics state (as required by the PDF "q" operator).
    /// </summary>
    public PDGraphicsState Clone() =>
        new PDGraphicsState(_currentTransformationMatrix, _textState.Clone());
}
