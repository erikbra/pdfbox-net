/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/BooleanType.java
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


public class BooleanType : AbstractSimpleProperty
{
    public const string TrueValue = "True";
    public const string FalseValue = "False";

    private bool booleanValue;

    public BooleanType(XMPMetadata metadata, string? namespaceURI, string? prefix, string propertyName, object value)
        : base(metadata, namespaceURI, prefix, propertyName, value)
    {
    }

    public bool Value => booleanValue;

    public override object GetValue()
    {
        return booleanValue;
    }

    public override void SetValue(object value)
    {
        switch (value)
        {
            case bool boolean:
                booleanValue = boolean;
                return;
            case string text:
                string s = text.Trim().ToUpperInvariant();
                if (s == "TRUE")
                {
                    booleanValue = true;
                    return;
                }

                if (s == "FALSE")
                {
                    booleanValue = false;
                    return;
                }

                throw new ArgumentException($"Not a valid boolean value : '{value}'");
            default:
                throw new ArgumentException("Value given is not allowed for the Boolean type.");
        }
    }

    public override string GetStringValue()
    {
        return booleanValue ? TrueValue : FalseValue;
    }
}
