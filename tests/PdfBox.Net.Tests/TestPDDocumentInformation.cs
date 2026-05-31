/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/TestPDDocumentInformation.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted-minimal
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

using PdfBox.Net.PDModel;
using PdfBox.Net.COS;

namespace PdfBox.Net.Tests;

public class TestPDDocumentInformation
{
    [Fact]
    public void StringFieldsRoundtrip()
    {
        PDDocumentInformation info = new();

        info.SetTitle("Test Title");
        info.SetAuthor("Test Author");
        info.SetSubject("Test Subject");
        info.SetKeywords("keyword1 keyword2");
        info.SetCreator("TestApp");
        info.SetProducer("TestLib");

        Assert.Equal("Test Title", info.GetTitle());
        Assert.Equal("Test Author", info.GetAuthor());
        Assert.Equal("Test Subject", info.GetSubject());
        Assert.Equal("keyword1 keyword2", info.GetKeywords());
        Assert.Equal("TestApp", info.GetCreator());
        Assert.Equal("TestLib", info.GetProducer());
    }

    [Fact]
    public void ClearingStringFieldsWithNull()
    {
        PDDocumentInformation info = new();
        info.SetTitle("Title");
        info.SetTitle(null);
        Assert.Null(info.GetTitle());
    }

    [Fact]
    public void CreationDateRoundtrip()
    {
        PDDocumentInformation info = new();
        DateTimeOffset expected = new DateTimeOffset(2026, 5, 22, 21, 58, 38, TimeSpan.Zero);
        info.SetCreationDate(expected);
        DateTimeOffset? actual = info.GetCreationDate();
        Assert.NotNull(actual);
        Assert.Equal(expected, actual!.Value);
    }

    [Fact]
    public void ModificationDateRoundtrip()
    {
        PDDocumentInformation info = new();
        DateTimeOffset expected = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.FromHours(2));
        info.SetModificationDate(expected);
        DateTimeOffset? actual = info.GetModificationDate();
        Assert.NotNull(actual);
        Assert.Equal(expected, actual!.Value);
    }

    [Fact]
    public void NullDateReturnsNull()
    {
        PDDocumentInformation info = new();
        Assert.Null(info.GetCreationDate());
        Assert.Null(info.GetModificationDate());
    }

    [Fact]
    public void SetDateToNullClearsField()
    {
        PDDocumentInformation info = new();
        info.SetCreationDate(DateTimeOffset.UtcNow);
        Assert.NotNull(info.GetCreationDate());
        info.SetCreationDate(null);
        Assert.Null(info.GetCreationDate());
    }

    [Fact]
    public void TrappedValidValues()
    {
        PDDocumentInformation info = new();
        Assert.Null(info.GetTrapped());

        info.SetTrapped("True");
        Assert.Equal("True", info.GetTrapped());

        info.SetTrapped("False");
        Assert.Equal("False", info.GetTrapped());

        info.SetTrapped("Unknown");
        Assert.Equal("Unknown", info.GetTrapped());

        info.SetTrapped(null);
        Assert.Null(info.GetTrapped());
    }

    [Fact]
    public void TrappedInvalidValueThrows()
    {
        PDDocumentInformation info = new();
        Assert.Throws<ArgumentException>(() => info.SetTrapped("Invalid"));
    }

    [Fact]
    public void CustomMetadataRoundtrip()
    {
        PDDocumentInformation info = new();
        info.SetCustomMetadataValue("CustomField", "CustomValue");
        Assert.Equal("CustomValue", info.GetCustomMetadataValue("CustomField"));
    }

    [Fact]
    public void MetadataKeysReturnsAllKeys()
    {
        PDDocumentInformation info = new();
        info.SetTitle("T");
        info.SetAuthor("A");
        ISet<string> keys = info.GetMetadataKeys();
        Assert.Contains("Title", keys);
        Assert.Contains("Author", keys);
    }

    [Fact]
    public void PropertyStringValueReturnsStoredString()
    {
        PDDocumentInformation info = new();
        info.SetCustomMetadataValue("Company", "Basis Technology Corp.");

        Assert.Equal("Basis Technology Corp.", info.GetPropertyStringValue("Company"));
    }

    [Fact]
    public void IndirectTitleEntryIsResolved()
    {
        COSDictionary dictionary = new();
        COSString title = new("Title");
        title.SetKey(new COSObjectKey(12, 0));
        dictionary.SetItem(COSName.TITLE, new COSObject(title, title.GetKey()!));

        PDDocumentInformation info = new(dictionary);

        Assert.Equal("Title", info.GetTitle());
    }
}
