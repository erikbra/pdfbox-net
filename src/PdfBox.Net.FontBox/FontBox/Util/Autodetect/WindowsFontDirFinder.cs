/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/util/autodetect/WindowsFontDirFinder.java
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace PdfBox.Net.FontBox.Util.Autodetect;

/// <summary>
/// FontFinder for native Windows platforms. This class is based on a class provided by Apache FOP.
/// </summary>
public class WindowsFontDirFinder : FontDirFinder
{
    /// <summary>
    /// Finds a list of detected font files.
    /// </summary>
    /// <returns>a list of detected font files.</returns>
    public virtual IList<DirectoryInfo> Find()
    {
        List<DirectoryInfo> fontDirList = [];
        string? windir = null;
        try
        {
            windir = GetWindowsDirectory();
        }
        catch (SecurityException)
        {
        }

        if (!string.IsNullOrEmpty(windir) && windir.Length > 2)
        {
            windir = windir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            TryAddReadableDirectory(fontDirList, Path.Combine(windir, "FONTS"));
            TryAddReadableDirectory(fontDirList, GetPostScriptFontsDirectory(windir));
        }
        else
        {
            string windowsDirName = GetOsName().EndsWith("NT", StringComparison.Ordinal) ? "WINNT" : "WINDOWS";
            for (char driveLetter = 'C'; driveLetter <= 'E'; driveLetter++)
            {
                string osFontsPath = $"{driveLetter}:{Path.DirectorySeparatorChar}{windowsDirName}{Path.DirectorySeparatorChar}FONTS";
                if (TryAddReadableDirectory(fontDirList, osFontsPath))
                {
                    break;
                }
            }

            for (char driveLetter = 'C'; driveLetter <= 'E'; driveLetter++)
            {
                string psFontsPath = $"{driveLetter}:{Path.DirectorySeparatorChar}PSFONTS";
                if (TryAddReadableDirectory(fontDirList, psFontsPath))
                {
                    break;
                }
            }
        }

        try
        {
            string? localAppData = GetLocalAppDataDirectory();
            if (!string.IsNullOrEmpty(localAppData))
            {
                TryAddReadableDirectory(fontDirList, Path.Combine(localAppData, "Microsoft", "Windows", "Fonts"));
            }
        }
        catch (SecurityException)
        {
        }

        return fontDirList;
    }

    protected virtual string? GetWindowsDirectory()
    {
        string? windir = Environment.GetEnvironmentVariable("windir");
        if (!string.IsNullOrEmpty(windir))
        {
            return windir;
        }

        string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return string.IsNullOrEmpty(specialFolder) ? null : specialFolder;
    }

    protected virtual string GetOsName()
    {
        return RuntimeInformation.OSDescription;
    }

    protected virtual string GetPostScriptFontsDirectory(string windir)
    {
        return Path.Combine(Path.GetPathRoot(windir) ?? string.Empty, "PSFONTS");
    }

    protected virtual string? GetLocalAppDataDirectory()
    {
        string? localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        if (!string.IsNullOrEmpty(localAppData))
        {
            return localAppData;
        }

        string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return string.IsNullOrEmpty(specialFolder) ? null : specialFolder;
    }

    private static bool TryAddReadableDirectory(ICollection<DirectoryInfo> fontDirList, string path)
    {
        try
        {
            DirectoryInfo directory = new(path);
            if (NativeFontDirFinder.DirectoryExistsAndIsReadable(directory))
            {
                fontDirList.Add(directory);
                return true;
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

        return false;
    }
}
