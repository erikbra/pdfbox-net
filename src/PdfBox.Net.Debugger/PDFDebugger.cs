/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/PDFDebugger.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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

using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;
using PdfBox.Net.Debugger.Certificatepane;

namespace PdfBox.Net.Debugger;

/// <summary>PDF document structure inspector.</summary>
public sealed class PDFDebugger
{
    private static ILogger<PDFDebugger> LOG => PdfBoxLogging.CreateLogger<PDFDebugger>();

    private PDFDebugger()
    {
    }

    public static string? GetPageLabel(PdfBox.Net.PDModel.PDDocument doc, int pageIndex)
    {
        string[]? labels = doc.GetDocumentCatalog().GetPageLabels()?.GetLabelsByPageIndices();
        return labels is not null && pageIndex >= 0 && pageIndex < labels.Length ? labels[pageIndex] : null;
    }

    public static void InspectDocument(PdfBox.Net.PDModel.PDDocument doc, System.IO.TextWriter output)
    {
        System.ArgumentNullException.ThrowIfNull(doc);
        System.ArgumentNullException.ThrowIfNull(output);

        output.WriteLine("PDF Debugger");
        output.WriteLine($"Pages: {doc.GetNumberOfPages()}");
        for (int i = 0; i < doc.GetNumberOfPages(); i++)
        {
            string? label = GetPageLabel(doc, i);
            output.WriteLine(label is null ? $"  Page {i + 1}" : $"  Page {i + 1}: {label}");
        }

        output.WriteLine("Trailer:");
        DumpCOSTree(doc.GetDocument().GetTrailer(), output, 0);

        Ui.XrefEntries xrefEntries = new(doc);
        output.WriteLine($"XRef entries: {xrefEntries.GetXrefEntryCount()}");
    }

    public static void DumpCOSTree(object? node, System.IO.TextWriter output, int depth = 0)
    {
        System.Collections.Generic.HashSet<object> visited = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);
        DumpNode(node, output, depth, visited);
    }

    private static void DumpNode(object? node, System.IO.TextWriter output, int depth, System.Collections.Generic.HashSet<object> visited)
    {
        string indent = new(' ', depth * 2);
        switch (node)
        {
            case null:
                output.WriteLine(indent + "(null)");
                return;
            case PdfBox.Net.COS.COSObject cosObject:
                output.WriteLine(indent + "COSObject");
                if (!visited.Add(cosObject))
                {
                    output.WriteLine(indent + "  <visited>");
                    return;
                }
                DumpNode(cosObject.GetObject(), output, depth + 1, visited);
                return;
            case PdfBox.Net.COS.COSDictionary dictionary:
                output.WriteLine(indent + "COSDictionary");
                if (!visited.Add(dictionary))
                {
                    output.WriteLine(indent + "  <visited>");
                    return;
                }
                System.Collections.Generic.List<PdfBox.Net.COS.COSName> keys = new(dictionary.KeySet());
                keys.Sort(static (a, b) => string.Compare(a.GetName(), b.GetName(), System.StringComparison.Ordinal));
                foreach (PdfBox.Net.COS.COSName key in keys)
                {
                    output.WriteLine(indent + "  " + key.GetName() + ":");
                    object? value = dictionary.GetDictionaryObject(key);
                    if ((key.GetName() == "Cert" || key.GetName() == "Certs") &&
                        DumpCertificates(value, output, depth + 2))
                    {
                        continue;
                    }
                    DumpNode(value, output, depth + 2, visited);
                }
                return;
            case PdfBox.Net.COS.COSArray array:
                output.WriteLine(indent + "COSArray");
                if (!visited.Add(array))
                {
                    output.WriteLine(indent + "  <visited>");
                    return;
                }
                for (int i = 0; i < array.Size(); i++)
                {
                    output.WriteLine(indent + $"  [{i}]:");
                    DumpNode(array.GetObject(i), output, depth + 2, visited);
                }
                return;
            default:
                output.WriteLine(indent + node);
                return;
        }
    }

    private static bool DumpCertificates(object? value, System.IO.TextWriter output, int depth)
    {
        string indent = new(' ', depth * 2);
        if (value is PdfBox.Net.COS.COSString or PdfBox.Net.COS.COSStream)
        {
            output.WriteLine(indent + "Certificate View:");
            using System.IO.StringReader lines = new(new CertificatePane((PdfBox.Net.COS.COSBase)value).GetText());
            while (lines.ReadLine() is string line)
            {
                output.WriteLine(indent + "  " + line);
            }
            return true;
        }
        if (value is PdfBox.Net.COS.COSArray certificates)
        {
            // Certificate arrays in DSS dictionaries contain certificate streams.
            for (int i = 0; i < certificates.Size(); i++)
            {
                if (certificates.GetObject(i) is not (PdfBox.Net.COS.COSString or PdfBox.Net.COS.COSStream))
                {
                    return false;
                }
            }
            for (int i = 0; i < certificates.Size(); i++)
            {
                output.WriteLine(indent + $"[{i}]:");
                DumpCertificates(certificates.GetObject(i), output, depth + 1);
            }
            return true;
        }
        return false;
    }
}
