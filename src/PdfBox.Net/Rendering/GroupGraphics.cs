/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/GroupGraphics.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

namespace PdfBox.Net.Rendering;

internal class GroupGraphics : Graphics2D
{
    private readonly BufferedImage _groupImage;
    private readonly BufferedImage _groupAlphaImage;
    private readonly Graphics2D _groupGraphics;
    private readonly Graphics2D _alphaGraphics;

    internal GroupGraphics(BufferedImage groupImage, Graphics2D groupGraphics)
    {
        _groupImage = groupImage;
        _groupGraphics = groupGraphics;
        _groupAlphaImage = new BufferedImage(groupImage.Width, groupImage.Height, BufferedImage.TYPE_INT_ARGB);
        _alphaGraphics = _groupAlphaImage.CreateGraphics();
    }

    private GroupGraphics(BufferedImage groupImage, Graphics2D groupGraphics, BufferedImage groupAlphaImage, Graphics2D alphaGraphics)
    {
        _groupImage = groupImage;
        _groupGraphics = groupGraphics;
        _groupAlphaImage = groupAlphaImage;
        _alphaGraphics = alphaGraphics;
    }

    public override Graphics Create()
    {
        return new GroupGraphics(_groupImage, (Graphics2D)_groupGraphics.Create(), _groupAlphaImage, (Graphics2D)_alphaGraphics.Create());
    }

    public override void ClearRect(int x, int y, int width, int height)
    {
        _groupGraphics.ClearRect(x, y, width, height);
        _alphaGraphics.ClearRect(x, y, width, height);
    }

    public override void Dispose()
    {
        _groupGraphics.Dispose();
        _alphaGraphics.Dispose();
    }

    public override void Rotate(double theta)
    {
        _groupGraphics.Rotate(theta);
        _alphaGraphics.Rotate(theta);
    }

    public override void Scale(double scaleX, double scaleY)
    {
        _groupGraphics.Scale(scaleX, scaleY);
        _alphaGraphics.Scale(scaleX, scaleY);
    }

    public override void SetBackground(Color color)
    {
        _groupGraphics.SetBackground(color);
        _alphaGraphics.SetBackground(color);
    }

    public override void Translate(double tx, double ty)
    {
        _groupGraphics.Translate(tx, ty);
        _alphaGraphics.Translate(tx, ty);
    }

    internal void RemoveBackdrop(BufferedImage backdrop, int offsetX, int offsetY)
    {
    }
}
