/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cmap/CMapStringsTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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
using PdfBox.Net.FontBox.CMap;

namespace PdfBox.Net.FontBox.Tests;

public class CMapStringsTest
{
    private static readonly System.Text.Encoding Latin1 = System.Text.Encoding.GetEncoding("ISO-8859-1");
    private static readonly System.Text.Encoding Utf16Be = System.Text.Encoding.BigEndianUnicode;

    [Fact]
    public void GetNonCachedMappings()
    {
        // arrays consisting of more than 2 bytes aren't cached.
        Assert.Null(CMapStrings.GetMapping([0, 0, 0]));
        Assert.Null(CMapStrings.GetMapping([0, 0, 0, 0]));
    }

    [Fact]
    public void GetMappingOneByte()
    {
        byte[] minValueOneByte = [0];
        string minValueMapping = Latin1.GetString(minValueOneByte);
        // the given values are equal
        Assert.Equal(CMapStrings.GetMapping(minValueOneByte), CMapStrings.GetMapping(minValueOneByte));
        // the given values are the same objects (string is cached in a static array)
        Assert.Same(CMapStrings.GetMapping(minValueOneByte), CMapStrings.GetMapping(minValueOneByte));
        // check the mapped string value
        Assert.Equal(minValueMapping, CMapStrings.GetMapping(minValueOneByte));

        byte[] maxValueOneByte = [0xFF];
        string maxValueMapping = Latin1.GetString(maxValueOneByte);
        Assert.Equal(CMapStrings.GetMapping(maxValueOneByte), CMapStrings.GetMapping(maxValueOneByte));
        Assert.Same(CMapStrings.GetMapping(maxValueOneByte), CMapStrings.GetMapping(maxValueOneByte));
        Assert.Equal(maxValueMapping, CMapStrings.GetMapping(maxValueOneByte));

        byte[] anyValueOneByte = [98];
        string anyValueMapping = Latin1.GetString(anyValueOneByte);
        Assert.Equal(CMapStrings.GetMapping(anyValueOneByte), CMapStrings.GetMapping(anyValueOneByte));
        Assert.Same(CMapStrings.GetMapping(anyValueOneByte), CMapStrings.GetMapping(anyValueOneByte));
        Assert.Equal(anyValueMapping, CMapStrings.GetMapping(anyValueOneByte));
    }

    [Fact]
    public void GetMappingTwoByte()
    {
        byte[] minValueTwoByte = [0, 0];
        string minValueMapping = Utf16Be.GetString(minValueTwoByte);
        // the given values are equal
        Assert.Equal(CMapStrings.GetMapping(minValueTwoByte), CMapStrings.GetMapping(minValueTwoByte));
        // the given values are the same objects (string is cached in a static array)
        Assert.Same(CMapStrings.GetMapping(minValueTwoByte), CMapStrings.GetMapping(minValueTwoByte));
        // check the mapped string value
        Assert.Equal(minValueMapping, CMapStrings.GetMapping(minValueTwoByte));

        byte[] maxValueTwoByte = [0xFF, 0xFF];
        string maxValueMapping = Utf16Be.GetString(maxValueTwoByte);
        Assert.Equal(CMapStrings.GetMapping(maxValueTwoByte), CMapStrings.GetMapping(maxValueTwoByte));
        Assert.Same(CMapStrings.GetMapping(maxValueTwoByte), CMapStrings.GetMapping(maxValueTwoByte));
        Assert.Equal(maxValueMapping, CMapStrings.GetMapping(maxValueTwoByte));

        byte[] anyValueTwoByte1 = [0x62, 0x43];
        string anyValueMapping1 = Utf16Be.GetString(anyValueTwoByte1);
        Assert.Equal(CMapStrings.GetMapping(anyValueTwoByte1), CMapStrings.GetMapping(anyValueTwoByte1));
        Assert.Same(CMapStrings.GetMapping(anyValueTwoByte1), CMapStrings.GetMapping(anyValueTwoByte1));
        Assert.Equal(anyValueMapping1, CMapStrings.GetMapping(anyValueTwoByte1));

        byte[] anyValueTwoByte2 = [0xFF, 0x43];
        string anyValueMapping2 = Utf16Be.GetString(anyValueTwoByte2);
        Assert.Equal(CMapStrings.GetMapping(anyValueTwoByte2), CMapStrings.GetMapping(anyValueTwoByte2));
        Assert.Same(CMapStrings.GetMapping(anyValueTwoByte2), CMapStrings.GetMapping(anyValueTwoByte2));
        Assert.Equal(anyValueMapping2, CMapStrings.GetMapping(anyValueTwoByte2));

        byte[] anyValueTwoByte3 = [0x38, 0xFF];
        string anyValueMapping3 = Utf16Be.GetString(anyValueTwoByte3);
        Assert.Equal(CMapStrings.GetMapping(anyValueTwoByte3), CMapStrings.GetMapping(anyValueTwoByte3));
        Assert.Same(CMapStrings.GetMapping(anyValueTwoByte3), CMapStrings.GetMapping(anyValueTwoByte3));
        Assert.Equal(anyValueMapping3, CMapStrings.GetMapping(anyValueTwoByte3));
    }

