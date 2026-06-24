/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.PageNavigation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel;

/// <summary>
/// A page in a PDF document.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PDPage</c>.
/// </remarks>
public sealed class PDPage : COSObjectable, PDContentStream
{
    private readonly COSDictionary _page;
    private readonly ResourceCache? _resourceCache;
    private PDRectangle? _mediaBox;

    /// <summary>
    /// Creates a new PDPage instance for embedding, with a size of U.S. Letter (8.5 x 11 inches).
    /// </summary>
    public PDPage()
        : this(PDRectangle.LETTER)
    {
    }

    /// <summary>
    /// Creates a new instance of PDPage for embedding.
    /// </summary>
    /// <param name="mediaBox">The MediaBox of the page.</param>
    public PDPage(PDRectangle mediaBox)
    {
        ArgumentNullException.ThrowIfNull(mediaBox);
        _page = new COSDictionary();
        _page.SetItem(COSName.TYPE, COSName.PAGE);
        _page.SetItem(COSName.MEDIA_BOX, mediaBox.GetCOSArray());
    }

    /// <summary>
    /// Creates a new instance of PDPage for reading.
    /// </summary>
    /// <param name="pageDictionary">A page dictionary in a PDF document.</param>
    public PDPage(COSDictionary pageDictionary)
        : this(pageDictionary, null)
    {
    }

    internal PDPage(COSDictionary pageDictionary, ResourceCache? resourceCache)
    {
        _page = pageDictionary ?? throw new ArgumentNullException(nameof(pageDictionary));
        _resourceCache = resourceCache;
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    /// <returns>The page dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _page;
    }

    /// <summary>
    /// A rectangle, expressed in default user space units, defining the boundaries of the physical
    /// medium on which the page is intended to be displayed or printed.
    /// </summary>
    /// <returns>The media box of the page.</returns>
    public PDRectangle GetMediaBox()
    {
        if (_mediaBox is null)
        {
            COSBase? base_ = PDPageTree.GetInheritableAttribute(_page, COSName.MEDIA_BOX);
            if (base_ is COSArray array)
            {
                _mediaBox = new PDRectangle(array);
            }
            else
            {
                _mediaBox = PDRectangle.LETTER;
            }
        }

        return _mediaBox;
    }

    /// <summary>
    /// Sets the media box for this page.
    /// </summary>
    /// <param name="mediaBox">The new media box for this page.</param>
    public void SetMediaBox(PDRectangle? mediaBox)
    {
        _mediaBox = mediaBox;
        if (mediaBox is null)
        {
            _page.RemoveItem(COSName.MEDIA_BOX);
        }
        else
        {
            _page.SetItem(COSName.MEDIA_BOX, mediaBox.GetCOSArray());
        }
    }

    /// <summary>
    /// A rectangle, expressed in default user space units, defining the visible region of default
    /// user space. When the page is displayed or printed, its contents are to be clipped (cropped)
    /// to this rectangle.
    /// </summary>
    /// <returns>The crop box of the page.</returns>
    public PDRectangle GetCropBox()
    {
        COSBase? base_ = PDPageTree.GetInheritableAttribute(_page, COSName.CROP_BOX);
        if (base_ is COSArray array)
        {
            return ClipToMediaBox(new PDRectangle(array));
        }

        return GetMediaBox();
    }

    /// <summary>
    /// Sets the CropBox for this page.
    /// </summary>
    /// <param name="cropBox">The new CropBox for this page.</param>
    public void SetCropBox(PDRectangle? cropBox)
    {
        if (cropBox is null)
        {
            _page.RemoveItem(COSName.CROP_BOX);
        }
        else
        {
            _page.SetItem(COSName.CROP_BOX, cropBox.GetCOSArray());
        }
    }

    /// <summary>
    /// A rectangle, expressed in default user space units, defining the region to which the contents
    /// of the page should be clipped when output in a production environment. The default is the
    /// CropBox.
    /// </summary>
    /// <returns>The BleedBox attribute.</returns>
    public PDRectangle GetBleedBox()
    {
        COSArray? bleedBox = _page.GetCOSArray(COSName.BLEED_BOX);
        return bleedBox is not null ? ClipToMediaBox(new PDRectangle(bleedBox)) : GetCropBox();
    }

