/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for the C# port of Apache PDFBox XMLUtil behavior.
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
using System.Xml;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for <see cref="XMLUtil"/>.
/// </summary>
public class XMLUtilTest
{
    [Fact]
    public void ParseSimpleXmlDocument()
    {
        string xml = "<root><child>hello</child></root>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        XmlDocument doc = XMLUtil.Parse(ms);
        Assert.NotNull(doc.DocumentElement);
        Assert.Equal("root", doc.DocumentElement!.Name);
    }

    [Fact]
    public void ParseNamespaceAwareDocument()
    {
        string xml = "<ns:root xmlns:ns=\"http://example.com\"><ns:child>value</ns:child></ns:root>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        XmlDocument doc = XMLUtil.Parse(ms, preserveNamespaces: true);
        Assert.NotNull(doc.DocumentElement);
        Assert.Equal("http://example.com", doc.DocumentElement!.NamespaceURI);
    }

    [Fact]
    public void ParseInvalidXmlThrowsIOException()
    {
        string badXml = "<unclosed";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(badXml));
        Assert.Throws<IOException>(() => XMLUtil.Parse(ms));
    }

    [Fact]
    public void ParseDocTypeDeclarationThrowsIOException()
    {
        // DOCTYPE processing must be prohibited for security (XXE prevention).
        string xml = "<!DOCTYPE foo [<!ENTITY xxe \"evil\">]><root>&xxe;</root>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        Assert.Throws<IOException>(() => XMLUtil.Parse(ms));
    }

    [Fact]
    public void GetNodeValueReturnsConcatenatedTextChildren()
    {
        string xml = "<root>hello world</root>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        XmlDocument doc = XMLUtil.Parse(ms);
        string value = XMLUtil.GetNodeValue(doc.DocumentElement!);
        Assert.Equal("hello world", value);
    }

    [Fact]
    public void GetNodeValueIgnoresNonTextChildren()
    {
        string xml = "<root>text1<child/>text2</root>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        XmlDocument doc = XMLUtil.Parse(ms);
        string value = XMLUtil.GetNodeValue(doc.DocumentElement!);
        Assert.Equal("text1text2", value);
    }

    [Fact]
    public void GetNodeValueEmptyElementReturnsEmptyString()
    {
        string xml = "<root/>";
        using MemoryStream ms = new(Encoding.UTF8.GetBytes(xml));
        XmlDocument doc = XMLUtil.Parse(ms);
        string value = XMLUtil.GetNodeValue(doc.DocumentElement!);
        Assert.Equal(string.Empty, value);
    }
}