    [Fact]
    public void GetByteValuesOneByte()
    {
        byte[] minValueOneByte = [0];
        // the given values are equal
        Assert.Equal(CMapStrings.GetByteValue(minValueOneByte), CMapStrings.GetByteValue(minValueOneByte));
        // the given values are the same objects (byte array cached in a static array)
        Assert.Same(CMapStrings.GetByteValue(minValueOneByte), CMapStrings.GetByteValue(minValueOneByte));
        // the cached value isn't the same object as the given one
        Assert.NotSame(minValueOneByte, CMapStrings.GetByteValue(minValueOneByte));

        byte[] maxValueOneByte = [0xFF];
        Assert.Equal(CMapStrings.GetByteValue(maxValueOneByte), CMapStrings.GetByteValue(maxValueOneByte));
        Assert.Same(CMapStrings.GetByteValue(maxValueOneByte), CMapStrings.GetByteValue(maxValueOneByte));
        Assert.NotSame(maxValueOneByte, CMapStrings.GetByteValue(maxValueOneByte));

        byte[] anyValueOneByte = [98];
        Assert.Equal(CMapStrings.GetByteValue(anyValueOneByte), CMapStrings.GetByteValue(anyValueOneByte));
        Assert.Same(CMapStrings.GetByteValue(anyValueOneByte), CMapStrings.GetByteValue(anyValueOneByte));
        Assert.NotSame(anyValueOneByte, CMapStrings.GetByteValue(anyValueOneByte));
    }

    [Fact]
    public void GetByteValuesTwoByte()
    {
        byte[] minValueTwoByte = [0, 0];
        // the given values are equal
        Assert.Equal(CMapStrings.GetByteValue(minValueTwoByte), CMapStrings.GetByteValue(minValueTwoByte));
        // the given values are the same objects
        Assert.Same(CMapStrings.GetByteValue(minValueTwoByte), CMapStrings.GetByteValue(minValueTwoByte));
        // the cached value isn't the same object as the given one
        Assert.NotSame(minValueTwoByte, CMapStrings.GetByteValue(minValueTwoByte));

        byte[] maxValueTwoByte = [0xFF, 0xFF];
        Assert.Equal(CMapStrings.GetByteValue(maxValueTwoByte), CMapStrings.GetByteValue(maxValueTwoByte));
        Assert.Same(CMapStrings.GetByteValue(maxValueTwoByte), CMapStrings.GetByteValue(maxValueTwoByte));
        Assert.NotSame(maxValueTwoByte, CMapStrings.GetByteValue(maxValueTwoByte));

        byte[] anyValueTwoByte1 = [0x62, 0x43];
        Assert.Equal(CMapStrings.GetByteValue(anyValueTwoByte1), CMapStrings.GetByteValue(anyValueTwoByte1));
        Assert.Same(CMapStrings.GetByteValue(anyValueTwoByte1), CMapStrings.GetByteValue(anyValueTwoByte1));
        Assert.NotSame(anyValueTwoByte1, CMapStrings.GetByteValue(anyValueTwoByte1));

        byte[] anyValueTwoByte2 = [0xFF, 0x43];
        Assert.Equal(CMapStrings.GetByteValue(anyValueTwoByte2), CMapStrings.GetByteValue(anyValueTwoByte2));
        Assert.Same(CMapStrings.GetByteValue(anyValueTwoByte2), CMapStrings.GetByteValue(anyValueTwoByte2));
        Assert.NotSame(anyValueTwoByte2, CMapStrings.GetByteValue(anyValueTwoByte2));

        byte[] anyValueTwoByte3 = [0x38, 0xFF];
        Assert.Equal(CMapStrings.GetByteValue(anyValueTwoByte3), CMapStrings.GetByteValue(anyValueTwoByte3));
        Assert.Same(CMapStrings.GetByteValue(anyValueTwoByte3), CMapStrings.GetByteValue(anyValueTwoByte3));
        Assert.NotSame(anyValueTwoByte3, CMapStrings.GetByteValue(anyValueTwoByte3));
    }