    /// <summary>
    /// Sets the BleedBox for this page.
    /// </summary>
    /// <param name="bleedBox">The new BleedBox for this page.</param>
    public void SetBleedBox(PDRectangle? bleedBox)
    {
        if (bleedBox is null)
        {
            _page.RemoveItem(COSName.BLEED_BOX);
        }
        else
        {
            _page.SetItem(COSName.BLEED_BOX, bleedBox.GetCOSArray());
        }
    }

    /// <summary>
    /// A rectangle, expressed in default user space units, defining the intended dimensions of the
    /// finished page after trimming. The default is the CropBox.
    /// </summary>
    /// <returns>The TrimBox attribute.</returns>
    public PDRectangle GetTrimBox()
    {
        COSArray? trimBox = _page.GetCOSArray(COSName.TRIM_BOX);
        return trimBox is not null ? ClipToMediaBox(new PDRectangle(trimBox)) : GetCropBox();
    }

    /// <summary>
    /// Sets the TrimBox for this page.
    /// </summary>
    /// <param name="trimBox">The new TrimBox for this page.</param>
    public void SetTrimBox(PDRectangle? trimBox)
    {
        if (trimBox is null)
        {
            _page.RemoveItem(COSName.TRIM_BOX);
        }
        else
        {
            _page.SetItem(COSName.TRIM_BOX, trimBox.GetCOSArray());
        }
    }

    /// <summary>
    /// A rectangle, expressed in default user space units, defining the extent of the page's
    /// meaningful content (including potential white space) as intended by the page's creator. The
    /// default is the CropBox.
    /// </summary>
    /// <returns>The ArtBox attribute.</returns>
    public PDRectangle GetArtBox()
    {
        COSArray? artBox = _page.GetCOSArray(COSName.ART_BOX);
        return artBox is not null ? ClipToMediaBox(new PDRectangle(artBox)) : GetCropBox();
    }

    /// <summary>
    /// Sets the ArtBox for this page.
    /// </summary>
    /// <param name="artBox">The new ArtBox for this page.</param>
    public void SetArtBox(PDRectangle? artBox)
    {
        if (artBox is null)
        {
            _page.RemoveItem(COSName.ART_BOX);
        }
        else
        {
            _page.SetItem(COSName.ART_BOX, artBox.GetCOSArray());
        }
    }

    /// <summary>
    /// Returns the rotation angle in degrees by which the page should be rotated clockwise when
    /// displayed or printed. Valid values in a PDF must be a multiple of 90.
    /// </summary>
    /// <returns>
    /// The rotation angle in degrees in normalized form (0, 90, 180, or 270), or 0 if invalid
    /// or not set at this level.
    /// </returns>
    public int GetRotation()
    {
        COSBase? obj = PDPageTree.GetInheritableAttribute(_page, COSName.ROTATE);
        if (obj is COSNumber number)
        {
            int rotationAngle = number.IntValue();
            if (rotationAngle % 90 == 0)
            {
                return (rotationAngle % 360 + 360) % 360;
            }
        }

        return 0;
    }

    /// <summary>
    /// Sets the rotation for this page.
    /// </summary>
    /// <param name="rotation">The new rotation for this page in degrees.</param>
    public void SetRotation(int rotation)
    {
        _page.SetInt(COSName.ROTATE, rotation);
    }

    /// <summary>
    /// Gets the key of this Page in the structural parent tree.
    /// </summary>
    /// <returns>
    /// The integer key of the page's entry in the structural parent tree, or -1 if there isn't any.
    /// </returns>
    public int GetStructParents()
    {
        return _page.GetInt(COSName.STRUCT_PARENTS);
    }

    /// <summary>
    /// Sets the key for this page in the structural parent tree.
    /// </summary>
    /// <param name="structParents">The new key for this page.</param>
    public void SetStructParents(int structParents)
    {
        _page.SetInt(COSName.STRUCT_PARENTS, structParents);
    }

    /// <summary>
    /// Returns <see langword="true"/> if this page has one or more content streams.
    /// </summary>
    /// <returns><see langword="true"/> if the page has a non-empty content stream, otherwise <see langword="false"/>.</returns>
    public bool HasContents()
    {
        COSBase? contents = _page.GetDictionaryObject(COSName.CONTENTS);
        if (contents is COSStream stream)
        {
            return stream.HasData();
        }
        else if (contents is COSArray array)
        {
            return !array.IsEmpty();
        }

        return false;
    }

