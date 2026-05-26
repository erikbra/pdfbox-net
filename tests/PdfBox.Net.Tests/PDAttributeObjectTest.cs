/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Tests for document-interchange attribute objects and role/class map APIs
 * introduced in issue #45: PDAttributeObject, PDDefaultAttributeObject,
 * PDUserAttributeObject, PDUserProperty, attribute CRUD on PDStructureElement,
 * and ClassMap read/write on PDStructureTreeRoot.
 *
 * PORT_MODE: native-test
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDAttributeObjectTest
{
    // ── Role map lookup ──────────────────────────────────────────────────────

    [Fact]
    public void StructureTreeRoot_RoleMap_RoundTrip()
    {
        PDStructureTreeRoot root = new();

        Dictionary<string, string> roleMap = new(StringComparer.Ordinal)
        {
            { "MyHeading", "H1" },
            { "MyParagraph", "P" }
        };

        root.SetRoleMap(roleMap);
        Dictionary<string, object> resolved = root.GetRoleMap();

        Assert.Equal("H1", Assert.IsType<string>(resolved["MyHeading"]));
        Assert.Equal("P", Assert.IsType<string>(resolved["MyParagraph"]));
    }

    [Fact]
    public void StructureElement_GetStandardStructureType_FollowsRoleMap()
    {
        PDStructureTreeRoot root = new();
        root.SetRoleMap(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "CustomSect", "Sect" }
        });

        PDStructureElement element = new("CustomSect", root);
        root.AppendKid(element);

        Assert.Equal("Sect", element.GetStandardStructureType());
    }

    [Fact]
    public void StructureElement_GetStandardStructureType_ReturnsOriginalWhenNoMapping()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement element = new("P", root);
        root.AppendKid(element);

        Assert.Equal("P", element.GetStandardStructureType());
    }

    // ── PDAttributeObject factory ────────────────────────────────────────────

    [Fact]
    public void PDAttributeObject_Create_ReturnsUserAttributeObject_ForUserPropertiesOwner()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), PDUserAttributeObject.OwnerUserProperties);

        PDAttributeObject ao = PDAttributeObject.Create(dict);

        Assert.IsType<PDUserAttributeObject>(ao);
        Assert.Equal(PDUserAttributeObject.OwnerUserProperties, ao.GetOwner());
    }

    [Fact]
    public void PDAttributeObject_Create_ReturnsDefaultAttributeObject_ForUnknownOwner()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), "SomeCustomOwner");

        PDAttributeObject ao = PDAttributeObject.Create(dict);

        Assert.IsType<PDDefaultAttributeObject>(ao);
        Assert.Equal("SomeCustomOwner", ao.GetOwner());
    }

    [Fact]
    public void PDAttributeObject_IsEmpty_ReturnsTrueWhenOnlyOwnerPresent()
    {
        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));

        // Only the O entry — isEmpty should be true.
        PDAttributeObject fromDict = PDAttributeObject.Create(ao.GetCOSObject());
        Assert.True(fromDict.IsEmpty());
    }

    // ── PDDefaultAttributeObject ─────────────────────────────────────────────

    [Fact]
    public void DefaultAttributeObject_SetGetAttribute_RoundTrip()
    {
        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));
        ao.SetAttribute("Width", new COSFloat(72.0f));

        Assert.Equal("Layout", Assert.IsType<COSName>(ao.GetAttributeValue("O")).GetName());
        float width = Assert.IsType<COSFloat>(ao.GetAttributeValue("Width")).FloatValue();
        Assert.True(Math.Abs(width - 72.0f) < 0.01f);
        Assert.Contains("Width", ao.GetAttributeNames());
    }

    // ── PDUserAttributeObject + PDUserProperty ───────────────────────────────

    [Fact]
    public void UserAttributeObject_AddRemoveUserProperty_RoundTrip()
    {
        PDUserAttributeObject uao = new();

        PDUserProperty prop1 = new(uao);
        prop1.SetName("Color");
        prop1.SetValue(COSName.GetPDFName("Red"));
        prop1.SetFormattedValue("#FF0000");
        prop1.SetHidden(false);

        PDUserProperty prop2 = new(uao);
        prop2.SetName("FontSize");
        prop2.SetValue(COSInteger.Get(12));

        uao.AddUserProperty(prop1);
        uao.AddUserProperty(prop2);

        List<PDUserProperty> props = uao.GetOwnerUserProperties();
        Assert.Equal(2, props.Count);
        Assert.Equal("Color", props[0].GetName());
        Assert.Equal("FontSize", props[1].GetName());
        Assert.Equal("#FF0000", props[0].GetFormattedValue());
        Assert.False(props[0].IsHidden());

        uao.RemoveUserProperty(prop1);
        Assert.Single(uao.GetOwnerUserProperties());
        Assert.Equal("FontSize", uao.GetOwnerUserProperties()[0].GetName());
    }

    [Fact]
    public void UserAttributeObject_SetUserProperties_ReplacesAll()
    {
        PDUserAttributeObject uao = new();
        PDUserProperty p1 = new(uao);
        p1.SetName("A");
        uao.AddUserProperty(p1);

        PDUserProperty p2 = new(uao);
        p2.SetName("B");
        PDUserProperty p3 = new(uao);
        p3.SetName("C");
        uao.SetUserProperties([p2, p3]);

        List<PDUserProperty> props = uao.GetOwnerUserProperties();
        Assert.Equal(2, props.Count);
        Assert.Equal("B", props[0].GetName());
        Assert.Equal("C", props[1].GetName());
    }

    [Fact]
    public void UserProperty_IsHidden_DefaultFalse()
    {
        PDUserAttributeObject uao = new();
        PDUserProperty prop = new(uao);
        Assert.False(prop.IsHidden());
    }

    // ── PDStructureElement attribute CRUD ────────────────────────────────────

    [Fact]
    public void StructureElement_AddRemoveAttribute_RoundTrip()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement element = new("P", root);
        root.AppendKid(element);

        PDDefaultAttributeObject ao1 = new();
        ao1.SetAttribute("O", COSName.GetPDFName("Layout"));
        ao1.SetAttribute("Width", COSInteger.Get(200));

        PDDefaultAttributeObject ao2 = new();
        ao2.SetAttribute("O", COSName.GetPDFName("Print"));

        element.AddAttribute(ao1);
        element.AddAttribute(ao2);

        Revisions<PDAttributeObject> attributes = element.GetAttributes();
        Assert.Equal(2, attributes.Size());

        element.RemoveAttribute(ao1);
        attributes = element.GetAttributes();
        Assert.Equal(1, attributes.Size());
        Assert.Equal("Print", attributes.GetObject(0).GetOwner());
    }

    [Fact]
    public void StructureElement_SetAttributes_SingleEntryStoredFlat()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement element = new("H1", root);

        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));

        Revisions<PDAttributeObject> rev = new();
        rev.AddObject(ao, 0);
        element.SetAttributes(rev);

        // With a single entry at revision 0, the dictionary should store it flat (not as an array).
        COSBase? a = element.GetCOSObject().GetDictionaryObject(COSName.A);
        Assert.IsType<COSDictionary>(a);
    }

    [Fact]
    public void StructureElement_AttributeChanged_UpdatesRevisionNumber()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement element = new("P", root);
        element.SetRevisionNumber(3);

        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));

        element.AddAttribute(ao);

        // Simulate a change notification.
        element.AttributeChanged(ao);

        Revisions<PDAttributeObject> attributes = element.GetAttributes();
        Assert.Equal(1, attributes.Size());
        Assert.Equal(3, attributes.GetRevisionNumber(0));
    }

    // ── PDStructureTreeRoot ClassMap ─────────────────────────────────────────

    [Fact]
    public void StructureTreeRoot_ClassMap_SingleAttributeRoundTrip()
    {
        PDStructureTreeRoot root = new();

        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));
        ao.SetAttribute("Width", COSInteger.Get(100));

        root.SetClassMap(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "MyClass", ao }
        });

        Dictionary<string, object> classMap = root.GetClassMap();
        Assert.Single(classMap);
        PDAttributeObject resolved = Assert.IsAssignableFrom<PDAttributeObject>(classMap["MyClass"]);
        Assert.IsType<PDLayoutAttributeObject>(resolved);
        Assert.Equal("Layout", resolved.GetOwner());
    }

    [Fact]
    public void StructureTreeRoot_ClassMap_ListAttributeRoundTrip()
    {
        PDStructureTreeRoot root = new();

        PDDefaultAttributeObject ao1 = new();
        ao1.SetAttribute("O", COSName.GetPDFName("Layout"));

        PDDefaultAttributeObject ao2 = new();
        ao2.SetAttribute("O", COSName.GetPDFName("Print"));

        root.SetClassMap(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "MultiClass", new List<PDAttributeObject> { ao1, ao2 } }
        });

        Dictionary<string, object> classMap = root.GetClassMap();
        List<PDAttributeObject> list = Assert.IsType<List<PDAttributeObject>>(classMap["MultiClass"]);
        Assert.Equal(2, list.Count);
        Assert.Equal("Layout", list[0].GetOwner());
        Assert.Equal("Print", list[1].GetOwner());
    }

    [Fact]
    public void StructureTreeRoot_ClassMap_SetNull_RemovesEntry()
    {
        PDStructureTreeRoot root = new();

        PDDefaultAttributeObject ao = new();
        ao.SetAttribute("O", COSName.GetPDFName("Layout"));
        root.SetClassMap(new Dictionary<string, object>(StringComparer.Ordinal) { { "C", ao } });

        root.SetClassMap(null);

        Assert.Empty(root.GetClassMap());
    }


    [Fact]
    public void PDAttributeObject_Create_ReturnsTaggedPdfSubtypes_ForKnownOwners()
    {
        COSDictionary layout = new();
        layout.SetName(COSName.GetPDFName("O"), PDLayoutAttributeObject.Owner);
        COSDictionary list = new();
        list.SetName(COSName.GetPDFName("O"), PDListAttributeObject.Owner);
        COSDictionary table = new();
        table.SetName(COSName.GetPDFName("O"), PDTableAttributeObject.Owner);

        Assert.IsType<PDLayoutAttributeObject>(PDAttributeObject.Create(layout));
        Assert.IsType<PDListAttributeObject>(PDAttributeObject.Create(list));
        Assert.IsType<PDTableAttributeObject>(PDAttributeObject.Create(table));
    }

}
