/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShading.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// A Shading Resource.
/// </summary>
public abstract class PDShading : COSObjectable
{
    private readonly COSDictionary _dictionary;
    private COSArray? _background;
    private PDRectangle? _bBox;
    private PDColorSpace? _colorSpace;
    private PDFunction? _function;
    private PDFunction[]? _functionArray;

    /// <summary>shading type 1 = function based shading.</summary>
    public const int SHADING_TYPE1 = 1;

    /// <summary>shading type 2 = axial shading.</summary>
    public const int SHADING_TYPE2 = 2;

    /// <summary>shading type 3 = radial shading.</summary>
    public const int SHADING_TYPE3 = 3;

    /// <summary>shading type 4 = Free-Form Gouraud-Shaded Triangle Meshes.</summary>
    public const int SHADING_TYPE4 = 4;

    /// <summary>shading type 5 = Lattice-Form Gouraud-Shaded Triangle Meshes.</summary>
    public const int SHADING_TYPE5 = 5;

    /// <summary>shading type 6 = Coons Patch Meshes.</summary>
    public const int SHADING_TYPE6 = 6;

    /// <summary>shading type 7 = Tensor-Product Patch Meshes.</summary>
    public const int SHADING_TYPE7 = 7;

    /// <summary>Default constructor.</summary>
    protected PDShading()
    {
        _dictionary = new COSDictionary();
    }

    /// <summary>Constructor using the given shading dictionary.</summary>
    /// <param name="shadingDictionary">the dictionary for this shading</param>
    protected PDShading(COSDictionary shadingDictionary)
    {
        _dictionary = shadingDictionary;
    }

    /// <summary>This will get the underlying dictionary.</summary>
    /// <returns>the dictionary for this shading.</returns>
    public COSDictionary GetCOSObject() => _dictionary;

    COSBase COSObjectable.GetCOSObject() => _dictionary;

    /// <summary>This will return the PDF type name for this resource.</summary>
    /// <returns>the PDF type name "Shading"</returns>
    public new string GetType() => COSName.SHADING.GetName();

    /// <summary>This will set the shading type.</summary>
    /// <param name="shadingType">the new shading type</param>
    public void SetShadingType(int shadingType)
    {
        _dictionary.SetInt(COSName.SHADING_TYPE, shadingType);
    }

    /// <summary>This will return the shading type.</summary>
    /// <returns>the shading type</returns>
    public abstract int GetShadingType();

    /// <summary>
    /// Calculate a bounding rectangle around this shading in user space.
    /// </summary>
    /// <param name="xform">affine transformation</param>
    /// <param name="matrix">pattern matrix</param>
    /// <returns>bounding rectangle or null when not available</returns>
    public virtual Rectangle2D? GetBounds(AffineTransform xform, Matrix matrix)
    {
        return null;
    }

    /// <summary>This will set the background.</summary>
    /// <param name="newBackground">the new background</param>
    public void SetBackground(COSArray newBackground)
    {
        _background = newBackground;
        _dictionary.SetItem(COSName.BACKGROUND, newBackground);
    }

    /// <summary>This will return the background.</summary>
    /// <returns>the background</returns>
    public COSArray? GetBackground()
    {
        _background ??= _dictionary.GetCOSArray(COSName.BACKGROUND);
        return _background;
    }

    /// <summary>
    /// An array of four numbers in the form coordinate system (see below),
    /// giving the coordinates of the left, bottom, right, and top edges,
    /// respectively, of the shading's bounding box.
    /// </summary>
    /// <returns>the BBox of the form</returns>
    public PDRectangle? GetBBox()
    {
        if (_bBox == null)
        {
            COSArray? array = _dictionary.GetCOSArray(COSName.BBOX);
            if (array != null)
            {
                _bBox = new PDRectangle(array);
            }
        }
        return _bBox;
    }

    /// <summary>This will set the BBox (bounding box) for this Shading.</summary>
    /// <param name="newBBox">the new BBox</param>
    public void SetBBox(PDRectangle? newBBox)
    {
        _bBox = newBBox;
        if (_bBox == null)
        {
            _dictionary.RemoveItem(COSName.BBOX);
        }
        else
        {
            _dictionary.SetItem(COSName.BBOX, _bBox.GetCOSArray());
        }
    }

    /// <summary>This will set the AntiAlias value.</summary>
    /// <param name="antiAlias">the new AntiAlias value</param>
    public void SetAntiAlias(bool antiAlias)
    {
        _dictionary.SetBoolean(COSName.ANTI_ALIAS, antiAlias);
    }

    /// <summary>This will return the AntiAlias value.</summary>
    /// <returns>the AntiAlias value</returns>
    public bool GetAntiAlias()
    {
        return _dictionary.GetBoolean(COSName.ANTI_ALIAS, false);
    }

    /// <summary>This will get the color space or null if none exists.</summary>
    /// <returns>the color space for the shading</returns>
    /// <exception cref="IOException">if there is an error getting the color space</exception>
    public PDColorSpace GetColorSpace()
    {
        if (_colorSpace == null)
        {
            COSBase? colorSpaceDictionary = _dictionary.GetDictionaryObject(COSName.CS, COSName.COLORSPACE);
            _colorSpace = PDColorSpace.Create(colorSpaceDictionary);
        }
        return _colorSpace;
    }

