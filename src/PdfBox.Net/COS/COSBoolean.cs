/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSBoolean.java
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

using System.IO;

namespace PdfBox.Net.COS;

/// <summary>
/// This class represents a boolean value in the PDF document.
/// </summary>
public sealed class COSBoolean : COSBase
{
    private static readonly byte[] TRUE_BYTES = [116, 114, 117, 101];
    private static readonly byte[] FALSE_BYTES = [102, 97, 108, 115, 101];

    public static readonly COSBoolean TRUE = new(true);
    public static readonly COSBoolean FALSE = new(false);

    private readonly bool _value;

    private COSBoolean(bool value)
    {
        _value = value;
    }

    public bool GetValue()
    {
        return _value;
    }

    public bool GetValueAsObject()
    {
        return _value;
    }

    public static COSBoolean GetBoolean(bool value)
    {
        return value ? TRUE : FALSE;
    }

    public static COSBoolean GetBoolean(bool? value)
    {
        return GetBoolean(value!.Value);
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromBoolean(this);
    }

    public override string ToString()
    {
        return _value.ToString().ToLowerInvariant();
    }

    public override int GetHashCode()
    {
        return _value ? 1231 : 1237;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public void WritePDF(Stream output)
    {
        output.Write(_value ? TRUE_BYTES : FALSE_BYTES);
    }
}
