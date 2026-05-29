/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/DimensionsType.java
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

namespace PdfBox.Net.XmpBox.Type;

[StructuredType("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim")]
public class DimensionsType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Real)]
    public static readonly string H = "h";

    [PropertyType(XmpTypeName.Real)]
    public static readonly string W = "w";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string UNIT = "unit";

    public DimensionsType(XMPMetadata metadata)
        : base(metadata)
    {
    }

    public float? GetH()
    {
        return GetProperty(H) is RealType prop ? prop.Value : null;
    }

    public float? GetW()
    {
        return GetProperty(W) is RealType prop ? prop.Value : null;
    }

    public string? GetUnit()
    {
        return GetPropertyValueAsString(UNIT);
    }

    public override string ToString()
    {
        return $"DimensionsType{{{GetW()} x {GetH()} {GetUnit()}}}";
    }
}
