/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/util/autodetect/NativeFontDirFinder.java
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

using System.IO;
using System.Security;

namespace PdfBox.Net.FontBox.Util.Autodetect;

/// <summary>
/// Native font finder base class. This class is based on a class provided by Apache FOP.
/// </summary>
public abstract class NativeFontDirFinder : FontDirFinder
{
    /// <summary>
    /// Generic method used by Mac and Unix font finders.
    /// </summary>
    /// <returns>list of natively existing font directories.</returns>
    public virtual IList<DirectoryInfo> Find()
    {
        List<DirectoryInfo> fontDirList = [];
        string[] searchableDirectories = GetSearchableDirectories();
        if (searchableDirectories != null)
        {
            foreach (string searchableDirectory in searchableDirectories)
            {
                DirectoryInfo fontDir = new(searchableDirectory);
                try
                {
                    if (DirectoryExistsAndIsReadable(fontDir))
                    {
                        fontDirList.Add(fontDir);
                    }
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        return fontDirList;
    }

    /// <summary>
    /// Returns an array of directories to search for fonts in.
    /// </summary>
    /// <returns>an array of directories.</returns>
    protected abstract string[] GetSearchableDirectories();

    internal static bool DirectoryExistsAndIsReadable(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return false;
        }

        using IEnumerator<FileSystemInfo> enumerator = directory.EnumerateFileSystemInfos().GetEnumerator();
        _ = enumerator.MoveNext();
        return true;
    }
}
