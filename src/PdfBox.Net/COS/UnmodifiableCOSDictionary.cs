/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/UnmodifiableCOSDictionary.java
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

/// <summary>
/// An unmodifiable COSDictionary.
/// </summary>
public sealed class UnmodifiableCOSDictionary : COSDictionary
{
    public UnmodifiableCOSDictionary(COSDictionary dict)
        : base(dict)
    {
    }

    private static InvalidOperationException ReadOnlyException()
    {
        return new("Dictionary is unmodifiable.");
    }

    public new void Clear() => throw ReadOnlyException();
    public new void RemoveItem(COSName key) => throw ReadOnlyException();
    public new void RemoveItem(string key) => throw ReadOnlyException();
    public new void AddAll(COSDictionary dict) => throw ReadOnlyException();
    public new void SetItem(COSName key, COSBase? value) => throw ReadOnlyException();
    public new void SetItem(COSName key, COSObjectable? value) => throw ReadOnlyException();
    public new void SetItem(string key, COSObjectable? value) => throw ReadOnlyException();
    public new void SetItem(string key, COSBase? value) => throw ReadOnlyException();
    public new void SetBoolean(string key, bool value) => throw ReadOnlyException();
    public new void SetBoolean(COSName key, bool value) => throw ReadOnlyException();
    public new void SetName(string key, string? value) => throw ReadOnlyException();
    public new void SetName(COSName key, string? value) => throw ReadOnlyException();
    public new void SetString(string key, string? value) => throw ReadOnlyException();
    public new void SetString(COSName key, string? value) => throw ReadOnlyException();
    public new void SetInt(string key, int value) => throw ReadOnlyException();
    public new void SetInt(COSName key, int value) => throw ReadOnlyException();
    public new void SetLong(string key, long value) => throw ReadOnlyException();
    public new void SetLong(COSName key, long value) => throw ReadOnlyException();
    public new void SetFloat(string key, float value) => throw ReadOnlyException();
    public new void SetFloat(COSName key, float value) => throw ReadOnlyException();
}
