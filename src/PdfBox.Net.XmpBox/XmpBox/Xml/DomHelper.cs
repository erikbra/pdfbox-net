/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for XML helper parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/xml/DomHelper.java
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

using System.Xml;

namespace PdfBox.Net.XmpBox.Xml;

public static class DomHelper
{
    public static XmlElement? GetUniqueElementChild(XmlElement description)
    {
        XmlNodeList nl = description.ChildNodes;
        int pos = -1;
        for (int i = 0; i < nl.Count; i++)
        {
            if (nl[i] is XmlElement)
            {
                if (pos >= 0)
                {
                    throw new XmpParsingException(
                        XmpParsingException.ErrorType.Undefined,
                        $"Found two child elements in {description}");
                }

                pos = i;
            }
        }

        return pos >= 0 ? nl[pos] as XmlElement : null;
    }

    /// <summary>
    /// Return the first child element of the element parameter. If there is no child, null is returned.
    /// </summary>
    /// <param name="description">the parent element</param>
    /// <returns>the first child element. Might be null.</returns>
    public static XmlElement? GetFirstChildElement(XmlElement description)
    {
        XmlNodeList nl = description.ChildNodes;
        for (int i = 0; i < nl.Count; i++)
        {
            if (nl[i] is XmlElement element)
            {
                return element;
            }
        }

        return null;
    }

    public static List<XmlElement> GetElementChildren(XmlElement description)
    {
        XmlNodeList nl = description.ChildNodes;
        List<XmlElement> ret = new(nl.Count);
        for (int i = 0; i < nl.Count; i++)
        {
            if (nl[i] is XmlElement element)
            {
                ret.Add(element);
            }
        }

        return ret;
    }

    public static XmlQualifiedName GetQName(XmlElement element)
    {
        return new XmlQualifiedName(element.LocalName, element.NamespaceURI);
    }

    public static bool IsParseTypeResource(XmlElement element)
    {
        XmlAttribute? parseType = element.GetAttributeNode(XmpConstants.ParseType, XmpConstants.RdfNamespace);
        return parseType is not null && string.Equals(XmpConstants.ResourceName, parseType.Value, StringComparison.Ordinal);
    }
}
