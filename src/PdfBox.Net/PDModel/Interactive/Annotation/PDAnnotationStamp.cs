/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationRubberStamp.java
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

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public partial class PDAnnotationStamp : PDAnnotationMarkup
{
    public const string NAME_APPROVED = "Approved";
    public const string NAME_EXPERIMENTAL = "Experimental";
    public const string NAME_NOT_APPROVED = "NotApproved";
    public const string NAME_AS_IS = "AsIs";
    public const string NAME_EXPIRED = "Expired";
    public const string NAME_NOT_FOR_PUBLIC_RELEASE = "NotForPublicRelease";
    public const string NAME_FOR_PUBLIC_RELEASE = "ForPublicRelease";
    public const string NAME_DRAFT = "Draft";
    public const string NAME_FOR_COMMENT = "ForComment";
    public const string NAME_TOP_SECRET = "TopSecret";
    public const string NAME_DEPARTMENTAL = "Departmental";
    public const string NAME_CONFIDENTIAL = "Confidential";
    public const string NAME_FINAL = "Final";
    public const string NAME_SOLD = "Sold";

    public const string SUB_TYPE = "Stamp";

    public PDAnnotationStamp()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationStamp(COSDictionary dict)
        : base(dict)
    {
    }

    public void SetName(string name)
    {
        GetCOSDictionary().SetName(COSName.NAME, name);
    }

    public string GetName()
    {
        return GetCOSDictionary().GetNameAsString(COSName.NAME, NAME_DRAFT);
    }
}
