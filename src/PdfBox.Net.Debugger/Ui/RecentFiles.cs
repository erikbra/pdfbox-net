/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/RecentFiles.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Stores recent file history for the adapted debugger.</summary>
public sealed class RecentFiles
{
    private readonly int _maximum;
    private readonly string _storagePath;
    private readonly System.Collections.Generic.List<string> _filePaths;

    public RecentFiles(System.Type ownerType, int maximumFile)
    {
        _maximum = maximumFile;
        string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        string directory = System.IO.Path.Combine(appData, "PdfBox.Net.Debugger");
        string fileName = (ownerType.FullName ?? ownerType.Name).Replace(".", "_", System.StringComparison.Ordinal) + "_recent-files.json";
        _storagePath = System.IO.Path.Combine(directory, fileName);
        _filePaths = ReadHistoryFromDisk();
    }

    public void RemoveAll() => _filePaths.Clear();

    public bool IsEmpty() => _filePaths.Count == 0;

    public void AddFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        _filePaths.RemoveAll(existing => string.Equals(existing, path, System.StringComparison.Ordinal));
        _filePaths.Add(path);
        while (_filePaths.Count > _maximum + 1)
        {
            _filePaths.RemoveAt(0);
        }
    }

    public void RemoveFile(string path) => _filePaths.RemoveAll(existing => string.Equals(existing, path, System.StringComparison.Ordinal));

    public System.Collections.Generic.List<string> GetFiles()
    {
        System.Collections.Generic.List<string> files = _filePaths.Where(System.IO.File.Exists).ToList();
        if (files.Count > _maximum)
        {
            files.RemoveAt(0);
        }
        return files;
    }

    public void Close()
    {
        string? directory = System.IO.Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrEmpty(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        string json = System.Text.Json.JsonSerializer.Serialize(_filePaths);
        System.IO.File.WriteAllText(_storagePath, json);
    }

    private System.Collections.Generic.List<string> ReadHistoryFromDisk()
    {
        if (!System.IO.File.Exists(_storagePath))
        {
            return new System.Collections.Generic.List<string>();
        }

        try
        {
            string json = System.IO.File.ReadAllText(_storagePath);
            return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json) ?? new System.Collections.Generic.List<string>();
        }
        catch
        {
            return new System.Collections.Generic.List<string>();
        }
    }
}
