/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Adapted from Apache PDFBox PDDocument tests for minimal chunk-3 open/inspect/save
 * document pipeline coverage.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/TestPDDocument.java
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

using System.Text;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class PDDocumentPipelineTest
{
    [Fact]
    public void OpenInspectSavePipelineFromFixture()
    {
        byte[] fixtureBytes = File.ReadAllBytes(GetFixturePath());
        using MemoryStream input = new(fixtureBytes);
        using PDDocument document = PDDocument.Load(input);

        Assert.Equal("Catalog", document.GetDocumentCatalog().GetTypeName());
        Assert.Equal(0, document.GetDocumentCatalog().GetPageCount());
        Assert.Equal("Chunk3 Fixture", document.GetDocumentInformation().GetTitle());
        Assert.Equal("pdfbox-net", document.GetDocumentInformation().GetAuthor());

        using MemoryStream output = new();
        document.Save(output);
        string serialized = Encoding.Latin1.GetString(output.ToArray());
        Assert.Contains("/Root", serialized);
        Assert.Contains("/Info", serialized);
    }

    [Fact]
    public void SaveLoadStreamPreservesMetadata()
    {
        byte[] serialized;
        using (PDDocument document = new())
        {
            document.AddPage(new PDPage());
            document.AddPage(new PDPage());
            PDDocumentInformation info = document.GetDocumentInformation();
            info.SetTitle("Smoke Pipeline");
            info.SetAuthor("Chunk3");
            info.SetCustomMetadataValue("Pipeline", "open-inspect-save");

            using MemoryStream output = new();
            document.Save(output);
            serialized = output.ToArray();
        }

        using PDDocument loaded = PDDocument.Load(new MemoryStream(serialized));
        Assert.Equal("Smoke Pipeline", loaded.GetDocumentInformation().GetTitle());
        Assert.Equal("Chunk3", loaded.GetDocumentInformation().GetAuthor());
        Assert.Equal("open-inspect-save", loaded.GetDocumentInformation().GetCustomMetadataValue("Pipeline"));
        Assert.Equal(2, loaded.GetNumberOfPages());
    }

    [Fact]
    public void SaveLoadFilePreservesCatalogAndInfo()
    {
        string path = Path.Combine(Path.GetTempPath(), $"pdmodel-pipeline-{Guid.NewGuid():N}.pdf");
        try
        {
            using (PDDocument document = new())
            {
                document.AddPage(new PDPage());
                document.GetDocumentInformation().SetTitle("file-save");
                document.GetDocumentCatalog().SetVersion("1.4");
                document.Save(path);
            }

            using PDDocument loaded = PDDocument.Load(path);
            Assert.Equal("file-save", loaded.GetDocumentInformation().GetTitle());
            Assert.Equal("1.4", loaded.GetDocumentCatalog().GetVersion());
            Assert.Equal("Catalog", loaded.GetDocumentCatalog().GetTypeName());
            Assert.Equal(1, loaded.GetNumberOfPages());
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void VersionsFollowHeaderAndCatalogRules()
    {
        using PDDocument document = new();
        Assert.Equal(1.4f, document.GetVersion());
        Assert.Equal("1.4", document.GetDocumentCatalog().GetVersion());

        document.SetVersion(1.3f);
        Assert.Equal(1.4f, document.GetVersion());
        Assert.Equal("1.4", document.GetDocumentCatalog().GetVersion());

        document.SetVersion(1.5f);
        Assert.Equal(1.5f, document.GetVersion());
        Assert.Equal("1.5", document.GetDocumentCatalog().GetVersion());
    }

    private static string GetFixturePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "minimal-document-fixture.pdf");
    }
}
