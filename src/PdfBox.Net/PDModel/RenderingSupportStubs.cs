/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Stub implementations for rendering-related PDModel types required by the rendering package.
 * These are adapted stubs pending full porting of the corresponding PDFBox model layers.
 *
 * PORT_MODE: adapted
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

namespace PdfBox.Net.PDModel.Graphics
{
    public enum BlendMode
    {
        NORMAL,
    }
}

namespace PdfBox.Net.PDModel.Common.Function
{
    public abstract class PDFunction
    {
        public virtual float[] Eval(float[] input)
        {
            return input;
        }
    }

    public sealed class PDFunctionTypeIdentity : PDFunction
    {
    }
}

namespace PdfBox.Net.PDModel.Graphics.Color
{
    public class PDColorSpace
    {
        private readonly string _name;
        private readonly int _numberOfComponents;

        public PDColorSpace()
            : this("Unknown", 0)
        {
        }

        public PDColorSpace(string name, int numberOfComponents = 0)
        {
            _name = name;
            _numberOfComponents = numberOfComponents;
        }

        public string GetName()
        {
            return _name;
        }

        public int GetNumberOfComponents()
        {
            return _numberOfComponents;
        }
    }

    public class PDColor
    {
        private readonly PDColorSpace? _colorSpace;
        private readonly float[] _components;

        public PDColor()
            : this(Array.Empty<float>(), null)
        {
        }

        public PDColor(PDColorSpace? colorSpace)
            : this(Array.Empty<float>(), colorSpace)
        {
        }

        public PDColor(float[] components, PDColorSpace? colorSpace)
        {
            _components = components ?? Array.Empty<float>();
            _colorSpace = colorSpace;
        }

        public PDColorSpace? GetColorSpace()
        {
            return _colorSpace;
        }

        public float[] GetComponents()
        {
            return _components;
        }

        public int ToRGB()
        {
            return 0;
        }
    }
}

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

namespace PdfBox.Net.PDModel.Graphics.Form
{
    public class PDFormXObject
    {
    }

    public class PDTransparencyGroup : PDFormXObject
    {
    }
}

namespace PdfBox.Net.PDModel.Graphics.State
{
    public class PDSoftMask
    {
    }

    public class PDLineDashPattern
    {
        private readonly float[] _dashArray;
        private readonly int _phaseStart;

        public PDLineDashPattern()
            : this(Array.Empty<float>(), 0)
        {
        }

        public PDLineDashPattern(float[] dashArray, int phaseStart)
        {
            _dashArray = dashArray ?? Array.Empty<float>();
            _phaseStart = phaseStart;
        }

        public float[] GetDashArray() => _dashArray;

        public int GetPhaseStart() => _phaseStart;
    }
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
