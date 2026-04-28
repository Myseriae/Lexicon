namespace Lexicon.Services;

public interface IWikipediaService
{
    Task<string?> GetSummaryAsync(string title);
}