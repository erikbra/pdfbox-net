/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox Vector behavior.
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
using Xunit;

namespace PdfBox.Net.Tests;

public class VectorTest
{
    [Fact]
    public void TestScaleAndStringRepresentation()
    {
        var vector = new Vector(2, 3);

        Assert.Equal(2, vector.GetX());
        Assert.Equal(3, vector.GetY());

        Vector scaled = vector.Scale(4);
        Assert.Equal(8, scaled.GetX());
        Assert.Equal(12, scaled.GetY());
        Assert.Equal("(8, 12)", scaled.ToString());
    }
}
