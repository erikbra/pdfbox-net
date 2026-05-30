/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/model/GsubData.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

namespace PdfBox.Net.FontBox.TTF.Model;

/// <summary>
/// Model for data from the GSUB tables
/// </summary>
public interface IGsubData
{
    /// <summary>
    /// To be used when there is no GSUB data available
    /// </summary>
    static readonly IGsubData NoDataFound = new NoDataFoundGsubData();

    Language GetLanguage();

    /// <summary>
    /// A <see cref="Language"/> can have more than one script that is supported. However, at any given
    /// point, only one of the many scripts are active.
    /// </summary>
    /// <returns>The name of the script that is active.</returns>
    string GetActiveScriptName();

    bool IsFeatureSupported(string featureName);

    IScriptFeature GetFeature(string featureName);

    ISet<string> GetSupportedFeatures();

    private sealed class NoDataFoundGsubData : IGsubData
    {
        public Language GetLanguage() => throw new NotSupportedException();
        public string GetActiveScriptName() => throw new NotSupportedException();
        public bool IsFeatureSupported(string featureName) => throw new NotSupportedException();
        public IScriptFeature GetFeature(string featureName) => throw new NotSupportedException();
        public ISet<string> GetSupportedFeatures() => throw new NotSupportedException();
    }
}
