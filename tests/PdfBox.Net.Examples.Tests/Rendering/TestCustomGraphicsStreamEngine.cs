// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/rendering/TestCustomGraphicsStreamEngine.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

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

using PdfBox.Net.Examples.Rendering;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Examples.Tests.Rendering;

/// <summary>
/// Test of CustomGraphicsStreamEngine example.
/// </summary>
public class TestCustomGraphicsStreamEngine
{
    [Fact]
    public void TestCustomGraphicsStreamEngineShowsUsageWithoutArgs()
    {
        Exception? ex = Record.Exception(() => CustomGraphicsStreamEngine.Main([]));
        Assert.Null(ex);
    }

    [Fact]
    public void TestCustomGraphicsStreamEngineRunsOnEmptyPage()
    {
        using PDDocument doc = new PDDocument();
        PDPage page = new PDPage();
        doc.AddPage(page);
        var engine = new CustomGraphicsStreamEngine(page);
        Exception? ex = Record.Exception(() => engine.Run());
        Assert.Null(ex);
    }
}
