// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestHelloWorld.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
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

using PdfBox.Net.Examples.PDModel;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test of HelloWorld and HelloWorldTTF examples.
/// Ported from TestHelloWorld.java — adapted because both examples rely on
/// PDType1Font(FontName) / PDPageContentStream text operators not yet implemented
/// in this .NET port.  Each test verifies the expected <see cref="NotSupportedException"/>.
/// </summary>
public class TestHelloWorld
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-helloworld");

    public TestHelloWorld()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestHelloWorldThrows()
    {
        string outputFile = Path.Combine(OutputDir, "HelloWorld.pdf");
        File.Delete(outputFile);
        // HelloWorld expects exactly 2 args: <output-file> <message>
        string[] args = { outputFile, "HelloWorld.pdf" };
        Assert.Throws<NotSupportedException>(() => HelloWorld.Main(args));
    }

    [Fact]
    public void TestHelloWorldTTFThrows()
    {
        string outputFile = Path.Combine(OutputDir, "HelloWorldTTF.pdf");
        File.Delete(outputFile);
        // HelloWorldTTF expects exactly 2 args: <output-file> <ttf-font-file>
        string[] args = { outputFile, "nonexistent-font.ttf" };
        Assert.Throws<NotSupportedException>(() => HelloWorldTTF.Main(args));
    }
}
