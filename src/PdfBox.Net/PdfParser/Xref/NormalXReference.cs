/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/xref/NormalXReference.java
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

namespace PdfBox.Net.PdfParser.Xref;

public class NormalXReference : AbstractXReference
{
    private static readonly COSName ObjStmName = COSName.GetPDFName("ObjStm");

    private readonly long _byteOffset;
    private readonly COSObjectKey _key;
    private readonly COSBase _object;
    private readonly bool _objectStream;

    public NormalXReference(long byteOffset, COSObjectKey key, COSBase obj)
        : base(XReferenceType.NORMAL)
    {
        _byteOffset = byteOffset;
        _key = key;
        _object = obj;

        COSBase? baseObject = obj is COSObject cosObject ? cosObject.GetObject() : obj;
        _objectStream = baseObject is COSStream cosStream && ObjStmName.Equals(cosStream.GetCOSName(COSName.TYPE));
    }

    public long GetByteOffset()
    {
        return _byteOffset;
    }

    public override COSObjectKey GetReferencedKey()
    {
        return _key;
    }

    public COSBase GetObject()
    {
        return _object;
    }

    public bool IsObjectStream()
    {
        return _objectStream;
    }

    public override long GetSecondColumnValue()
    {
        return GetByteOffset();
    }

    public override long GetThirdColumnValue()
    {
        return GetReferencedKey().GetGeneration();
    }

    public override string ToString()
    {
        return (IsObjectStream() ? "ObjectStreamParent{" : "NormalReference{") +
               $" key={_key}, type={(int)GetXReferenceType()}, byteOffset={_byteOffset} }}";
    }
}
