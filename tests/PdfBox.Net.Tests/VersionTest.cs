/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for the C# port of Apache PDFBox Version behavior.
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

using PdfBox.Net.Util;
using PdfBoxVersion = PdfBox.Net.Util.Version;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for <see cref="PdfBoxVersion"/>.
/// </summary>
public class VersionTest
{
    [Fact]
    public void GetVersionReturnsNonNullOrNullWithoutThrowing()
    {
        // The method must not throw regardless of environment.
        string? version = PdfBoxVersion.GetVersion();
        // Version may be null in some build configurations; we only verify it does not throw.
        _ = version;
    }

    [Fact]
    public void GetVersionReturnedValueIsNullOrNonEmpty()
    {
        string? version = PdfBoxVersion.GetVersion();
        if (version != null)
        {
            Assert.NotEmpty(version);
        }
    }
}
