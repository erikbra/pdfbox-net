/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/util/autodetect/FontFileFinder.java
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
using System.Runtime.InteropServices;
using System.Security;

namespace PdfBox.Net.FontBox.Util.Autodetect;

/// <summary>
/// Helps to autodetect/locate available operating system fonts. This class is based on a class provided by Apache FOP.
/// </summary>
public class FontFileFinder
{
    private FontDirFinder? _fontDirFinder;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public FontFileFinder()
    {
    }

    /// <summary>
    /// Constructor with an explicit directory finder.
    /// </summary>
    /// <param name="fontDirFinder">Directory finder to use.</param>
    public FontFileFinder(FontDirFinder fontDirFinder)
    {
        _fontDirFinder = fontDirFinder ?? throw new ArgumentNullException(nameof(fontDirFinder));
    }

    /// <summary>
    /// Automagically finds a list of font files on local system.
    /// </summary>
    /// <returns>List of font files.</returns>
    public virtual IList<Uri> Find()
    {
        _fontDirFinder ??= DetermineDirFinder();
        IList<DirectoryInfo> fontDirs = _fontDirFinder.Find();
        List<Uri> results = [];
        foreach (DirectoryInfo dir in fontDirs)
        {
            Walk(dir, results);
        }

        return results;
    }

    /// <summary>
    /// Searches a given directory for font files.
    /// </summary>
    /// <param name="dir">directory to search.</param>
    /// <returns>list of font files.</returns>
    public virtual IList<Uri> Find(string dir)
    {
        List<Uri> results = [];
        DirectoryInfo directory = new(dir);
        if (directory.Exists)
        {
            Walk(directory, results);
        }

        return results;
    }

    protected virtual FontDirFinder DetermineDirFinder()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsFontDirFinder();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacFontDirFinder();
        }

        if (RuntimeInformation.OSDescription.StartsWith("OS/400", StringComparison.Ordinal))
        {
            return new OS400FontDirFinder();
        }

        return new UnixFontDirFinder();
    }

    private static void Walk(DirectoryInfo directory, ICollection<Uri> results)
    {
        if (!directory.Exists)
        {
            return;
        }

        IEnumerable<FileSystemInfo> fileList;
        try
        {
            fileList = directory.EnumerateFileSystemInfos();
        }
        catch (IOException)
        {
            return;
        }
        catch (SecurityException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        foreach (FileSystemInfo file in fileList)
        {
            if (file is DirectoryInfo childDirectory)
            {
                if (IsHidden(childDirectory))
                {
                    continue;
                }

                Walk(childDirectory, results);
            }
            else if (file is FileInfo fontFile && CheckFontfile(fontFile))
            {
                results.Add(new Uri(fontFile.FullName));
            }
        }
    }

    private static bool CheckFontfile(FileInfo file)
    {
        string name = file.Name;
        return (name.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)) &&
               !name.StartsWith("fonts.", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHidden(FileSystemInfo file)
    {
        try
        {
            return file.Name.StartsWith(".", StringComparison.Ordinal) ||
                   (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }
        catch (IOException)
        {
            return true;
        }
        catch (SecurityException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }
}
