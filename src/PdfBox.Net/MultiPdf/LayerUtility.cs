/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/LayerUtility.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Provides utilities to work with PDF layers (optional content groups / OCG).
/// Allows importing pages as XObject form layers and appending them to existing pages.
/// </summary>
public class LayerUtility
{
    private readonly PDDocument _targetDocument;
    private readonly PDFCloneUtility _cloner;

    /// <summary>
    /// Creates a new <see cref="LayerUtility"/> targeting the given document.
    /// </summary>
    /// <param name="document">The target PDF document.</param>
    public LayerUtility(PDDocument document)
    {
        _targetDocument = document ?? throw new ArgumentNullException(nameof(document));
        _cloner = new PDFCloneUtility(document);
    }

    /// <summary>
    /// Gets the target document.
    /// </summary>
    public PDDocument Document => _targetDocument;

    /// <summary>
    /// Wraps an existing page's content stream in save/restore graphics state operators
    /// so that subsequent additions cannot affect the existing content rendering state.
    /// </summary>
    /// <param name="page">The page whose content to wrap.</param>
    public void WrapInSaveRestore(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        COSBase? existingContents = page.GetContents();
        if (existingContents is null)
        {
            return;
        }

        // Wrap original content with q … Q
        COSStream saveStream = CreateTextStream("q\n");
        COSStream restoreStream = CreateTextStream("Q\n");

        COSArray array = new();
        array.Add(saveStream);
        if (existingContents is COSArray existingArray)
        {
            for (int i = 0; i < existingArray.Size(); i++)
            {
                COSBase? item = existingArray.Get(i);
                if (item is not null)
                {
                    array.Add(item);
                }
            }
        }
        else
        {
            array.Add(existingContents);
        }

        array.Add(restoreStream);
        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.CONTENTS, array);
    }

    /// <summary>
    /// Imports a page from a foreign PDF document as a form XObject in the target document.
    /// </summary>
    /// <param name="sourceDoc">The source document.</param>
    /// <param name="pageIndex">Zero-based index of the page to import.</param>
    /// <returns>A <see cref="PDFormXObject"/> usable in the target document.</returns>
    public PDFormXObject ImportPageAsForm(PDDocument sourceDoc, int pageIndex)
    {
        ArgumentNullException.ThrowIfNull(sourceDoc);
        PDPage sourcePage = sourceDoc.GetPage(pageIndex);
        return ImportPageAsForm(sourceDoc, sourcePage);
    }

    /// <summary>
    /// Imports a specific page from a foreign PDF document as a form XObject.
    /// </summary>
    /// <param name="sourceDoc">The source document.</param>
    /// <param name="sourcePage">The source page.</param>
    /// <returns>A <see cref="PDFormXObject"/> usable in the target document.</returns>
    public PDFormXObject ImportPageAsForm(PDDocument sourceDoc, PDPage sourcePage)
    {
        ArgumentNullException.ThrowIfNull(sourceDoc);
        ArgumentNullException.ThrowIfNull(sourcePage);

        COSStream newStream = (COSStream)_cloner.CloneForNewDocument(GetPageContentStream(sourcePage))!;
        PDFormXObject form = new(newStream);

        // Copy resources
        PDResources? srcResources = sourcePage.GetResources();
        if (srcResources is not null)
        {
            COSDictionary? clonedResDict = (COSDictionary?)_cloner.CloneForNewDocument(srcResources.GetCOSObject());
            if (clonedResDict is not null)
            {
                form.GetCOSObject()?.SetItem(COSName.RESOURCES, clonedResDict);
            }
        }

        // Set bounding box to source media box
        PDRectangle mediaBox = sourcePage.GetMediaBox();
        form.SetBBox(new PDRectangle(mediaBox.GetLowerLeftX(), mediaBox.GetLowerLeftY(), mediaBox.GetWidth(), mediaBox.GetHeight()));
        form.SetFormType(1);

        // Handle source page rotation with an affine transform
        AffineTransform at = new();
        int rotation = sourcePage.GetRotation();
        switch (rotation)
        {
            case 90:
                at.Translate(0, mediaBox.GetWidth());
                at.QuadrantRotate(3);
                break;
            case 180:
                at.Translate(mediaBox.GetWidth(), mediaBox.GetHeight());
                at.QuadrantRotate(2);
                break;
            case 270:
                at.Translate(mediaBox.GetHeight(), 0);
                at.QuadrantRotate(1);
                break;
        }

        if (!at.IsIdentity())
        {
            form.SetMatrix(at);
        }

        return form;
    }

    /// <summary>
    /// Appends a form XObject as a new layer (OCG) onto an existing page.
    /// </summary>
    /// <param name="targetPage">The target page in the target document.</param>
    /// <param name="form">The form XObject to append as a layer.</param>
    /// <param name="transform">An affine transform to apply to the form when rendering. Pass null for identity.</param>
    /// <param name="layerName">The name of the optional content group (layer) to create.</param>
    /// <returns>The optional content group that was created for the layer.</returns>
    public PDOptionalContentGroup AppendFormAsLayer(
        PDPage targetPage,
        PDFormXObject form,
        AffineTransform? transform,
        string layerName)
    {
        ArgumentNullException.ThrowIfNull(targetPage);
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(layerName);

        PDOptionalContentGroup ocg = new(layerName);
        ImportOcProperties(targetPage, ocg);

        using PDPageContentStream cs = new(_targetDocument, targetPage, PDPageContentStream.AppendMode.APPEND, false);
        cs.BeginMarkedContent(COSName.GetPDFName("OC"), ocg);
        cs.SaveGraphicsState();
        if (transform is not null)
        {
            cs.Transform(new Matrix(transform));
        }

        cs.DrawForm(form);
        cs.RestoreGraphicsState();
        cs.EndMarkedContent();

        return ocg;
    }

    private void ImportOcProperties(PDPage targetPage, PDOptionalContentGroup ocg)
    {
        PDDocumentCatalog catalog = _targetDocument.GetDocumentCatalog();
        PDOptionalContentProperties? ocProperties = catalog.GetOCProperties();
        if (ocProperties is null)
        {
            ocProperties = new PDOptionalContentProperties();
            catalog.SetOCProperties(ocProperties);
        }

        ocProperties.AddGroup(ocg);

        // Register the OCG in the page's resource dictionary under /Properties
        PDResources resources = targetPage.GetResources() ?? new PDResources();
        targetPage.SetResources(resources);
        resources.Add(ocg, "MC");
    }

    private static COSBase GetPageContentStream(PDPage page)
    {
        COSBase? contents = page.GetContents();
        if (contents is null)
        {
            // Return an empty but valid stream
            COSStream emptyStream = new();
            using (Stream output = emptyStream.CreateOutputStream())
            {
                // intentionally empty
            }

            return emptyStream;
        }

        return contents;
    }

    private static COSStream CreateTextStream(string content)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = System.Text.Encoding.Latin1.GetBytes(content);
        output.Write(bytes, 0, bytes.Length);
        return stream;
    }
}
