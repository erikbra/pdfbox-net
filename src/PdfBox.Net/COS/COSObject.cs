/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSObject.java
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

namespace PdfBox.Net.COS;

public class COSObject : COSBase, COSUpdateInfo
{
    private COSBase? _baseObject;
    private bool _isDereferenced;
    private readonly COSUpdateState _updateState;

    public COSObject(COSBase? obj)
    {
        _updateState = new(this);
        _baseObject = obj;
        _isDereferenced = obj is not null;
    }

    public COSObject(COSBase? obj, COSObjectKey objectKey)
    {
        _updateState = new(this);
        _baseObject = obj;
        _isDereferenced = obj is not null;
        SetKey(objectKey);
    }

    public COSObject(COSObjectKey key)
    {
        _updateState = new(this);
        SetKey(key);
    }

    public bool IsObjectNull()
    {
        return _baseObject is null;
    }

    public COSBase? GetObject()
    {
        return _baseObject;
    }

    public void SetObject(COSBase? baseObject)
    {
        if (!ReferenceEquals(_baseObject, baseObject))
        {
            _updateState.Update(baseObject);
        }

        _baseObject = baseObject;
        _isDereferenced = baseObject is not null;
    }

    public void SetToNull()
    {
        if (_baseObject is not null)
        {
            _updateState.Update();
        }

        _baseObject = COSNull.NULL;
        _isDereferenced = true;
    }

    public override string ToString()
    {
        return $"COSObject{{{GetKey()}}}";
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromObject(this);
    }

    public bool IsDereferenced()
    {
        return _isDereferenced;
    }

    public COSUpdateState GetUpdateState()
    {
        return _updateState;
    }
}
