/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted focused xUnit coverage for Apache PDFBox COS update-state and increment behavior.
 *
 * No direct equivalent upstream test file exists for this exact slice.
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

namespace PdfBox.Net.Tests;

public class COSIncrementTest
{
    [Fact]
    public void TestUpdateStateStartsTrackingAfterDocumentParsingCompletes()
    {
        COSDocumentState documentState = new();
        COSDictionary dictionary = new();

        dictionary.GetUpdateState().SetOriginDocumentState(documentState);
        Assert.Same(documentState, dictionary.GetUpdateState().GetOriginDocumentState());
        Assert.False(dictionary.IsNeedToBeUpdated());

        documentState.SetParsing(false);
        dictionary.SetInt(COSName.LENGTH, 12);

        Assert.True(dictionary.IsNeedToBeUpdated());
    }

    [Fact]
    public void TestOriginDocumentStatePropagatesToExistingChildren()
    {
        COSDocumentState documentState = new();
        COSDictionary parent = new();
        COSArray childArray = new();
        COSDictionary childDictionary = new();

        parent.SetItem(COSName.GetPDFName("Kids"), childArray);
        parent.SetItem(COSName.GetPDFName("Resources"), childDictionary);

        parent.GetUpdateState().SetOriginDocumentState(documentState);

        Assert.Same(documentState, childArray.GetUpdateState().GetOriginDocumentState());
        Assert.Same(documentState, childDictionary.GetUpdateState().GetOriginDocumentState());
    }

    [Fact]
    public void TestIncrementPromotesUpdatedDirectArrayToParentDictionary()
    {
        COSDocumentState documentState = new();
        COSDictionary parent = new();
        COSArray directArray = new();

        parent.SetItem(COSName.GetPDFName("Kids"), directArray);
        parent.GetUpdateState().SetOriginDocumentState(documentState);
        documentState.SetParsing(false);

        directArray.Add(COSInteger.ONE);

        COSIncrement increment = parent.ToIncrement();

        Assert.Contains(parent, increment.GetObjects());
        Assert.DoesNotContain(directArray, increment.GetObjects());
    }

    [Fact]
    public void TestIncrementCollectsUpdatedIndirectChildObject()
    {
        COSDocumentState documentState = new();
        COSDictionary parent = new();
        COSDictionary indirectChild = new();
        indirectChild.SetKey(new COSObjectKey(10, 0));

        parent.SetItem(COSName.GetPDFName("Metadata"), indirectChild);
        parent.GetUpdateState().SetOriginDocumentState(documentState);
        documentState.SetParsing(false);

        indirectChild.SetString(COSName.TYPE, "Metadata");

        COSIncrement increment = parent.ToIncrement();

        Assert.Contains(indirectChild, increment.GetObjects());
        Assert.DoesNotContain(parent, increment.GetObjects());
    }

    [Fact]
    public void TestContainsTracksProcessedObjects()
    {
        COSDocumentState documentState = new();
        COSDictionary parent = new();
        COSDictionary child = new();
        child.SetKey(new COSObjectKey(12, 0));
        parent.SetItem(COSName.GetPDFName("Metadata"), child);
        parent.GetUpdateState().SetOriginDocumentState(documentState);
        documentState.SetParsing(false);
        child.SetString(COSName.TYPE, "Metadata");

        COSIncrement increment = parent.ToIncrement();
        _ = increment.GetObjects();

        Assert.True(increment.Contains(child));
    }
}
