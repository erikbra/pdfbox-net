namespace PdfBox.Net.Tools;

internal static class ToolSupport
{
    internal const int UsageExitCode = 1;
    internal const int IoErrorExitCode = 4;

    internal static NotSupportedException NotSupported(string toolName)
    {
        return new NotSupportedException($"{toolName} is not yet supported in this PdfBox.Net tools port.");
    }

    internal static int Usage(TextWriter error, string message)
    {
        error.WriteLine(message);
        return UsageExitCode;
    }

    internal static int IoError(TextWriter error, string action, Exception ex)
    {
        error.WriteLine($"Error {action} [{ex.GetType().Name}]: {ex.Message}");
        return IoErrorExitCode;
    }

    internal static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    internal static int ReadIntOption(string[] args, ref int index, string optionName)
    {
        string value = ReadOptionValue(args, ref index, optionName);
        if (!int.TryParse(value, out int result))
        {
            throw new ArgumentException($"Value for {optionName} must be an integer.");
        }

        return result;
    }

    internal static float ReadFloatOption(string[] args, ref int index, string optionName)
    {
        string value = ReadOptionValue(args, ref index, optionName);
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            throw new ArgumentException($"Value for {optionName} must be a number.");
        }

        return result;
    }

    internal static bool IsHelpOption(string arg)
    {
        return string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsOption(string arg)
    {
        return arg.StartsWith("-", StringComparison.Ordinal);
    }
}
