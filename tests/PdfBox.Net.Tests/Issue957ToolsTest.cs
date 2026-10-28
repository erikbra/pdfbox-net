/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native regression tests added with AI assistance for upstream synchronization #957.
 * PORT_MODE: native-adapter
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using PdfBox.Net.PDModel;
using PdfBox.Net.Tools;

namespace PdfBox.Net.Tests;

public class Issue957ToolsTest
{
    [Theory]
    [InlineData("export:fdf")]
    [InlineData("export:xfdf")]
    [InlineData("export:images")]
    [InlineData("export:xmp")]
    [InlineData("fromimage")]
    [InlineData("import:fdf")]
    [InlineData("import:xfdf")]
    [InlineData("fromtext")]
    public void NewUpstreamCommandNamesAreRecognized(string command)
    {
        using StringWriter error = new();
        Assert.NotEqual(0, PDFBox.Run([command], error: error));
        Assert.DoesNotContain("Unknown command:", error.ToString());
    }

    [Fact]
    public void DecompressCommandCanRewriteItsInputWhenOutputIsOmitted()
    {
        string path = Path.Combine(Path.GetTempPath(), $"pdfbox-957-{Guid.NewGuid():N}.pdf");
        try
        {
            using (PDDocument source = new())
            {
                source.AddPage(new());
                source.Save(path);
            }
            using StringWriter error = new();
            Assert.Equal(0, PDFBox.Run(["decompress", "-i", path], error: error));
            using PDDocument result = PDDocument.Load(path);
            Assert.Equal(1, result.GetNumberOfPages());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
