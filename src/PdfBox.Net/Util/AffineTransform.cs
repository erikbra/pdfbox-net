/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Stub implementation for java.awt.geom.AffineTransform.
 * This is a platform adaptation with no upstream PDFBox Java source equivalent.
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

namespace PdfBox.Net.Util;

public class AffineTransform : IEquatable<AffineTransform>
{
    public double ScaleX { get; private set; } = 1d;

    public double ScaleY { get; private set; } = 1d;

    public double TranslateX { get; private set; }

    public double TranslateY { get; private set; }

    public AffineTransform Clone()
    {
        return new AffineTransform
        {
            ScaleX = ScaleX,
            ScaleY = ScaleY,
            TranslateX = TranslateX,
            TranslateY = TranslateY,
        };
    }

    public void Concatenate(AffineTransform other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ScaleX *= other.ScaleX;
        ScaleY *= other.ScaleY;
        TranslateX += other.TranslateX;
        TranslateY += other.TranslateY;
    }

    public bool Equals(AffineTransform? other)
    {
        return other is not null &&
               ScaleX.Equals(other.ScaleX) &&
               ScaleY.Equals(other.ScaleY) &&
               TranslateX.Equals(other.TranslateX) &&
               TranslateY.Equals(other.TranslateY);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AffineTransform);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ScaleX, ScaleY, TranslateX, TranslateY);
    }

    public void Rotate(double theta)
    {
    }

    public void Scale(double sx, double sy)
    {
        ScaleX *= sx;
        ScaleY *= sy;
    }

    public void Translate(double tx, double ty)
    {
        TranslateX += tx;
        TranslateY += ty;
    }
}
