/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native/adapted regression tests added with AI assistance for upstream synchronization #957.
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: bd21bd44f5dd050fa716c9bb033adcf725b7ebd3
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Adapted regression coverage for Apache PDFBox changes through 5206dee5bfe0f435609f6506e3fccccbc46a95cc.
 * Upstream test mappings and adaptations: reports/issue-957-core-slice.json.
 */
using PdfBox.Net.COS;
using PdfBox.Net.Examples.Signature;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Tests.Signature;

public class Issue957LatestSignatureTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public void InvalidCertificationPermissionsFailBeforeMutatingDocument(int permissions)
    {
        using PDDocument document = new();
        PDSignature signature = new();
        var exception = Assert.Throws<ArgumentException>(() => SigUtils.SetMDPPermission(document, signature, permissions));
        Assert.Equal("The permissions value must be 1, 2 or 3", exception.Message);
        Assert.Null(((COSDictionary)document.GetDocumentCatalog().GetCOSObject()).GetItem(COSName.GetPDFName("Perms")));
        Assert.Null(((COSDictionary)signature.GetCOSObject()).GetItem(COSName.GetPDFName("Reference")));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(0, 2)]
    [InlineData(8, 2)]
    public void CertificationPermissionIsReadFromCatalogAndInvalidValueDefaultsToTwo(int encoded, int expected)
    {
        using PDDocument document = new();
        PDSignature certification = new();
        SigUtils.SetMDPPermission(document, certification, 1);
        var reference = ((COSDictionary)certification.GetCOSObject()).GetCOSArray(COSName.GetPDFName("Reference"))!;
        var parameters = ((COSDictionary)reference.GetObject(0)!).GetCOSDictionary(COSName.GetPDFName("TransformParams"))!;
        parameters.SetInt(COSName.P, encoded);
        Assert.Equal(expected, SigUtils.GetMDPPermission(document));
    }

    [Fact]
    public void ExistingApprovalSignaturePreventsNewCertification()
    {
        using PDDocument document = new();
        AddSignature(document, "Sig", 10, contents: true);
        Assert.Throws<IOException>(() => SigUtils.SetMDPPermission(document, new PDSignature(), 1));
    }

    [Fact]
    public void ExistingTimestampDoesNotPreventNewCertification()
    {
        using PDDocument document = new();
        AddSignature(document, "DocTimeStamp", 10, contents: true);
        SigUtils.SetMDPPermission(document, new PDSignature(), 3);
        Assert.Equal(3, SigUtils.GetMDPPermission(document));
    }

    [Fact]
    public void UnknownTypeOnLatestSignatureIsIgnored()
    {
        using PDDocument document = new();
        AddSignature(document, "Sig", 10);
        AddSignature(document, "Unknown", 20);
        Assert.Null(SigUtils.GetLastRelevantSignature(document));
    }

    private static void AddSignature(PDDocument document, string type, int offset, bool contents = false)
    {
        PDAcroForm form = document.GetDocumentCatalog().GetAcroForm() ?? new(document);
        PDSignature signature = new();
        signature.SetByteRange([0, offset, offset + 100, 10]);
        if (contents) signature.SetContents([1, 2, 3]);
        ((COSDictionary)signature.GetCOSObject()).SetName(COSName.TYPE, type);
        PDSignatureField field = new(form);
        field.SetValue(signature);
        var fields = form.GetFields().ToList();
        fields.Add(field);
        form.SetFields(fields);
        document.GetDocumentCatalog().SetAcroForm(form);
    }
}
