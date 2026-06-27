/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDSoftMask.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.State;

public sealed partial class PDSoftMask : COSObjectable
{
    private static readonly COSName NoneName = COSName.GetPDFName("None");
    private static readonly COSName SName = COSName.GetPDFName("S");
    private static readonly COSName GName = COSName.GetPDFName("G");
    private static readonly COSName BcName = COSName.GetPDFName("BC");
    private static readonly COSName TrName = COSName.GetPDFName("TR");

    private readonly COSDictionary _dictionary;
    private COSName? _subType;
    private PDTransparencyGroup? _group;
    private COSArray? _backdropColor;
    private PDFunction? _transferFunction;
    private Matrix? _initialTransformationMatrix;

    public PDSoftMask(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public static PDSoftMask? Create(COSBase? dictionary)
    {
        if (dictionary is null)
        {
            return null;
        }

        if (dictionary is COSName name && name.Equals(NoneName))
        {
            return null;
        }

        return dictionary is COSDictionary dict ? new PDSoftMask(dict) : null;
    }

    public COSDictionary GetCOSObject() => _dictionary;
    COSBase COSObjectable.GetCOSObject() => _dictionary;

    public COSName? GetSubType() => _subType ??= _dictionary.GetCOSName(SName);

    public PDTransparencyGroup? GetGroup()
    {
        if (_group is not null)
        {
            return _group;
        }

        COSBase? groupBase = _dictionary.GetDictionaryObject(GName);
        if (groupBase is COSStream stream)
        {
            _group = new PDTransparencyGroup(stream);
        }

        return _group;
    }

    public COSArray? GetBackdropColor() => _backdropColor ??= _dictionary.GetCOSArray(BcName);

    public PDFunction? GetTransferFunction()
    {
        if (_transferFunction is not null)
        {
            return _transferFunction;
        }

        COSBase? tf = _dictionary.GetDictionaryObject(TrName);
        _transferFunction = tf is null ? null : PDFunction.Create(tf);
        return _transferFunction;
    }

    public void SetInitialTransformationMatrix(Matrix matrix)
    {
        _initialTransformationMatrix = matrix;
    }

    public Matrix? GetInitialTransformationMatrix() => _initialTransformationMatrix;
}
