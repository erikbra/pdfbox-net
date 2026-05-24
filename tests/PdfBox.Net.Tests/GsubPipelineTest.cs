using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.TTF.GSub;
using PdfBox.Net.FontBox.TTF.Model;

namespace PdfBox.Net.Tests;

public class GsubPipelineTest
{
    [Fact]
    public void MapBackedScriptFeatureShouldMatchGlyphListsByValue()
    {
        MapBackedScriptFeature scriptFeature = new("liga", new Dictionary<IList<int>, IList<int>>
        {
            [new List<int> { 10, 11 }] = new List<int> { 42 }
        });

        Assert.True(scriptFeature.CanReplaceGlyphs(new List<int> { 10, 11 }));
        Assert.Equal([42], scriptFeature.GetReplacementForGlyphs(new List<int> { 10, 11 }));
    }

    [Fact]
    public void TamilWorkerShouldAdjustRephPositionWithoutFeatures()
    {
        MapBackedGsubData gsubData = new(Language.Tamil, "taml",
            new Dictionary<string, Dictionary<IList<int>, IList<int>>>());
        CmapLookup cmapLookup = new TestCmapLookup(new Dictionary<int, int>
        {
            ['\u0BB0'] = 1, // Tamil letter RA
            ['\u0BCD'] = 2, // Tamil sign VIRAMA
            ['\u0BB8'] = 4,
            ['\u0ABF'] = 5
        });

        IGsubWorker worker = new GsubWorkerFactory().GetGsubWorker(cmapLookup, gsubData);

        IList<int> transformed = worker.ApplyTransforms(new List<int> { 1, 2, 3 });

        Assert.Equal([3, 1, 2], transformed);
    }

    private sealed class TestCmapLookup : CmapLookup
    {
        private readonly Dictionary<int, int> _codePointToGlyphId;

        public TestCmapLookup(Dictionary<int, int> codePointToGlyphId)
        {
            _codePointToGlyphId = codePointToGlyphId;
        }

        public int GetGlyphId(int codePoint)
        {
            return _codePointToGlyphId.TryGetValue(codePoint, out int glyphId) ? glyphId : -1;
        }

        public List<int>? GetCharCodes(int gid)
        {
            return null;
        }
    }
}
