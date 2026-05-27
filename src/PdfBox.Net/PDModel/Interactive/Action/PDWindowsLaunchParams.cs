/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDWindowsLaunchParams.java
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

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// Launch parameters for the Windows OS.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDWindowsLaunchParams</c>.</remarks>
public class PDWindowsLaunchParams : COSObjectable
{
    private static readonly COSName OName = COSName.GetPDFName("O");

    /// <summary>
    /// The open operation for the launch.
    /// </summary>
    public const string OPERATION_OPEN = "open";

    /// <summary>
    /// The print operation for the launch.
    /// </summary>
    public const string OPERATION_PRINT = "print";

    /// <summary>
    /// The params dictionary.
    /// </summary>
    protected readonly COSDictionary @params;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDWindowsLaunchParams()
    {
        @params = new COSDictionary();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="p">The params dictionary.</param>
    public PDWindowsLaunchParams(COSDictionary p)
    {
        @params = p;
    }

    public COSBase GetCOSObject()
    {
        return @params;
    }

    /// <summary>
    /// Gets the file to launch.
    /// </summary>
    public string? GetFilename()
    {
        return @params.GetString(COSName.F);
    }

    /// <summary>
    /// Sets the file to launch.
    /// </summary>
    public void SetFilename(string? file)
    {
        @params.SetString(COSName.F, file);
    }

    /// <summary>
    /// Gets the directory to launch from.
    /// </summary>
    public string? GetDirectory()
    {
        return @params.GetString(COSName.D);
    }

    /// <summary>
    /// Sets the directory to launch from.
    /// </summary>
    public void SetDirectory(string? dir)
    {
        @params.SetString(COSName.D, dir);
    }

    /// <summary>
    /// Gets the operation to perform for the file.
    /// </summary>
    public string GetOperation()
    {
        return @params.GetString(OName, OPERATION_OPEN);
    }

    /// <summary>
    /// Sets the operation to perform for the file.
    /// </summary>
    public void SetOperation(string? op)
    {
        @params.SetString(OName, op);
    }

    /// <summary>
    /// Gets a parameter to pass the executable.
    /// </summary>
    public string? GetExecuteParam()
    {
        return @params.GetString(COSName.P);
    }

    /// <summary>
    /// Sets a parameter to pass the executable.
    /// </summary>
    public void SetExecuteParam(string? param)
    {
        @params.SetString(COSName.P, param);
    }
}
