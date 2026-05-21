/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Adapted low-level writer bridge for chunk-2 token/object flow.
 * Minimal COS object serialization support aligned with existing COS types.
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

using System.Text;
using PdfBox.Net.COS;

namespace PdfBox.Net.PdfWriter;

public sealed class COSWriter
{
    private readonly COSStandardOutputStream _output;

    public COSWriter(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = new COSStandardOutputStream(output);
    }

    public void Write(COSBase? value)
    {
        COSDictionary.WriteValuePDF(value, _output);
    }

    public static byte[] Serialize(COSBase? value)
    {
        using MemoryStream output = new();
        COSWriter writer = new(output);
        writer.Write(value);
        writer._output.Flush();
        return output.ToArray();
    }

    public static string SerializeToString(COSBase? value)
    {
        return Encoding.Latin1.GetString(Serialize(value));
    }
}