    /// <summary>This will set the color space for the shading.</summary>
    /// <param name="colorSpace">the color space</param>
    public void SetColorSpace(PDColorSpace? colorSpace)
    {
        _colorSpace = colorSpace;
        if (colorSpace != null)
        {
            _dictionary.SetItem(COSName.COLORSPACE, colorSpace.GetCOSObject());
        }
        else
        {
            _dictionary.RemoveItem(COSName.COLORSPACE);
        }
    }

    /// <summary>
    /// Create the correct PD Model shading based on the COS base shading.
    /// </summary>
    /// <param name="shadingDictionary">the COS shading dictionary</param>
    /// <returns>the newly created shading resources object</returns>
    /// <exception cref="IOException">if we are unable to create the PDShading object</exception>
    public static PDShading Create(COSDictionary shadingDictionary)
    {
        int shadingType = shadingDictionary.GetInt(COSName.SHADING_TYPE, 0);
        return shadingType switch
        {
            SHADING_TYPE1 => new PDShadingType1(shadingDictionary),
            SHADING_TYPE2 => new PDShadingType2(shadingDictionary),
            SHADING_TYPE3 => new PDShadingType3(shadingDictionary),
            SHADING_TYPE4 => new PDShadingType4(shadingDictionary),
            SHADING_TYPE5 => new PDShadingType5(shadingDictionary),
            SHADING_TYPE6 => new PDShadingType6(shadingDictionary),
            SHADING_TYPE7 => new PDShadingType7(shadingDictionary),
            _ => throw new IOException($"Error: Unknown shading type {shadingType}")
        };
    }

    /// <summary>This will set the function for the color conversion.</summary>
    /// <param name="newFunction">the new function</param>
    public void SetFunction(PDFunction newFunction)
    {
        _functionArray = null;
        _function = newFunction;
        GetCOSObject().SetItem(COSName.FUNCTION, newFunction);
    }

    /// <summary>This will set the functions COSArray for the color conversion.</summary>
    /// <param name="newFunctions">the new COSArray containing all functions</param>
    public void SetFunction(COSArray newFunctions)
    {
        _functionArray = null;
        _function = null;
        GetCOSObject().SetItem(COSName.FUNCTION, newFunctions);
    }

    /// <summary>This will return the function used to convert the color values.</summary>
    /// <returns>the function</returns>
    /// <exception cref="IOException">if we were not able to create the function.</exception>
    public PDFunction? GetFunction()
    {
        if (_function == null)
        {
            COSBase? dictionaryFunctionObject = GetCOSObject().GetDictionaryObject(COSName.FUNCTION);
            if (dictionaryFunctionObject != null)
            {
                _function = PDFunction.Create(dictionaryFunctionObject);
            }
        }
        return _function;
    }

    /// <summary>
    /// Convert the input value using the functions of the shading dictionary.
    /// </summary>
    /// <param name="inputValue">the input value</param>
    /// <returns>the output values</returns>
    /// <exception cref="IOException">thrown if something went wrong</exception>
    public float[] EvalFunction(float inputValue)
    {
        return EvalFunction([inputValue]);
    }

    /// <summary>
    /// Convert the input values using the functions of the shading dictionary.
    /// </summary>
    /// <param name="input">the input values</param>
    /// <returns>the output values</returns>
    /// <exception cref="IOException">thrown if something went wrong</exception>
    public float[] EvalFunction(float[] input)
    {
        PDFunction[] functions = GetFunctionsArray();
        int numberOfFunctions = functions.Length;
        float[] returnValues;
        if (numberOfFunctions == 1)
        {
            returnValues = functions[0].Eval(input);
        }
        else
        {
            returnValues = new float[numberOfFunctions];
            for (int i = 0; i < numberOfFunctions; i++)
            {
                float[] newValue = functions[i].Eval(input);
                returnValues[i] = newValue[0];
            }
        }
        // From the PDF spec:
        // "If the value returned by the function for a given colour component
        // is out of range, it shall be adjusted to the nearest valid value."
        for (int i = 0; i < returnValues.Length; ++i)
        {
            if (returnValues[i] < 0)
            {
                returnValues[i] = 0;
            }
            else if (returnValues[i] > 1)
            {
                returnValues[i] = 1;
            }
        }
        return returnValues;
    }

    private PDFunction[] GetFunctionsArray()
    {
        if (_functionArray == null)
        {
            COSBase? functionObject = GetCOSObject().GetDictionaryObject(COSName.FUNCTION);
            if (functionObject is COSDictionary)
            {
                _functionArray = [PDFunction.Create(functionObject)];
            }
            else if (functionObject is COSArray functionCOSArray)
            {
                int numberOfFunctions = functionCOSArray.Size();
                _functionArray = new PDFunction[numberOfFunctions];
                for (int i = 0; i < numberOfFunctions; i++)
                {
                    _functionArray[i] = PDFunction.Create(functionCOSArray.Get(i)!);
                }
            }
            else
            {
                throw new IOException("mandatory /Function element must be a dictionary or an array");
            }
        }
        return _functionArray;
    }

    /// <summary>
    /// Returns a paint abstraction for this shading.
    /// </summary>
    /// <param name="matrix">pattern-to-user matrix</param>
    /// <returns>paint instance</returns>
    public abstract IPaint ToPaint(Matrix matrix);
}
