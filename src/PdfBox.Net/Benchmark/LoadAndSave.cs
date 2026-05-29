/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: benchmark/src/main/java/org/apache/pdfbox/benchmark/LoadAndSave.java
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

using PdfBox.Net.PDModel;

namespace PdfBox.Net.Benchmark;

public static class LoadAndSave
{
    public const string MediumSizeTestFile = "target/pdfs/849-42-94772-1-10-20210818.pdf";
    public const string LargeSizeTestFile = "target/pdfs/506-42-86246-2-10-20190822.pdf";

    public static void LoadMediumFile() => LoadFile(MediumSizeTestFile);

    public static void SaveMediumFile() => SaveFile(MediumSizeTestFile);

    public static void SaveIncrementalMediumFile() => SaveFile(MediumSizeTestFile);

    public static void SaveNoCompressionMediumFile() => SaveFile(MediumSizeTestFile);

    public static void LoadLargeFile() => LoadFile(LargeSizeTestFile);

    public static void SaveLargeFile() => SaveFile(LargeSizeTestFile);

    public static void SaveIncrementalLargeFile() => SaveFile(LargeSizeTestFile);

    public static void SaveNoCompressionLargeFile() => SaveFile(LargeSizeTestFile);

    public static void LoadFile(string filePath)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
    }

    public static void SaveFile(string filePath)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
        document.Save(Stream.Null);
    }
}
