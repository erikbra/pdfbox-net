/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/DebugLog.java
 * PDFBOX_SOURCE_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 */

/*
 * Copyright 2016 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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

/// <summary>
/// Custom Log implementation which forwards to LogDialog.
/// </summary>
public sealed class DebugLog
{
    private readonly string _name;

    // hardcoded, but kept to aid with debugging custom builds
    private static readonly bool InfoEnabled = true;
    private static readonly bool TraceEnabled = false;
    private static readonly bool DebugEnabled = false;

    public DebugLog(string name)
    {
        _name = name;
    }

    public void Debug(object? o)
    {
        if (DebugEnabled)
        {
            Log("debug", o, null);
        }
    }

    public void Debug(object? o, Exception? throwable)
    {
        if (DebugEnabled)
        {
            Log("debug", o, throwable);
        }
    }

    public void Error(object? o)
    {
        Log("error", o, null);
    }

    public void Error(object? o, Exception? throwable)
    {
        Log("error", o, throwable);
    }

    public void Fatal(object? o)
    {
        Log("fatal", o, null);
    }

    public void Fatal(object? o, Exception? throwable)
    {
        Log("fatal", o, throwable);
    }

    public void Info(object? o)
    {
        if (InfoEnabled)
        {
            Log("info", o, null);
        }
    }

    public void Info(object? o, Exception? throwable)
    {
        if (InfoEnabled)
        {
            Log("info", o, throwable);
        }
    }

    public bool IsDebugEnabled() => DebugEnabled;

    public bool IsErrorEnabled() => true;

    public bool IsFatalEnabled() => true;

    public bool IsInfoEnabled() => InfoEnabled;

    public bool IsTraceEnabled() => TraceEnabled;

    public bool IsWarnEnabled() => true;

    public void Trace(object? o)
    {
        if (TraceEnabled)
        {
            Log("trace", o, null);
        }
    }

    public void Trace(object? o, Exception? throwable)
    {
        if (TraceEnabled)
        {
            Log("trace", o, throwable);
        }
    }

    public void Warn(object? o)
    {
        Log("warn", o, null);
    }

    public void Warn(object? o, Exception? throwable)
    {
        Log("warn", o, throwable);
    }

    private void Log(string level, object? value, Exception? throwable)
    {
        string message = $"{_name} {level}: {value}";
        if (throwable is not null)
        {
            message += Environment.NewLine + throwable;
        }
        LogDialog.Append(message);
    }
}
