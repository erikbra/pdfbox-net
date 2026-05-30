/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDStandardAttributeObject.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A standard attribute object.
/// </summary>
/// <remarks>Author: Johannes Koch</remarks>
public abstract class PDStandardAttributeObject : PDAttributeObject
{
    /// <summary>
    /// An "unspecified" default float value.
    /// </summary>
    protected const float Unspecified = -1f;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected PDStandardAttributeObject()
    {
    }

    /// <summary>
    /// Creates a new standard attribute object with a given dictionary.
    /// </summary>
    /// <param name="dictionary">the dictionary</param>
    protected PDStandardAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Returns true if the attribute with the given name is specified in this attribute object.
    /// </summary>
    /// <param name="name">the attribute name</param>
    public bool IsSpecified(string name) =>
        GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name)) != null;

    /// <summary>
    /// Gets a string attribute value.
    /// </summary>
    protected string? GetString(string name) =>
        GetCOSObject().GetString(name);

    /// <summary>
    /// Sets a string attribute value.
    /// </summary>
    protected void SetString(string name, string value)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetString(name, value);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Gets an array of strings.
    /// </summary>
    protected string[]? GetArrayOfString(string name)
    {
        COSBase? v = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (v is COSArray array)
        {
            var strings = new string[array.Size()];
            for (int i = 0; i < array.Size(); i++)
            {
                strings[i] = ((COSName)array.GetObject(i)!).GetName();
            }
            return strings;
        }
        return null;
    }

    /// <summary>
    /// Sets an array of strings.
    /// </summary>
    protected void SetArrayOfString(string name, string[] values)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        var array = new COSArray();
        foreach (string value in values)
        {
            array.Add(new COSString(value));
        }
        GetCOSObject().SetItem(COSName.GetPDFName(name), array);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Gets a name value.
    /// </summary>
    protected string? GetName(string name) =>
        GetCOSObject().GetNameAsString(COSName.GetPDFName(name));

    /// <summary>
    /// Gets a name value with a default.
    /// </summary>
    protected string GetName(string name, string defaultValue) =>
        GetCOSObject().GetNameAsString(COSName.GetPDFName(name)) ?? defaultValue;

    /// <summary>
    /// Gets a name value or array of name values.
    /// </summary>
    /// <returns>a <see cref="string"/> or an array of strings</returns>
    protected object GetNameOrArrayOfName(string name, string defaultValue)
    {
        COSBase? v = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (v is COSArray array)
        {
            var names = new string[array.Size()];
            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? item = array.GetObject(i);
                if (item is COSName cosName)
                {
                    names[i] = cosName.GetName();
                }
            }
            return names;
        }
        if (v is COSName nameVal)
        {
            return nameVal.GetName();
        }
        return defaultValue;
    }

    /// <summary>
    /// Sets a name value.
    /// </summary>
    protected void SetName(string name, string value)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetName(COSName.GetPDFName(name), value);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Sets an array of name values.
    /// </summary>
    protected void SetArrayOfName(string name, string[] values)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        var array = new COSArray();
        foreach (string value in values)
        {
            array.Add(COSName.GetPDFName(value));
        }
        GetCOSObject().SetItem(COSName.GetPDFName(name), array);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Gets a number or a name value.
    /// </summary>
    /// <returns>a <see cref="float"/> or a <see cref="string"/></returns>
    protected object? GetNumberOrName(string name, string defaultValue)
    {
        COSBase? value = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (value is COSNumber number) return number.FloatValue();
        if (value is COSName cosName)  return cosName.GetName();
        return defaultValue;
    }

    /// <summary>
    /// Gets an integer.
    /// </summary>
    protected int GetInteger(string name, int defaultValue) =>
        GetCOSObject().GetInt(COSName.GetPDFName(name), defaultValue);

    /// <summary>
    /// Sets an integer.
    /// </summary>
    protected void SetInteger(string name, int value)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetInt(COSName.GetPDFName(name), value);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Gets a number value.
    /// </summary>
    protected float GetNumber(string name, float defaultValue) =>
        GetCOSObject().GetFloat(COSName.GetPDFName(name), defaultValue);

    /// <summary>
    /// Gets a number value.
    /// </summary>
    protected float GetNumber(string name) =>
        GetCOSObject().GetFloat(COSName.GetPDFName(name));

    /// <summary>
    /// Gets a number or an array of numbers.
    /// </summary>
    /// <returns>a <see cref="float"/> or an array of floats, or null when unspecified</returns>
    protected object? GetNumberOrArrayOfNumber(string name, float defaultValue)
    {
        COSBase? v = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (v is COSArray array)
        {
            var values = new float[array.Size()];
            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? item = array.GetObject(i);
                if (item is COSNumber num)
                {
                    values[i] = num.FloatValue();
                }
            }
            return values;
        }
        if (v is COSNumber number) return number.FloatValue();
        if (MathF.Abs(defaultValue - Unspecified) < float.Epsilon) return null;
        return defaultValue;
    }

    /// <summary>
    /// Sets a float number.
    /// </summary>
    protected void SetNumber(string name, float value)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetFloat(COSName.GetPDFName(name), value);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Sets an integer number.
    /// </summary>
    protected void SetNumber(string name, int value)
    {
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetInt(COSName.GetPDFName(name), value);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Sets an array of float numbers.
    /// </summary>
    protected void SetArrayOfNumber(string name, float[] values)
    {
        var array = new COSArray();
        foreach (float value in values)
        {
            array.Add(new COSFloat(value));
        }
        COSBase? oldBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        GetCOSObject().SetItem(COSName.GetPDFName(name), array);
        COSBase? newBase = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        PotentiallyNotifyChanged(oldBase, newBase);
    }

    /// <summary>
    /// Gets a colour.
    /// </summary>
    protected PDGamma? GetColor(string name)
    {
        COSBase? c = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (c is COSArray array) return new PDGamma(array);
        return null;
    }

    /// <summary>
    /// Gets a single colour or four colours.
    /// </summary>
    /// <returns>a <see cref="PDGamma"/> or a <see cref="PDFourColours"/>, or null</returns>
    protected object? GetColorOrFourColors(string name)
    {
        COSBase? v = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (v is not COSArray array) return null;
        if (array.Size() == 3) return new PDGamma(array);
        if (array.Size() == 4) return new PDFourColours(array);
        return null;
    }

    /// <summary>
    /// Sets a colour.
    /// </summary>
    protected void SetColor(string name, PDGamma? value)
    {
        COSBase? oldValue = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (value != null)
            GetCOSObject().SetItem(COSName.GetPDFName(name), value.GetCOSObject());
        else
            GetCOSObject().RemoveItem(COSName.GetPDFName(name));
        COSBase? newValue = value?.GetCOSObject();
        PotentiallyNotifyChanged(oldValue, newValue);
    }

    /// <summary>
    /// Sets four colours.
    /// </summary>
    protected void SetFourColors(string name, PDFourColours? value)
    {
        COSBase? oldValue = GetCOSObject().GetDictionaryObject(COSName.GetPDFName(name));
        if (value != null)
            GetCOSObject().SetItem(COSName.GetPDFName(name), value.GetCOSObject());
        else
            GetCOSObject().RemoveItem(COSName.GetPDFName(name));
        COSBase? newValue = value?.GetCOSObject();
        PotentiallyNotifyChanged(oldValue, newValue);
    }
}
