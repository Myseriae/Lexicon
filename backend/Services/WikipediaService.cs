using System.Net.Http.Json;

namespace Lexicon.Services;

public class WikipediaService : IWikipediaService
{
    private readonly HttpClient _httpClient;

    public WikipediaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetSummaryAsync(string title)
    {
        var url =
            $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadFromJsonAsync<WikipediaResponse>();

            // Ignore disambiguation pages
            if (data?.Type == "disambiguation")
                return null;

            return data?.Extract;
        }
        catch
        {
            return null;
        }
    }

    private class WikipediaResponse
    {
        public string? Extract { get; set; }
        public string? Type { get; set; }
    }
}