    [Fact]
    public void GetNonCachedByteValues()
    {
        // arrays consisting of more than 2 bytes aren't cached.
        Assert.Null(CMapStrings.GetByteValue([0, 0, 0]));
        Assert.Null(CMapStrings.GetByteValue([0, 0, 0, 0]));
    }

    [Fact]
    public void GetIndexValuesOneByte()
    {
        byte[] minValueOneByte = [0];
        // the given values are equal
        Assert.Equal(CMapStrings.GetIndexValue(minValueOneByte), CMapStrings.GetIndexValue(minValueOneByte));
        // check the int value
        Assert.Equal(0, CMapStrings.GetIndexValue(minValueOneByte));

        byte[] maxValueOneByte = [0xFF];
        Assert.Equal(CMapStrings.GetIndexValue(maxValueOneByte), CMapStrings.GetIndexValue(maxValueOneByte));
        Assert.Equal(0xFF, CMapStrings.GetIndexValue(maxValueOneByte));

        byte[] anyValueOneByte = [98];
        Assert.Equal(CMapStrings.GetIndexValue(anyValueOneByte), CMapStrings.GetIndexValue(anyValueOneByte));
        Assert.Equal(98, CMapStrings.GetIndexValue(anyValueOneByte));
    }

    [Fact]
    public void GetIndexValuesTwoByte()
    {
        byte[] minValueTwoByte = [0, 0];
        // the given values are equal
        Assert.Equal(CMapStrings.GetIndexValue(minValueTwoByte), CMapStrings.GetIndexValue(minValueTwoByte));
        // check the int value
        Assert.Equal(0, CMapStrings.GetIndexValue(minValueTwoByte));

        byte[] maxValueTwoByte = [0xFF, 0xFF];
        Assert.Equal(CMapStrings.GetIndexValue(maxValueTwoByte), CMapStrings.GetIndexValue(maxValueTwoByte));
        Assert.Equal(0xFFFF, CMapStrings.GetIndexValue(maxValueTwoByte));

        byte[] anyValueTwoByte1 = [0x62, 0x43];
        Assert.Equal(CMapStrings.GetIndexValue(anyValueTwoByte1), CMapStrings.GetIndexValue(anyValueTwoByte1));
        Assert.Equal(0x6243, CMapStrings.GetIndexValue(anyValueTwoByte1));

        byte[] anyValueTwoByte2 = [0xFF, 0x43];
        Assert.Equal(CMapStrings.GetIndexValue(anyValueTwoByte2), CMapStrings.GetIndexValue(anyValueTwoByte2));
        Assert.Equal(0xFF43, CMapStrings.GetIndexValue(anyValueTwoByte2));

        byte[] anyValueTwoByte3 = [0x38, 0xFF];
        Assert.Equal(CMapStrings.GetIndexValue(anyValueTwoByte3), CMapStrings.GetIndexValue(anyValueTwoByte3));
        Assert.Equal(0x38FF, CMapStrings.GetIndexValue(anyValueTwoByte3));
    }

    [Fact]
    public void GetNonCachedIndexValues()
    {
        // arrays consisting of more than 2 bytes aren't cached.
        Assert.Null(CMapStrings.GetIndexValue([0, 0, 0]));
        Assert.Null(CMapStrings.GetIndexValue([0, 0, 0, 0]));
    }
}
