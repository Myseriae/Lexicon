namespace LexiconTest;

/// <summary>
/// Stub implementation of IWikipediaService for integration tests.
/// Returns consistent test data without making real HTTP calls to Wikipedia.
/// </summary>
public class StubWikipediaService : Lexicon.Services.IWikipediaService
{
    public Task<string?> GetSummaryAsync(string title)
    {
        // Return a consistent stub summary for all titles
        var summary = $"Test summary for '{title}'. This is a stub implementation used during integration testing.\nSummaries may not be correct as they are received from outside sources.";
        return Task.FromResult<string?>(summary);
    }
}

