/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PdfBox.Net.Logging;

/// <summary>
/// Provides the application-wide logger factory used by PdfBox.Net.
/// </summary>
/// <remarks>
/// Applications should assign <see cref="LoggerFactory"/> during startup. The default factory
/// discards all log events. PdfBox.Net does not dispose an application-supplied factory.
/// </remarks>
public static class PdfBoxLogging
{
    private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

    /// <summary>
    /// Gets or sets the application-wide logger factory used by PdfBox.Net.
    /// </summary>
    public static ILoggerFactory LoggerFactory
    {
        get => Volatile.Read(ref _loggerFactory);
        set => Volatile.Write(ref _loggerFactory,
            value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// Creates a logger whose category is the fully qualified name of <typeparamref name="T"/>.
    /// </summary>
    public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
}
