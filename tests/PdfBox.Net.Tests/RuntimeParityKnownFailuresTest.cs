using System.Text.Json;

namespace PdfBox.Net.Tests;

public class RuntimeParityKnownFailuresTest
{
    [Fact]
    public void KnownFailures_DoNotContainClosedIssue438EncryptionDiagnostics()
    {
        string path = FindRepoFile("tools/parity/runtime/known-failures.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        JsonElement.ArrayEnumerator entries = document.RootElement.GetProperty("entries").EnumerateArray();
        string[] staleIssue438Entries = entries
            .Where(entry => entry.TryGetProperty("owner", out JsonElement owner)
                && owner.GetString() == "issue-438")
            .Select(entry => entry.GetProperty("id").GetString() ?? string.Empty)
            .Where(id => id.Length != 0)
            .ToArray();

        Assert.Empty(staleIssue438Entries);
    }

    private static string FindRepoFile(string relativePath)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{relativePath}'.");
    }
}
