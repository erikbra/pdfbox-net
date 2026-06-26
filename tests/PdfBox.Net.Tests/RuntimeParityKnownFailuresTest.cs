using System.Text.Json;

namespace PdfBox.Net.Tests;

public class RuntimeParityKnownFailuresTest
{
    [Fact]
    public void KnownFailures_DoNotContainClosedIssue438EncryptionDiagnostics()
    {
        Assert.Empty(KnownFailureIdsForOwner("issue-438"));
    }

    [Fact]
    public void KnownFailures_DoNotContainClosedIssue492ImageMaskRenderBucket()
    {
        Assert.Empty(KnownFailureIdsForOwner("issue-492"));
    }

    [Fact]
    public void KnownFailures_DoNotContainClosedIssue493PatternTransparencyBucket()
    {
        Assert.Empty(KnownFailureIdsForOwner("issue-493"));
    }

    [Fact]
    public void KnownFailures_AreEmptyAfterIssue441ZeroKnownGate()
    {
        string path = FindRepoFile("tools/parity/runtime/known-failures.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        Assert.Empty(document.RootElement.GetProperty("entries").EnumerateArray());
    }

    [Fact]
    public void KnownFailures_HaveRoadmapMetadata()
    {
        string path = FindRepoFile("tools/parity/runtime/known-failures.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        string[] requiredStringProperties = ["id", "op", "owner", "rootCause", "reason", "expiresWhen", "ratchet"];
        List<string> failures = [];

        foreach (JsonElement entry in document.RootElement.GetProperty("entries").EnumerateArray())
        {
            string id = entry.TryGetProperty("id", out JsonElement idElement)
                ? idElement.GetString() ?? "<empty>"
                : "<missing-id>";

            foreach (string property in requiredStringProperties)
            {
                if (!entry.TryGetProperty(property, out JsonElement value)
                    || value.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(value.GetString()))
                {
                    failures.Add($"{id}: missing {property}");
                }
            }

            if (!entry.TryGetProperty("issue", out JsonElement issueElement)
                || issueElement.ValueKind != JsonValueKind.Number
                || !issueElement.TryGetInt32(out int issue)
                || issue <= 0)
            {
                failures.Add($"{id}: missing positive issue");
                continue;
            }

            if (entry.TryGetProperty("owner", out JsonElement ownerElement)
                && ownerElement.GetString() != $"issue-{issue}")
            {
                failures.Add($"{id}: owner must match issue");
            }
        }

        Assert.Empty(failures);
    }

    private static string[] KnownFailureIdsForOwner(string ownerName)
    {
        string path = FindRepoFile("tools/parity/runtime/known-failures.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement.GetProperty("entries").EnumerateArray()
            .Where(entry => entry.TryGetProperty("owner", out JsonElement owner)
                && owner.GetString() == ownerName)
            .Select(entry => entry.GetProperty("id").GetString() ?? string.Empty)
            .Where(id => id.Length != 0)
            .ToArray();
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
