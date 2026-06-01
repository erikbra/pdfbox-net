/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache FontBox BoundingBox behavior.
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

using PdfBox.Net.FontBox.Util;
using Xunit;

namespace PdfBox.Net.FontBox.Tests;

public class BoundingBoxTest
{
    [Fact]
    public void TestConstructorsAndGeometry()
    {
        var fromFloats = new BoundingBox(1, 2, 6, 10);
        Assert.Equal(1, fromFloats.GetLowerLeftX());
        Assert.Equal(2, fromFloats.GetLowerLeftY());
        Assert.Equal(6, fromFloats.GetUpperRightX());
        Assert.Equal(10, fromFloats.GetUpperRightY());
        Assert.Equal(5, fromFloats.GetWidth());
        Assert.Equal(8, fromFloats.GetHeight());
        Assert.True(fromFloats.Contains(1, 2));
        Assert.True(fromFloats.Contains(6, 10));
        Assert.False(fromFloats.Contains(0.9f, 2));
        Assert.Equal("[1,2,6,10]", fromFloats.ToString());

        var fromList = new BoundingBox(new[] { 3f, 4f, 7f, 9f });
        Assert.Equal(3, fromList.GetLowerLeftX());
        Assert.Equal(4, fromList.GetLowerLeftY());
        Assert.Equal(7, fromList.GetUpperRightX());
        Assert.Equal(9, fromList.GetUpperRightY());
    }

    [Fact]
    public void TestDefaultBoundingBoxIsAllZero()
    {
        var bbox = new BoundingBox();
        Assert.Equal(0f, bbox.GetLowerLeftX());
        Assert.Equal(0f, bbox.GetLowerLeftY());
        Assert.Equal(0f, bbox.GetUpperRightX());
        Assert.Equal(0f, bbox.GetUpperRightY());
    }
}
