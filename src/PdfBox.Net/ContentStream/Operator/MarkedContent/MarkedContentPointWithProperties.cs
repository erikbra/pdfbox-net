/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/markedcontent/MarkedContentPointWithProperties.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.ContentStream.Operator.MarkedContent;

/// <summary>
/// Processes the PDF "DP" operator: designate a marked-content point with a
/// property dictionary or property list name.
/// </summary>
public sealed class MarkedContentPointWithProperties : OperatorProcessor
{
    public MarkedContentPointWithProperties(PDFStreamEngine context)
        : base(OperatorName.MARKED_CONTENT_POINT_WITH_PROPS, context)
    {
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSName tag)
        {
            return;
        }

        COSDictionary? props = null;
        if (operands[1] is COSName propertyName)
        {
            PDPropertyList? propertyList = Context.GetResources()?.GetProperties(propertyName);
            props = propertyList?.GetCOSObject();
        }
        else if (operands[1] is COSDictionary dictionary)
        {
            props = dictionary;
        }

        if (props == null)
        {
            return;
        }

        Context.MarkedContentPoint(tag, props);
    }
}
