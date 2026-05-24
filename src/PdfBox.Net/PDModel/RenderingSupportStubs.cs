/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted rendering-support stubs derived from Apache PDFBox model and rendering dependencies.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Patterns
{
    public abstract class PDAbstractPattern
    {
    }

    public class PDTilingPattern : PDAbstractPattern
    {
        private readonly COSDictionary _dictionary = new();

        public PDRectangle GetBBox() => new(1, 1);

        public COSDictionary GetCOSObject() => _dictionary;

        public Matrix GetMatrix() => new();

        public float GetXStep() => 1f;

        public float GetYStep() => 1f;
    }

    public class PDShadingPattern : PDAbstractPattern
    {
    }
}

namespace PdfBox.Net.PDModel.Graphics.State
{
}

namespace PdfBox.Net.PDModel.Graphics.OptionalContent
{
    public class PDPropertyList
    {
    }

    public class PDOptionalContentGroup : PDPropertyList
    {
    }

    public class PDOptionalContentMembershipDictionary : PDPropertyList
    {
    }

    public class PDOptionalContentProperties
    {
        public bool IsGroupEnabled(PDOptionalContentGroup group)
        {
            return true;
        }
    }

    public enum RenderState
    {
        ON,
        OFF,
        UNCHANGED,
    }
}

namespace PdfBox.Net.PDModel.Graphics.Image
{
    public class PDImage
    {
    }
}

namespace PdfBox.Net.PDModel.Font
{
    public abstract class PDType3Font : PDFont
    {
        protected PDType3Font()
            : base(new COSDictionary())
        {
        }

        public override string GetName()
        {
            return GetType().Name;
        }
    }
}
