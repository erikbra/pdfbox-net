/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Shared native adapter added with AI assistance for PDFBOX-6289 signature encryption rules.
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/SecurityHandler.java
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

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

internal static class SignatureDictionary
{
    // /Type is optional (PDFBOX-4466). Match the predicate used by upstream SecurityHandler.
    internal static bool IsSignature(COSDictionary dictionary)
    {
        COSName? type = dictionary.GetCOSName(COSName.TYPE);
        return type?.GetName() is "Sig" or "DocTimeStamp" ||
            (dictionary.GetDictionaryObject(COSName.CONTENTS) is COSString &&
             dictionary.GetDictionaryObject(COSName.GetPDFName("ByteRange")) is COSArray);
    }
}