    public IEnumerable<PDThreadBead> GetThreadBeads()
    {
        COSArray? beads = _page.GetCOSArray(COSName.B);
        if (beads == null)
        {
            beads = new COSArray();
        }

        List<PDThreadBead> pdObjects = new(beads.Size());
        for (int i = 0; i < beads.Size(); i++)
        {
            if (beads.GetObject(i) is COSDictionary dictionary)
            {
                pdObjects.Add(new PDThreadBead(dictionary));
            }
        }

        return new COSArrayList<PDThreadBead>(pdObjects, beads);
    }

    /// <summary>
    /// Sets the list of thread beads.
    /// </summary>
    /// <param name="beads">A list of <see cref="PDThreadBead"/> objects or null.</param>
    public void SetThreadBeads(IList<PDThreadBead>? beads)
    {
        if (beads == null)
        {
            _page.RemoveItem(COSName.B);
            return;
        }

        _page.SetItem(COSName.B, COSArrayList<object>.ConverterToCOSArray(beads.Cast<object>().ToList()));
    }

    /// <summary>
    /// Gets the page transition associated with this page, if any.
    /// </summary>
    public PDTransition? GetTransition()
    {
        COSDictionary? transition = _page.GetCOSDictionary(COSName.GetPDFName("Trans"));
        return transition != null ? new PDTransition(transition) : null;
    }

    /// <summary>
    /// Sets the page transition.
    /// </summary>
    /// <param name="transition">The transition to set, or null to clear.</param>
    public void SetTransition(PDTransition? transition)
    {
        _page.SetItem(COSName.GetPDFName("Trans"), transition);
    }

    /// <summary>
    /// Sets the page transition and display duration.
    /// </summary>
    /// <param name="transition">The transition to set.</param>
    /// <param name="duration">The maximum display duration in seconds.</param>
    public void SetTransition(PDTransition transition, float duration)
    {
        _page.SetItem(COSName.GetPDFName("Trans"), transition);
        _page.SetItem(COSName.GetPDFName("Dur"), new COSFloat(duration));
    }

    /// <summary>
    /// Returns the annotation list for this page.
    /// </summary>
    public IList<PDAnnotation> GetAnnotations()
    {
        COSArray? annots = _page.GetCOSArray(COSName.GetPDFName("Annots"));
        if (annots == null)
        {
            return new COSArrayList<PDAnnotation>(_page, COSName.GetPDFName("Annots"));
        }

        List<PDAnnotation> annotations = new(annots.Size());
        for (int i = 0; i < annots.Size(); i++)
        {
            COSBase? item = annots.GetObject(i);
            if (item is COSDictionary dictionary)
            {
                annotations.Add(PDAnnotation.CreateAnnotation(dictionary));
            }
        }
        return new COSArrayList<PDAnnotation>(annotations, annots);
    }

    /// <summary>
    /// Sets the annotation list for this page.
    /// </summary>
    public void SetAnnotations(IList<PDAnnotation>? annotations)
    {
        _page.SetItem(COSName.GetPDFName("Annots"), COSArrayList<object>.ConverterToCOSArray(annotations?.Cast<object>().ToList()));
    }

    /// <summary>
    /// Gets the page additional actions.
    /// </summary>
    public PDPageAdditionalActions GetActions()
    {
        COSDictionary? addAct = _page.GetCOSDictionary(COSName.AA);
        if (addAct == null)
        {
            addAct = new COSDictionary();
            _page.SetItem(COSName.AA, addAct);
        }
        return new PDPageAdditionalActions(addAct);
    }

    /// <summary>
    /// Sets the page additional actions.
    /// </summary>
    public void SetActions(PDPageAdditionalActions? actions)
    {
        _page.SetItem(COSName.AA, actions);
    }

    public COSBase? GetContents()
    {
        return _page.GetDictionaryObject(COSName.CONTENTS);
    }

    public void SetContents(PDStream? contents)
    {
        _page.SetItem(COSName.CONTENTS, contents);
    }

    public void SetContents(IList<PDStream> contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        COSArray array = new();
        foreach (PDStream stream in contents)
        {
            array.Add(stream.GetCOSObject());
        }

        _page.SetItem(COSName.CONTENTS, array);
    }

