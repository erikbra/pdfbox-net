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
/// Ported from TestHelloWorld.java.
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
    public void TestHelloWorldCreatesFile()
    {
        string outputFile = Path.Combine(OutputDir, "HelloWorld.pdf");
        File.Delete(outputFile);
        // HelloWorld expects exactly 2 args: <output-file> <message>
        string[] args = { outputFile, "Hello World!" };
        HelloWorld.Main(args);
        Assert.True(File.Exists(outputFile), "HelloWorld should have created the PDF");
    }

    [Fact]
    public void TestHelloWorldTTFCreatesFile()
    {
        const string ttfFont = "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf";
        if (!File.Exists(ttfFont))
            Assert.Skip("LiberationSans-Regular.ttf not available on this system");

        string outputFile = Path.Combine(OutputDir, "HelloWorldTTF.pdf");
        File.Delete(outputFile);
        // HelloWorldTTF expects 3 args: <output-file> <message> <ttf-file>
        string[] args = { outputFile, "Hello World TTF!", ttfFont };
        HelloWorldTTF.Main(args);
        Assert.True(File.Exists(outputFile), "HelloWorldTTF should have created the PDF");
    }
}
