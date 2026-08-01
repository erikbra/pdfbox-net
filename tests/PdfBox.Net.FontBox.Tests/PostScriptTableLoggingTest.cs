using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.Tests;

public class PostScriptTableLoggingTest
{
    [Fact]
    public void TestPostScriptFormatsUseExpectedLogLevelsAndCurrentFactory()
    {
        ILoggerFactory originalFactory = PdfBoxLogging.LoggerFactory;
        RecordingLoggerFactory recordingFactory = new();
        try
        {
            // Initialize PostScriptTable before installing the recording factory. Its logger must
            // still resolve through the factory that is current when the diagnostic is emitted.
            PdfBoxLogging.LoggerFactory = NullLoggerFactory.Instance;
            using (TrueTypeFont beforeConfiguration = new TTFParser().Parse(
                       FontBoxTestFixtures.CreateMinimalTrueTypeWithPostVersion(
                           0x00030000, "BeforeLoggingConfiguration")))
            {
                Assert.Equal(3.0f, beforeConfiguration.GetPostScript()!.FormatType);
            }

            PdfBoxLogging.LoggerFactory = recordingFactory;

            using (TrueTypeFont format3 = new TTFParser().Parse(
                       FontBoxTestFixtures.CreateMinimalTrueTypeWithPostVersion(
                           0x00030000, "Issue916Format3")))
            {
                Assert.Equal(3.0f, format3.GetPostScript()!.FormatType);
            }

            LogRecord format3Record = Assert.Single(recordingFactory.Records,
                record => record.Message.Contains("Issue916Format3", StringComparison.Ordinal));
            Assert.Equal(LogLevel.Debug, format3Record.Level);
            Assert.Equal(typeof(PostScriptTable).FullName, format3Record.Category);

            using (TrueTypeFont format1 = new TTFParser().Parse(
                       FontBoxTestFixtures.CreateMinimalTrueTypeWithPostVersion(
                           0x00010000, "Issue916Format1")))
            {
                PostScriptTable post = Assert.IsType<PostScriptTable>(format1.GetPostScript());
                Assert.Equal(1.0f, post.FormatType);
                Assert.Equal(WGL4Names.NumberOfMacGlyphs, post.GlyphNames!.Length);
            }

            Assert.DoesNotContain(recordingFactory.Records,
                record => record.Message.Contains("Issue916Format1", StringComparison.Ordinal));

            using (TrueTypeFont truncatedFormat2 = new TTFParser().Parse(
                       FontBoxTestFixtures.CreateMinimalTrueTypeWithPostVersion(
                           0x00020000, "Issue916TruncatedFormat2")))
            {
                Assert.Equal(2.0f, truncatedFormat2.GetPostScript()!.FormatType);
            }

            LogRecord format2Record = Assert.Single(recordingFactory.Records,
                record => record.Message.Contains("Issue916TruncatedFormat2", StringComparison.Ordinal));
            Assert.Equal(LogLevel.Warning, format2Record.Level);
        }
        finally
        {
            PdfBoxLogging.LoggerFactory = originalFactory;
            recordingFactory.Dispose();
        }
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public ConcurrentQueue<LogRecord> Records { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, Records);

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger(string category, ConcurrentQueue<LogRecord> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            records.Enqueue(new LogRecord(category, logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record LogRecord(string Category, LogLevel Level, string Message, Exception? Exception);
}