    Stream? PDContentStream.GetContents()
    {
        COSBase? contents = GetContents();
        return contents switch
        {
            COSStream stream => stream.CreateInputStream(),
            COSArray array => CreateConcatenatedContentsStream(array),
            _ => null,
        };
    }

    RandomAccessRead? PDContentStream.GetContentsForRandomAccess()
    {
        Stream? contents = ((PDContentStream)this).GetContents();
        return contents is null ? null : new RandomAccessReadBuffer(contents);
    }

    /// <summary>
    /// Returns the resource dictionary for this page, or <see langword="null"/> if none is set.
    /// Resources are inherited from parent nodes in the page tree when not set directly on the page.
    /// </summary>
    public PDResources? GetResources()
    {
        COSBase? resourceBase = PDPageTree.GetInheritableAttribute(_page, COSName.RESOURCES);
        if (resourceBase is COSDictionary resourceDict)
        {
            return new PDResources(resourceDict, _resourceCache);
        }

        return null;
    }

    PDRectangle? PDContentStream.GetBBox() => GetCropBox();

    Matrix PDContentStream.GetMatrix() => new();

    /// <summary>
    /// Sets the resource dictionary for this page.
    /// </summary>
    /// <param name="resources">The resource dictionary, or <see langword="null"/> to remove it.</param>
    public void SetResources(PDResources? resources)
    {
        if (resources is null)
        {
            _page.RemoveItem(COSName.RESOURCES);
        }
        else
        {
            _page.SetItem(COSName.RESOURCES, resources.GetCOSObject());
        }
    }

    public void RemovePageResourceFromCache()
    {
        if (_resourceCache is null)
        {
            return;
        }

        RemovePageResources(COSName.GetPDFName("Font"), indirect => _resourceCache.RemoveFont(indirect));
        RemovePageResources(COSName.GetPDFName("ColorSpace"), indirect => _resourceCache.RemoveColorSpace(indirect));
        RemovePageResources(COSName.GetPDFName("XObject"), indirect => _resourceCache.RemoveXObject(indirect));
        RemovePageResources(COSName.GetPDFName("ExtGState"), indirect => _resourceCache.RemoveExtState(indirect));
        RemovePageResources(COSName.GetPDFName("Shading"), indirect => _resourceCache.RemoveShading(indirect));
        RemovePageResources(COSName.GetPDFName("Pattern"), indirect => _resourceCache.RemovePattern(indirect));
        RemovePageResources(COSName.GetPDFName("Properties"), indirect => _resourceCache.RemoveProperties(indirect));
    }

    private void RemovePageResources(COSName category, Action<COSObject> removeAction)
    {
        COSBase? resourceBase = PDPageTree.GetInheritableAttribute(_page, COSName.RESOURCES);
        if (resourceBase is not COSDictionary resources)
        {
            return;
        }

        COSDictionary? categoryDictionary = resources.GetCOSDictionary(category);
        if (categoryDictionary is null)
        {
            return;
        }

        foreach (COSName name in categoryDictionary.KeySet())
        {
            if (categoryDictionary.GetItem(name) is COSObject indirect)
            {
                removeAction(indirect);
            }
        }
    }

    /// <summary>
    /// Clips the given box to the bounds of the media box.
    /// </summary>
    private PDRectangle ClipToMediaBox(PDRectangle box)
    {
        PDRectangle mediaBox = GetMediaBox();
        PDRectangle result = new();
        result.SetLowerLeftX(Math.Max(mediaBox.GetLowerLeftX(), box.GetLowerLeftX()));
        result.SetLowerLeftY(Math.Max(mediaBox.GetLowerLeftY(), box.GetLowerLeftY()));
        result.SetUpperRightX(Math.Min(mediaBox.GetUpperRightX(), box.GetUpperRightX()));
        result.SetUpperRightY(Math.Min(mediaBox.GetUpperRightY(), box.GetUpperRightY()));
        return result;
    }

    private static Stream CreateConcatenatedContentsStream(COSArray array)
    {
        MemoryStream output = new();
        bool first = true;
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is not COSStream stream)
            {
                continue;
            }

            if (!first)
            {
                output.WriteByte((byte)'\n');
            }

            using Stream input = stream.CreateInputStream();
            input.CopyTo(output);
            first = false;
        }

        output.Position = 0;
        return output;
    }
}
