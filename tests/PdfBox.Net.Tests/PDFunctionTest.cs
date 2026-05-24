/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox function tests with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/function/TestPDFunctionType4.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;

namespace PdfBox.Net.Tests;

public class PDFunctionTest
{
    [Fact]
    public void Type2EvaluatesKnownInput()
    {
        COSDictionary dictionary = new();
        dictionary.SetInt(COSName.FUNCTION_TYPE, 2);
        dictionary.SetItem(COSName.DOMAIN, COSArray.Of(0f, 1f));
        dictionary.SetItem(COSName.RANGE, COSArray.Of(0f, 1f));
        dictionary.SetItem(COSName.C0, COSArray.Of(0f));
        dictionary.SetItem(COSName.C1, COSArray.Of(1f));
        dictionary.SetFloat(COSName.N, 1f);

        PDFunctionType2 function = new(dictionary);
        Assert.Equal(0.5f, function.Eval([0.5f])[0], 4);

        dictionary.SetFloat(COSName.N, 2f);
        function = new PDFunctionType2(dictionary);
        Assert.Equal(0.25f, function.Eval([0.5f])[0], 4);
    }

    [Fact]
    public void Type4SimpleAndClipping()
    {
        PDFunctionType4 function = CreateFunction("{ add }", [-1f, 1f, -1f, 1f], [-1f, 1f]);
        Assert.Equal(0.9f, function.Eval([0.8f, 0.1f])[0], 4);
        Assert.Equal(1f, function.Eval([0.8f, 0.3f])[0], 4);
        Assert.Equal(1f, function.Eval([0.8f, 1.2f])[0], 4);
    }

    [Fact]
    public void Type4ArgumentOrder()
    {
        PDFunctionType4 function = CreateFunction("{ pop }", [-1f, 1f, -1f, 1f], [-1f, 1f]);
        Assert.Equal(-0.7f, function.Eval([-0.7f, 0f])[0], 4);
    }

    [Fact]
    public void Type4ParserBasics()
    {
        Type4Tester.Create("3 4 add 2 sub").Pop(5).IsEmpty();
        Type4Tester.Create("true { 2 1 add } { 2 1 sub } ifelse").Pop(3).IsEmpty();
        Type4Tester.Create("1 {dup dup .72 mul exch 0 exch .38 mul}\n").Pop(0.38f).Pop(0f).Pop(0.72f).Pop(1f).IsEmpty();
    }

    private static PDFunctionType4 CreateFunction(string functionText, float[] domain, float[] range)
    {
        COSStream stream = new();
        stream.SetInt(COSName.FUNCTION_TYPE, 4);
        COSArray domainArray = new();
        domainArray.SetFloatArray(domain);
        stream.SetItem(COSName.DOMAIN, domainArray);
        COSArray rangeArray = new();
        rangeArray.SetFloatArray(range);
        stream.SetItem(COSName.RANGE, rangeArray);
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] data = Encoding.ASCII.GetBytes(functionText);
            output.Write(data, 0, data.Length);
        }

        return new PDFunctionType4(stream);
    }
}
