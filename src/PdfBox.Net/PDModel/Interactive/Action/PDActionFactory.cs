/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionFactory.java
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
/// This class will take a dictionary and determine which type of action to create.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionFactory</c>.</remarks>
public static class PDActionFactory
{
    /// <summary>
    /// This will create the correct type of action based on the type specified in the dictionary.
    /// </summary>
    /// <param name="action">An action dictionary.</param>
    /// <returns>An action of the correct type.</returns>
    public static PDAction? CreateAction(COSDictionary? action)
    {
        if (action != null)
        {
            string? type = action.GetNameAsString(COSName.S);
            if (type != null)
            {
                return type switch
                {
                    PDActionJavaScript.SUB_TYPE => new PDActionJavaScript(action),
                    PDActionGoTo.SUB_TYPE => new PDActionGoTo(action),
                    PDActionLaunch.SUB_TYPE => new PDActionLaunch(action),
                    PDActionRemoteGoTo.SUB_TYPE => new PDActionRemoteGoTo(action),
                    PDActionURI.SUB_TYPE => new PDActionURI(action),
                    PDActionNamed.SUB_TYPE => new PDActionNamed(action),
                    _ => null
                };
            }
        }
        return null;
    }
}
