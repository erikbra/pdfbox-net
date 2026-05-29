/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/DateType.java
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

using PdfBox.Net.XmpBox;


namespace PdfBox.Net.XmpBox.Type;

public class DateType : AbstractSimpleProperty
{
    private DateTimeOffset dateValue;

    public DateType(XMPMetadata metadata, string? namespaceURI, string? prefix, string propertyName, object value)
        : base(metadata, namespaceURI, prefix, propertyName, value)
    {
    }

    public DateTimeOffset Value => dateValue;

    public override object GetValue()
    {
        return dateValue;
    }

    public override void SetValue(object value)
    {
        switch (value)
        {
            case DateTimeOffset date:
                dateValue = date;
                return;
            case string text:
                dateValue = DateConverter.ToCalendar(text);
                return;
            default:
                throw new ArgumentException(value is null
                    ? "Value null is not allowed for the Date type"
                    : $"Value given is not allowed for the Date type: {value.GetType()}, value: {value}");
        }
    }

    public override string GetStringValue()
    {
        return DateConverter.ToISO8601(dateValue);
    }
}
