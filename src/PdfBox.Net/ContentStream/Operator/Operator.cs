/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/Operator.java
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

using System.Collections.Concurrent;
using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator;

/// <summary>
/// An Operator in a PDF content stream.
/// </summary>
public sealed partial class Operator
{
    private static readonly ConcurrentDictionary<string, Operator> operators = new();

    private readonly string theOperator;
    private byte[]? imageData;
    private COSDictionary? imageParameters;

    private Operator(string aOperator)
    {
        theOperator = aOperator;
        if (aOperator.StartsWith('/'))
        {
            throw new ArgumentException($"Operators are not allowed to start with / '{aOperator}'", nameof(aOperator));
        }
    }

    /// <summary>
    /// This is used to create/cache operators in the system.
    /// </summary>
    /// <param name="operatorName">The operator for the system.</param>
    /// <returns>The operator that matches the operator keyword.</returns>
    public static Operator GetOperator(string operatorName)
    {
        ArgumentNullException.ThrowIfNull(operatorName);

        if (operatorName.Equals(OperatorName.BEGIN_INLINE_IMAGE_DATA, StringComparison.Ordinal) ||
            operatorName.Equals(OperatorName.BEGIN_INLINE_IMAGE, StringComparison.Ordinal))
        {
            // we can't cache the ID/BI operators.
            return new Operator(operatorName);
        }

        return operators.GetOrAdd(operatorName, static name => new Operator(name));
    }

    /// <summary>
    /// This will get the name of the operator.
    /// </summary>
    /// <returns>The string representation of the operation.</returns>
    public string GetName()
    {
        return theOperator;
    }

    public override string ToString()
    {
        return $"PDFOperator{{{theOperator}}}";
    }

    /// <summary>
    /// This is the special case for the ID operator where there are just random bytes inlined in the stream.
    /// </summary>
    /// <returns>Value of property imageData.</returns>
    public byte[]? GetImageData()
    {
        return imageData;
    }

    /// <summary>
    /// This will set the image data, this is only used for the ID operator.
    /// </summary>
    /// <param name="imageDataArray">New value of property imageData.</param>
    public void SetImageData(byte[]? imageDataArray)
    {
        imageData = imageDataArray;
    }

    /// <summary>
    /// This will get the image parameters, this is only valid for BI operators.
    /// </summary>
    /// <returns>The image parameters.</returns>
    public COSDictionary? GetImageParameters()
    {
        return imageParameters;
    }

    /// <summary>
    /// This will set the image parameters, this is only valid for BI operators.
    /// </summary>
    /// <param name="parameters">The image parameters.</param>
    public void SetImageParameters(COSDictionary? parameters)
    {
        imageParameters = parameters;
    }
}
